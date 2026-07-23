using DotNetCore.CAP;

namespace Shared.Kernel.Messaging;

public interface IIntegrationEventHandler<in TEvent> : ICapSubscribe
    where TEvent : IIntegrationEvent
{
    Task HandleAsync(TEvent integrationEvent, CancellationToken ct);
}
