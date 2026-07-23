using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Shared.Constants;
using Shared.Kernel.Messaging;
using Shared.Kernel.ResultPattern;
using Shared.Kernel.Settings;

namespace Modules.Identity.Features.RefreshToken;

public static class RefreshTokenEndpoint
{
    public static void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPost("/refresh-token", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        ICommandHandler<RefreshTokenCommand, RefreshTokenResponse> handler,
        IOptions<JwtSettings> options,
        CancellationToken ct
    )
    {
        var jwtSettings = options.Value;
        var accessToken = httpContext.Request.Cookies[CookieKeys.AccessToken] ?? string.Empty;
        var refreshToken = httpContext.Request.Cookies[CookieKeys.RefreshToken];

        var deviceId = httpContext.Request.Headers[HeaderKeys.DeviceId].ToString();
        var deviceMetadata = httpContext.Request.Headers[HeaderKeys.DeviceMetadata].ToString();

        if (string.IsNullOrWhiteSpace(refreshToken) || string.IsNullOrWhiteSpace(deviceId))
            return Results.Unauthorized();

        var command = new RefreshTokenCommand(accessToken, refreshToken, deviceId, deviceMetadata);
        var result = await handler.Handle(command, ct);

        var isSecure = jwtSettings.RequireHttpsMetadata;

        if (result.IsSuccess)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = isSecure,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(14),
            };

            httpContext.Response.Cookies.Append(
                CookieKeys.AccessToken,
                result.Value.AccessToken,
                cookieOptions
            );
            httpContext.Response.Cookies.Append(
                CookieKeys.RefreshToken,
                result.Value.RefreshToken,
                cookieOptions
            );

            return Results.Ok(result.Value);
        }

        var deleteOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = isSecure,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(-1),
        };

        httpContext.Response.Cookies.Delete(CookieKeys.AccessToken, deleteOptions);
        httpContext.Response.Cookies.Delete(CookieKeys.RefreshToken, deleteOptions);

        return result.ToProblemDetails();
    }
}
