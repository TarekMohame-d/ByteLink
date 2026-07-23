using Microsoft.AspNetCore.Authorization;
using Modules.Identity.Interfaces;
using Shared.Extensions;

namespace Modules.Identity.Infrastructure.Auth;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IPermissionService _permissionService;

    public PermissionAuthorizationHandler(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement
    )
    {
        var userId = context.User.GetUserId();
        if (!userId.HasValue)
        {
            return;
        }

        var permissions = await _permissionService.GetUserPermissionsAsync(userId.Value);

        if (permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }
    }
}
