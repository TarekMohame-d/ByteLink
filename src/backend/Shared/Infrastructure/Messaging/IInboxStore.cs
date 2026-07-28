using Microsoft.EntityFrameworkCore;

namespace Shared.Infrastructure.Messaging;

public interface IInboxStore
{
    Task<bool> HasBeenProcessedAsync(Guid eventId, string consumerName, CancellationToken ct = default);
    Task MarkAsProcessedAsync(Guid eventId, string consumerName, CancellationToken ct = default);
}

public interface IInboxStore<TContext>
    where TContext : DbContext
{
    Task<bool> HasBeenProcessedAsync(Guid eventId, string consumerName, CancellationToken ct = default);
    void MarkAsProcessed(Guid eventId, string consumerName);
}
