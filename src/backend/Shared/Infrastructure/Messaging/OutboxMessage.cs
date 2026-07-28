namespace Shared.Infrastructure.Messaging;

public sealed class OutboxMessage
{
    public Guid EventId { get; private set; }
    public string Type { get; private set; } = default!;
    public string Payload { get; private set; } = default!;
    public DateTimeOffset OccurredAtUtc { get; private set; }
    public DateTimeOffset? ProcessedAtUtc { get; private set; }
    public string? Error { get; private set; }
    public int RetryCount { get; private set; }
    public bool DeadLetter { get; private set; }

    private OutboxMessage() { }

    public OutboxMessage(Guid id, string type, string payload, DateTimeOffset occurredAtUtc)
    {
        EventId = id;
        Type = type;
        Payload = payload;
        OccurredAtUtc = occurredAtUtc;
        RetryCount = 0;
        ProcessedAtUtc = null;
        Error = null;
        DeadLetter = false;
    }

    public void MarkProcessed()
    {
        ProcessedAtUtc = DateTimeOffset.UtcNow;
        Error = null;
    }

    public void HandleFailure(string error, int maxRetries = 3)
    {
        RetryCount++;
        Error = $"Failed after {maxRetries} attempts. error: {error}";
        if (RetryCount < maxRetries)
            return;
        // Stop retrying, mark as "Dead Letter"
        ProcessedAtUtc = DateTimeOffset.UtcNow;
        DeadLetter = true;
        Error = $"Dead letter after {maxRetries} attempts. error: {error}";
    }
}
