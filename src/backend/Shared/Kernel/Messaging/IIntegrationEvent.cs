namespace Shared.Kernel.Messaging;

public interface IIntegrationEvent
{
    Guid EventId { get; }
}
