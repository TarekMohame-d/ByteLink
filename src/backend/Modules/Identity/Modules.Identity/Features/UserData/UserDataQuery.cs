using Modules.Identity.Domain;
using Shared.Constants;
using Shared.Kernel.Messaging;

namespace Modules.Identity.Features.UserData;

public record UserDataQuery(Guid UserId) : ICachedQuery<UserData>
{
    public string CacheKey => CacheKeys.UserData(UserId.ToString());

    public string? CacheSetKey => null;

    public TimeSpan Expiration => TimeSpan.FromHours(1);
}

public record UserData(
    string FirstName,
    string LastName,
    string Email,
    UserRole UserRole,
    IEnumerable<string> Permissions
);
