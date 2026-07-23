using Microsoft.AspNetCore.Authorization;

namespace Modules.Identity.Infrastructure.Auth;

public class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
