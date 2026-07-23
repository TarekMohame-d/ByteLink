using Shared.Kernel.Domain;

namespace Modules.Identity.Domain;

public sealed class User : Entity<Guid>, IAuditable, ISoftDeletable
{
    private User() { } // EF

    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public UserRole UserRole { get; private set; }
    public bool EmailVerified { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    public static User Create(
        string firstName,
        string lastName,
        string email,
        string passwordHash,
        UserRole userRole
    )
    {
        return new()
        {
            Id = Guid.CreateVersion7(),
            FirstName = firstName,
            LastName = lastName,
            Email = email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            UserRole = userRole,
            IsActive = false,
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };
    }

    public void ChangePassword(string passwordHash) => PasswordHash = passwordHash;

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;

    public void Delete() => IsDeleted = true;

    public void VerifyEmail() => EmailVerified = true;
}
