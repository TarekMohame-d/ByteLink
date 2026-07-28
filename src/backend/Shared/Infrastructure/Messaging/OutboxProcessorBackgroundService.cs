using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Infrastructure.Dispatchers;
using Shared.Infrastructure.Persistence;

namespace Shared.Infrastructure.Messaging;

public sealed class OutboxProcessorBackgroundService(
    OutboxSignalChannel signalChannel,
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxProcessorBackgroundService> logger
) : BackgroundService
{
    private const int BatchSize = 20;
    private const int MaxRetries = 5;
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(10);
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _ = StartPeriodicTimerAsync(stoppingToken);

        while (await signalChannel.Reader.WaitToReadAsync(stoppingToken))
        {
            while (signalChannel.Reader.TryRead(out _))
            {
                await ProcessAllPendingOutboxMessagesAsync(stoppingToken);
            }
        }
    }

    private async Task StartPeriodicTimerAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollingInterval);
        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            signalChannel.Signal();
        }
    }

    private async Task ProcessAllPendingOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        bool hasMore;
        do
        {
            hasMore = await ProcessBatchAsync(cancellationToken);
        } while (hasMore && !cancellationToken.IsCancellationRequested);
    }

    private async Task<bool> ProcessBatchAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("Processing outbox messages {date}", DateTime.UtcNow);

        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MessagingDbContext>();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IIntegrationEventDispatcher>();

        var messages = await dbContext
            .OutboxMessages.FromSqlInterpolated(
                $"""
                SELECT *
                FROM messaging.outbox_messages
                WHERE processed_at_utc IS NULL AND NOT dead_letter
                ORDER BY occurred_at_utc
                LIMIT {BatchSize}
                FOR UPDATE SKIP LOCKED
                """
            )
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
            return false;

        foreach (var message in messages)
        {
            try
            {
                var eventType = Type.GetType(message.Type);
                if (eventType is null)
                {
                    logger.LogError(
                        "Failed to resolve outbox event type: {Type} for Message {Id}",
                        message.Type,
                        message.EventId
                    );
                    message.HandleFailure($"Failed to resolve event type: {message.Type}", MaxRetries);
                    continue;
                }

                var deserializedObject = JsonSerializer.Deserialize(
                    message.Payload,
                    eventType,
                    SerializerOptions
                );
                if (deserializedObject is not IIntegrationEvent integrationEvent)
                {
                    logger.LogError(
                        "Failed to deserialize payload or payload is not an IIntegrationEvent for message {Id}",
                        message.EventId
                    );
                    message.HandleFailure(
                        $"Payload deserialization failed or missing {nameof(IIntegrationEvent)} implementation",
                        MaxRetries
                    );
                    continue;
                }

                await dispatcher.DispatchAsync(integrationEvent, cancellationToken);

                message.MarkProcessed();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing outbox message {Id}", message.EventId);
                message.HandleFailure(ex.Message, MaxRetries);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return messages.Count == BatchSize;
    }
}
