namespace Modules.Identity.Interfaces;

public interface IPermissionService
{
    Task<HashSet<string>> GetUserPermissionsAsync(Guid userId);
    Task InvalidateUserPermissionsAsync(Guid userId);
}
