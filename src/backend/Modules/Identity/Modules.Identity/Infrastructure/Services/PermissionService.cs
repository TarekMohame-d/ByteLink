using Microsoft.EntityFrameworkCore;
using Modules.Identity.Infrastructure.Persistence;
using Modules.Identity.Interfaces;
using ZiggyCreatures.Caching.Fusion;

namespace Modules.Identity.Infrastructure.Services;

public class PermissionService : IPermissionService
{
    private readonly IFusionCache _cache;
    private readonly IdentityDbContext _context;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    public PermissionService(IFusionCache cache, IdentityDbContext context)
    {
        _cache = cache;
        _context = context;
    }

    public async Task<HashSet<string>> GetUserPermissionsAsync(Guid userId)
    {
        string cacheKey = $"user-permissions:{userId}";

        return await _cache.GetOrSetAsync(
            cacheKey,
            async _ =>
            {
                string sql =
                    @"
                    SELECT DISTINCT p.name
                    FROM identity.users u
                    JOIN identity.role_permissions rp ON u.user_role = rp.role_id
                    JOIN identity.permissions p ON rp.permission_id = p.id
                    WHERE u.id = {0}";

                var freshPermissions = await _context.Database.SqlQueryRaw<string>(sql, userId).ToListAsync();

                return freshPermissions.ToHashSet();
            },
            options => options.SetDuration(CacheDuration)
        )!;
    }

    public async Task InvalidateUserPermissionsAsync(Guid userId)
    {
        await _cache.RemoveAsync($"user-permissions:{userId}");
    }
}
