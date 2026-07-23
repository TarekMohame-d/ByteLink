namespace Shared.Kernel.Messaging;

public interface ICacheInvalidator : ICommand
{
    string[] CacheKeys { get; }
    string? CacheSetKey { get; }
}

public interface ICacheInvalidator<TResponse> : ICommand<TResponse>
{
    string[] CacheKeys { get; }
    string? CacheSetKey { get; }
}
