using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Extensions;
using Shared.Kernel.Messaging;
using Shared.Kernel.ResultPattern;

namespace Modules.Identity.Features.UserData;

public static class UserDataEndpoint
{
    public static void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapGet("/me", HandleAsync).RequireAuthorization();
    }

    private static async Task<IResult> HandleAsync(
        ClaimsPrincipal user,
        IQueryHandler<UserDataQuery, UserData> handler,
        CancellationToken ct
    )
    {
        var userId = user.GetUserId();

        if (!userId.HasValue)
            return Results.Unauthorized();

        var query = new UserDataQuery(userId.Value);
        var result = await handler.Handle(query, ct);
        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
    }
}
