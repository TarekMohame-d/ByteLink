using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Shared.Infrastructure.Messaging;

namespace Shared.Infrastructure.Decorator;

public sealed class ResilientIntegrationEventHandler<TEvent>(
    IIntegrationEventHandler<TEvent> innerHandler,
    ILogger<ResilientIntegrationEventHandler<TEvent>> logger
) : IIntegrationEventHandler<TEvent>
    where TEvent : class, IIntegrationEvent
{
    private readonly ResiliencePipeline _pipeline = new ResiliencePipelineBuilder()
        .AddRetry(
            new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<Exception>(ex =>
                    ex is not InvalidOperationException
                ),
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(100),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                OnRetry = args =>
                {
                    logger.LogWarning(
                        args.Outcome.Exception,
                        "Transient failure on attempt {AttemptNumber}. Retrying in {RetryDelay}...",
                        args.AttemptNumber + 1,
                        args.RetryDelay
                    );
                    return ValueTask.CompletedTask;
                },
            }
        )
        .Build();

    public Task HandleAsync(TEvent integrationEvent, CancellationToken ct = default)
    {
        return _pipeline
            .ExecuteAsync(async token => await innerHandler.HandleAsync(integrationEvent, token), ct)
            .AsTask();
    }
}
