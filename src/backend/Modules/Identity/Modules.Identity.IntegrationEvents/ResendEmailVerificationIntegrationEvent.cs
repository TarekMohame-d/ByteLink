using DotNetCore.CAP;
using Shared.Kernel.Messaging;

namespace Modules.Identity.IntegrationEvents;

public record ResendEmailVerificationIntegrationEvent(string Email, string Token, string TokenExpiresAt)
    : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.CreateVersion7();
}
