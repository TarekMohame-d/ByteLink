using Microsoft.Extensions.Logging;
using Shared.Kernel.Messaging;
using Shared.Kernel.ResultPattern;
using ZiggyCreatures.Caching.Fusion;

namespace Shared.Infrastructure.Decorator;

internal sealed class CachingCommandInvalidationDecorator<TCommand>(
    ICommandHandler<TCommand> innerHandler,
    IFusionCache cache,
    ILogger<CachingCommandInvalidationDecorator<TCommand>> logger
) : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    public async Task<Result> Handle(TCommand command, CancellationToken ct)
    {
        var result = await innerHandler.Handle(command, ct);

        if (result.IsSuccess && command is ICacheInvalidator invalidator)
        {
            await CacheEvictionHelper.EvictAsync(invalidator, cache, logger, ct);
        }

        return result;
    }
}

internal sealed class CommandInvalidationDecorator<TCommand, TResponse>(
    ICommandHandler<TCommand, TResponse> innerHandler,
    IFusionCache cache,
    ILogger<CommandInvalidationDecorator<TCommand, TResponse>> logger
) : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    public async Task<Result<TResponse>> Handle(TCommand command, CancellationToken ct)
    {
        var result = await innerHandler.Handle(command, ct);

        if (result.IsSuccess && command is ICacheInvalidator invalidator)
        {
            await CacheEvictionHelper.EvictAsync(invalidator, cache, logger, ct);
        }

        return result;
    }
}

internal static class CacheEvictionHelper
{
    public static async Task EvictAsync(
        ICacheInvalidator invalidator,
        IFusionCache cache,
        ILogger logger,
        CancellationToken ct
    )
    {
        if (invalidator.CacheKeys.Length > 0)
        {
            foreach (var key in invalidator.CacheKeys)
            {
                logger.LogInformation("Evicting explicit cache key: '{Key}'", key);
                await cache.RemoveAsync(key, token: ct);
            }
        }

        if (!string.IsNullOrWhiteSpace(invalidator.CacheSetKey))
        {
            logger.LogInformation("Evicting whole tag group: '{SetKey}'", invalidator.CacheSetKey);
            await cache.RemoveByTagAsync(invalidator.CacheSetKey, token: ct);
        }
    }
}
