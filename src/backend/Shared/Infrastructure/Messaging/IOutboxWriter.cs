using Microsoft.EntityFrameworkCore;

namespace Shared.Infrastructure.Messaging;

public interface IOutboxWriter<TContext>
    where TContext : DbContext
{
    void Write<TEvent>(TEvent integrationEvent)
        where TEvent : class, IIntegrationEvent;
}
