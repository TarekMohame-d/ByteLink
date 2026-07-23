using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Modules.Identity.Domain;
using Modules.Identity.Infrastructure.Persistence;
using Modules.Identity.Interfaces;
using Shared.Extensions;
using Shared.Kernel.Settings;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

namespace Modules.Identity.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IdentityDbContext _context;
    private readonly ISecureHasher _hasher;
    private readonly IPermissionService _permissionService;

    public TokenService(
        IOptions<JwtSettings> jwtOptions,
        IdentityDbContext context,
        ISecureHasher hasher,
        IPermissionService permissionService
    )
    {
        _jwtSettings = jwtOptions.Value;
        _context = context;
        _hasher = hasher;
        _permissionService = permissionService;
    }

    public string GenerateAccessToken(Guid userId, string email)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);

        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    public async Task<(string AccessToken, string RefreshToken)?> RotateTokensAsync(
        string expiredAccessToken,
        string refreshToken,
        string deviceId,
        string deviceMetadata
    )
    {
        var tokenHandler = new JsonWebTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
            ValidateLifetime = false,
        };

        var validationResult = await tokenHandler.ValidateTokenAsync(
            expiredAccessToken,
            validationParameters
        );

        if (!validationResult.IsValid)
            return null;

        var principal = new ClaimsPrincipal(validationResult.ClaimsIdentity);
        var userId = principal.GetUserId();

        if (!userId.HasValue)
            return null;

        string hashedIncomingToken = _hasher.HashToken(refreshToken);

        var storedRefreshToken = await _context.UserRefreshTokens.FirstOrDefaultAsync(t =>
            t.TokenHash == hashedIncomingToken && t.UserId == userId && t.DeviceId == deviceId
        );

        if (storedRefreshToken != null && storedRefreshToken.IsRevoked)
        {
            var activeDeviceTokens = await _context
                .UserRefreshTokens.Where(t =>
                    t.UserId == userId
                    && t.DeviceId == deviceId
                    && !t.IsRevoked
                    && t.ExpiresAtUtc > DateTimeOffset.UtcNow
                )
                .ToListAsync();

            foreach (var token in activeDeviceTokens)
            {
                token.Revoke();
            }

            await _context.SaveChangesAsync();
            return null;
        }

        if (storedRefreshToken == null || storedRefreshToken.IsExpired)
        {
            return null;
        }

        storedRefreshToken.Revoke();

        var email = principal.GetEmail();
        var newAccessToken = GenerateAccessToken(userId.Value, email ?? "");
        var newRefreshTokenString = GenerateRefreshToken();

        var newRefreshTokenEntity = UserRefreshToken.Create(
            userId.Value,
            _hasher.HashToken(newRefreshTokenString),
            deviceId,
            deviceMetadata
        );

        _context.UserRefreshTokens.Add(newRefreshTokenEntity);
        await _context.SaveChangesAsync();

        return (newAccessToken, newRefreshTokenString);
    }
}
