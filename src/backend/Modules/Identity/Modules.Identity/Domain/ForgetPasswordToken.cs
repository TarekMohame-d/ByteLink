using Shared.Kernel.Domain;

namespace Modules.Identity.Domain;

public sealed class ForgetPasswordToken : Entity<Guid>
{
    private ForgetPasswordToken() { } // EF

    public string Email { get; private set; } = default!;
    public string TokenHash { get; private set; } = default!;
    public bool IsUsed { get; private set; }
    public DateTimeOffset ExpiresAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? UsedAtUtc { get; private set; }

    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAtUtc;
    public bool IsActive => !IsUsed && !IsExpired;

    public static ForgetPasswordToken Create(
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
            throw new InvalidOperationException("Forget password token already used.");
        if (IsExpired)
            throw new InvalidOperationException("Forget password token expired.");

        IsUsed = true;
        UsedAtUtc = DateTimeOffset.UtcNow;
    }
}
