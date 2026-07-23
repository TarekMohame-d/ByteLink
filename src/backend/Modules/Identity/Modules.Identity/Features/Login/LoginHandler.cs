using Microsoft.EntityFrameworkCore;
using Modules.Identity.Domain;
using Modules.Identity.Infrastructure.Persistence;
using Modules.Identity.Interfaces;
using Shared.Kernel.Messaging;
using Shared.Kernel.ResultPattern;

namespace Modules.Identity.Features.Login;

internal sealed class LoginHandler(
    IdentityDbContext dbContext,
    ISecureHasher secureHasher,
    ITokenService tokenService
) : ICommandHandler<LoginCommand, LoginResponse>
{
    public async Task<Result<LoginResponse>> Handle(LoginCommand command, CancellationToken ct)
    {
        var email = command.Email.ToLowerInvariant();

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null || !secureHasher.VerifyPassword(command.Password, user.PasswordHash))
            return Result.Failure<LoginResponse>(
                Error.Unauthorized("InvalidCredentials", "Invalid email or password.")
            );

        if (!user.EmailVerified)
            return Result.Failure<LoginResponse>(
                Error.Unauthorized("EmailNotVerified", "Email not verified.")
            );

        var accessToken = tokenService.GenerateAccessToken(user.Id, user.Email);
        var rawRefreshToken = tokenService.GenerateRefreshToken();

        var refreshTokenEntity = UserRefreshToken.Create(
            user.Id,
            secureHasher.HashToken(rawRefreshToken),
            command.DeviceId,
            command.DeviceMetadata
        );

        dbContext.UserRefreshTokens.Add(refreshTokenEntity);
        await dbContext.SaveChangesAsync();

        return new LoginResponse(accessToken, rawRefreshToken);
    }
}
