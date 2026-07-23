using Shared.Kernel.Messaging;

namespace Modules.Identity.Features.ForgetPassword;

public record ForgetPasswordCommand(string Email) : ITransactionalCommand;
