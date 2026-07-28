namespace Shared.Infrastructure.Messaging;

public interface IIntegrationEvent
{
    Guid EventId { get; }
}
