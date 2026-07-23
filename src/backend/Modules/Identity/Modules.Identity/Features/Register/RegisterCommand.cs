using Shared.Kernel.Messaging;

namespace Modules.Identity.Features.Register;

public record RegisterCommand(string FirstName, string LastName, string Email, string Password)
    : ITransactionalCommand<Guid>;
