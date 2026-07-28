using Shared.Infrastructure.Messaging;

namespace Modules.Identity.IntegrationEvents;

public record ForgetPasswordIntegrationEvent(string Email, string Token, string TokenExpiresAt)
    : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.CreateVersion7();
}
