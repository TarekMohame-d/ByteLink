using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Messaging;

namespace Shared.Infrastructure.Dispatchers;

public sealed class IntegrationEventDispatcher(IServiceProvider serviceProvider) : IIntegrationEventDispatcher
{
    private static readonly ConcurrentDictionary<Type, IntegrationEventHandlerWrapper> HandlerWrappersCache =
        new();

    public Task DispatchAsync<TEvent>(TEvent integrationEvent, CancellationToken ct = default)
        where TEvent : class, IIntegrationEvent
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        Type runtimeEventType = integrationEvent.GetType();

        IntegrationEventHandlerWrapper wrapper = HandlerWrappersCache.GetOrAdd(
            runtimeEventType,
            static type =>
                (IntegrationEventHandlerWrapper)
                    Activator.CreateInstance(
                        typeof(IntegrationEventHandlerWrapperImpl<>).MakeGenericType(type)
                    )!
        );

        return wrapper.HandleAsync(integrationEvent, serviceProvider, ct);
    }
}

internal abstract class IntegrationEventHandlerWrapper
{
    public abstract Task HandleAsync(
        IIntegrationEvent integrationEvent,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken
    );
}

internal sealed class IntegrationEventHandlerWrapperImpl<TEvent> : IntegrationEventHandlerWrapper
    where TEvent : class, IIntegrationEvent
{
    public override async Task HandleAsync(
        IIntegrationEvent integrationEvent,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken
    )
    {
        var handlers = serviceProvider.GetServices<IIntegrationEventHandler<TEvent>>();
        var typedEvent = (TEvent)integrationEvent;

        foreach (var handler in handlers)
        {
            await handler.HandleAsync(typedEvent, cancellationToken);
        }
    }
}
