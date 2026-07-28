namespace Shared.Infrastructure.Messaging;

public interface IIntegrationEventHandler<in TEvent>
    where TEvent : class, IIntegrationEvent
{
    Task HandleAsync(TEvent integrationEvent, CancellationToken ct);
}
