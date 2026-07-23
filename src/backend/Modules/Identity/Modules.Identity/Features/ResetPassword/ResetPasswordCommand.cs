using Shared.Kernel.Messaging;

namespace Modules.Identity.Features.ResetPassword;

public record ResetPasswordCommand(string Password, string Token) : ICommand;
