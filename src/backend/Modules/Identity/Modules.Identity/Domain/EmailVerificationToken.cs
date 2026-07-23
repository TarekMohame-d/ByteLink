using Shared.Kernel.Domain;

namespace Modules.Identity.Domain;

public sealed class EmailVerificationToken : Entity<Guid>
{
    private EmailVerificationToken() { } // EF

    public string Email { get; private set; } = default!;
    public string TokenHash { get; private set; } = default!;
    public bool IsUsed { get; private set; }
    public DateTimeOffset ExpiresAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? UsedAtUtc { get; private set; }

    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAtUtc;
    public bool IsActive => !IsUsed && !IsExpired;

    public static EmailVerificationToken Create(
        string email,
        string tokenHash,
        DateTimeOffset? expiresAtUtc = null
    )
    {
        return new()
        {
            Id = Guid.CreateVersion7(),
            Email = email,
            TokenHash = tokenHash,
            IsUsed = false,
            ExpiresAtUtc = expiresAtUtc ?? DateTimeOffset.UtcNow.AddMinutes(30),
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };
    }

    public void MarkAsUsed()
    {
        if (IsUsed)
            throw new InvalidOperationException("Email verification token already used.");
        if (IsExpired)
            throw new InvalidOperationException("Email verification token expired.");

        IsUsed = true;
        UsedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Renew(string tokenHash, DateTimeOffset? expiresAtUtc = null)
    {
        if (IsUsed)
            throw new InvalidOperationException("Email verification token already used.");

        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc ?? DateTimeOffset.UtcNow.AddMinutes(30);
        IsUsed = false;
    }
}
