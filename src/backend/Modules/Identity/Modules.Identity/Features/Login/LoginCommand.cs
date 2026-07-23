using Shared.Kernel.Messaging;

namespace Modules.Identity.Features.Login;

public record LoginRequest(string Email, string Password);

public record LoginCommand(string Email, string Password, string DeviceId, string DeviceMetadata)
    : ICommand<LoginResponse>;

public record LoginResponse(string AccessToken, string RefreshToken);
