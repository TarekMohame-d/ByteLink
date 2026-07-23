namespace Shared.Kernel.Messaging;

public interface ICachedQuery<TResponse> : IQuery<TResponse>
{
    string CacheKey { get; }
    string? CacheSetKey { get; }
    TimeSpan Expiration { get; }
}
