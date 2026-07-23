using Shared.Kernel.Messaging;

namespace Modules.Identity.Features.ResendEmailVerification;

public record ResendEmailVerificationCommand(string Email) : ITransactionalCommand;
