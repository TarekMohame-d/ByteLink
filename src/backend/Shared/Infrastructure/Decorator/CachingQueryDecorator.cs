using Microsoft.Extensions.Logging;
using Shared.Kernel.Messaging;
using Shared.Kernel.ResultPattern;
using ZiggyCreatures.Caching.Fusion;

namespace Shared.Infrastructure.Decorator;

internal sealed class CachingQueryDecorator<TQuery, TResponse>(
    IQueryHandler<TQuery, TResponse> innerHandler,
    IFusionCache cache,
    ILogger<CachingQueryDecorator<TQuery, TResponse>> logger
) : IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    public async Task<Result<TResponse>> Handle(TQuery query, CancellationToken ct)
    {
        if (query is not ICachedQuery<TResponse> cachedQuery)
        {
            return await innerHandler.Handle(query, ct);
        }

        string[] tags = !string.IsNullOrWhiteSpace(cachedQuery.CacheSetKey) ? [cachedQuery.CacheSetKey] : [];

        logger.LogInformation("Cache lookup for key: '{CacheKey}'", cachedQuery.CacheKey);

        try
        {
            var cachedValue = await cache.GetOrSetAsync<TResponse>(
                cachedQuery.CacheKey,
                async (ctx, token) =>
                {
                    var result = await innerHandler.Handle(query, token);

                    if (!result.IsSuccess)
                    {
                        logger.LogWarning(
                            "Query execution failed. Bypassing cache persistence for key: '{CacheKey}'",
                            cachedQuery.CacheKey
                        );

                        throw new CacheMissExecutionException(result);
                    }

                    return result.Value;
                },
                options => options.SetDuration(cachedQuery.Expiration),
                tags: tags,
                token: ct
            );

            return Result.Success(cachedValue);
        }
        catch (CacheMissExecutionException ex)
        {
            return (Result<TResponse>)ex.FailedResult;
        }
    }

    private sealed class CacheMissExecutionException(object failedResult) : Exception
    {
        public object FailedResult { get; } = failedResult;
    }
}
