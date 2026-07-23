using Modules.Identity.Interfaces;
using Shared.Kernel.Messaging;
using Shared.Kernel.ResultPattern;

namespace Modules.Identity.Features.RefreshToken;

internal sealed class RefreshTokenHandler(ITokenService tokenService)
    : ICommandHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand command, CancellationToken ct)
    {
        var result = await tokenService.RotateTokensAsync(
            command.AccessToken,
            command.RefreshToken,
            command.DeviceId,
            command.DeviceMetadata
        );

        if (result == null)
        {
            return Result.Failure<RefreshTokenResponse>(
                Error.Unauthorized("InvalidRefreshToken", "Invalid refresh token.")
            );
        }

        return new RefreshTokenResponse(result.Value.AccessToken, result.Value.RefreshToken);
    }
}
