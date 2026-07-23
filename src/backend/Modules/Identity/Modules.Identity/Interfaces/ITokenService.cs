namespace Modules.Identity.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(Guid userId, string email);
    string GenerateRefreshToken();
    Task<(string AccessToken, string RefreshToken)?> RotateTokensAsync(
        string expiredAccessToken,
        string refreshToken,
        string deviceId,
        string deviceMetadata
    );
}
