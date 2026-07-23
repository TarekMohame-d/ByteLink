using Microsoft.EntityFrameworkCore;
using Modules.Identity.Infrastructure.Persistence;
using Modules.Identity.Interfaces;
using Shared.Kernel.Messaging;
using Shared.Kernel.ResultPattern;

namespace Modules.Identity.Features.UserData;

internal sealed class UserDataHandler(IdentityDbContext dbContext, IPermissionService permissionService)
    : IQueryHandler<UserDataQuery, UserData>
{
    public async Task<Result<UserData>> Handle(UserDataQuery query, CancellationToken ct)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == query.UserId);

        if (user is null)
            return Result.Failure<UserData>(Error.NotFound("UserNotFound", "User not found."));

        var permissions = await permissionService.GetUserPermissionsAsync(user.Id);

        return Result.Success(
            new UserData(user.FirstName, user.LastName, user.Email, user.UserRole, permissions)
        );
    }
}
