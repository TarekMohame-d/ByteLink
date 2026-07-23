using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Constants;
using Shared.Kernel.Messaging;

namespace Modules.Identity.Features.Logout;

public static class LogoutEndpoint
{
    public static void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPost("/logout", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        ICommandHandler<LogoutCommand> handler,
        HttpContext httpContext,
        CancellationToken ct
    )
    {
        var refreshToken = httpContext.Request.Cookies[CookieKeys.RefreshToken];

        if (!string.IsNullOrEmpty(refreshToken))
        {
            await handler.Handle(new LogoutCommand(refreshToken), ct);
        }

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(-1),
        };

        httpContext.Response.Cookies.Delete(CookieKeys.AccessToken, cookieOptions);
        httpContext.Response.Cookies.Delete(CookieKeys.RefreshToken, cookieOptions);

        return Results.NoContent();
    }
}
