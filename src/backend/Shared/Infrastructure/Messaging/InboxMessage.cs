namespace Shared.Infrastructure.Messaging;

public sealed class InboxMessage
{
    public Guid EventId { get; private set; }
    public string ConsumerName { get; private set; } = default!;
    public DateTimeOffset? ProcessedAtUtc { get; private set; }

    private InboxMessage() { }

    public InboxMessage(Guid eventId, string consumerName)
    {
        EventId = eventId;
        ConsumerName = consumerName;
        ProcessedAtUtc = DateTimeOffset.UtcNow;
    }
}
