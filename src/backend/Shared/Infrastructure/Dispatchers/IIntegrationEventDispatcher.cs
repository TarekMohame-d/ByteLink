using Shared.Infrastructure.Messaging;

namespace Shared.Infrastructure.Dispatchers;

public interface IIntegrationEventDispatcher
{
    Task DispatchAsync<TEvent>(TEvent integrationEvent, CancellationToken ct = default)
        where TEvent : class, IIntegrationEvent;
}
