using Shared.Kernel.Messaging;

namespace Modules.Identity.Features.Logout;

public record LogoutCommand(string RefreshToken) : ICommand;
