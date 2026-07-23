using Shared.Kernel.Domain;

namespace Modules.Identity.Domain;

public sealed class UserRefreshToken : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public string DeviceId { get; private set; } = string.Empty;
    public string DeviceMetadata { get; private set; } = string.Empty;
    public DateTimeOffset ExpiresAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public bool IsRevoked { get; private set; }

    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAtUtc;
    public bool IsActive => !IsRevoked && !IsExpired;

    public static UserRefreshToken Create(
        Guid userId,
        string tokenHash,
        string deviceId,
        string deviceMetadata,
        DateTimeOffset? expiresAtUtc = null
    ) =>
        new()
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            TokenHash = tokenHash,
            DeviceId = deviceId,
            DeviceMetadata = deviceMetadata,
            ExpiresAtUtc = expiresAtUtc ?? DateTimeOffset.UtcNow.AddDays(14),
            IsRevoked = false,
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };

    public void Revoke() => IsRevoked = true;
}
