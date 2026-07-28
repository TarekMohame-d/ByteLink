using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Infrastructure.Persistence;

namespace Shared.Infrastructure.Messaging.Jobs;

public interface IOutboxInboxCleanupService
{
    Task ProcessCleanupAsync(CancellationToken cancellationToken = default);
}

public sealed class OutboxInboxCleanupService(
    MessagingDbContext dbContext,
    ILogger<OutboxInboxCleanupService> logger
) : IOutboxInboxCleanupService
{
    private const int BatchSize = 2000;
    private static readonly TimeSpan RetentionPeriod = TimeSpan.FromDays(7);

    public async Task ProcessCleanupAsync(CancellationToken cancellationToken = default)
    {
        var cutoffUtc = DateTimeOffset.UtcNow.Subtract(RetentionPeriod);

        logger.LogInformation(
            "Starting Outbox and Inbox cleanup for records older than {CutoffUtc}",
            cutoffUtc
        );

        int totalOutboxDeleted = await CleanupOutboxAsync(cutoffUtc, cancellationToken);
        int totalInboxDeleted = await CleanupInboxAsync(cutoffUtc, cancellationToken);

        logger.LogInformation(
            "Cleanup complete. Removed {OutboxCount} Outbox messages and {InboxCount} Inbox messages.",
            totalOutboxDeleted,
            totalInboxDeleted
        );
    }

    private async Task<int> CleanupOutboxAsync(DateTimeOffset cutoffUtc, CancellationToken cancellationToken)
    {
        int totalDeleted = 0;
        int deletedInBatch;

        do
        {
            deletedInBatch = await dbContext
                .OutboxMessages.Where(m => m.ProcessedAtUtc != null && m.ProcessedAtUtc < cutoffUtc)
                .Take(BatchSize)
                .ExecuteDeleteAsync(cancellationToken);

            totalDeleted += deletedInBatch;
        } while (deletedInBatch == BatchSize && !cancellationToken.IsCancellationRequested);

        return totalDeleted;
    }

    private async Task<int> CleanupInboxAsync(DateTimeOffset cutoffUtc, CancellationToken cancellationToken)
    {
        int totalDeleted = 0;
        int deletedInBatch;

        do
        {
            deletedInBatch = await dbContext
                .InboxMessages.Where(i => i.ProcessedAtUtc < cutoffUtc)
                .Take(BatchSize)
                .ExecuteDeleteAsync(cancellationToken);

            totalDeleted += deletedInBatch;
        } while (deletedInBatch == BatchSize && !cancellationToken.IsCancellationRequested);

        return totalDeleted;
    }
}
