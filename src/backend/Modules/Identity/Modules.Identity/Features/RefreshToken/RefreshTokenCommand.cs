using Shared.Kernel.Messaging;

namespace Modules.Identity.Features.RefreshToken;

public record RefreshTokenCommand(
    string AccessToken,
    string RefreshToken,
    string DeviceId,
    string DeviceMetadata
) : ICommand<RefreshTokenResponse>;

public record RefreshTokenResponse(string AccessToken, string RefreshToken);
