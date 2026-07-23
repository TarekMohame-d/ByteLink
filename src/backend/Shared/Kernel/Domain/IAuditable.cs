namespace Shared.Kernel.Domain;

public interface IAuditable
{
    DateTimeOffset CreatedAtUtc { get; }
    DateTimeOffset? UpdatedAtUtc { get; }
}
