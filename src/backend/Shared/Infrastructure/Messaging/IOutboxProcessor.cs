namespace Shared.Infrastructure.Messaging;

public interface IOutboxProcessor
{
    Task ProcessPendingMessagesAsync();
}
