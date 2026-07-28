using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Persistence;

namespace Shared.Infrastructure.Messaging;

public sealed class InboxStore<TContext>(TContext dbContext) : IInboxStore<TContext>
    where TContext : DbContext
{
    public Task<bool> HasBeenProcessedAsync(Guid messageId, string consumerName, CancellationToken ct) =>
        dbContext
            .Set<InboxMessage>()
            .AnyAsync(x => x.EventId == messageId && x.ConsumerName == consumerName, ct);

    public void MarkAsProcessed(Guid messageId, string consumerName) =>
        dbContext.Set<InboxMessage>().Add(new InboxMessage(messageId, consumerName));
}

public sealed class InboxStore(MessagingDbContext dbContext) : IInboxStore
{
    public Task<bool> HasBeenProcessedAsync(
        Guid messageId,
        string consumerName,
        CancellationToken ct = default
    )
    {
        return dbContext.InboxMessages.AnyAsync(
            m => m.EventId == messageId && m.ConsumerName == consumerName,
            ct
        );
    }

    public async Task MarkAsProcessedAsync(
        Guid messageId,
        string consumerName,
        CancellationToken ct = default
    )
    {
        var inboxMessage = new InboxMessage(messageId, consumerName);

        dbContext.InboxMessages.Add(inboxMessage);
        await dbContext.SaveChangesAsync(ct);
    }
}
