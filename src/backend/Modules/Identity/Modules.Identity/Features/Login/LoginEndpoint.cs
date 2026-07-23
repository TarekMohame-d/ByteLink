using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Shared.Constants;
using Shared.Kernel.Messaging;
using Shared.Kernel.ResultPattern;
using Shared.Kernel.Settings;

namespace Modules.Identity.Features.Login;

public static class LoginEndpoint
{
    public static void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPost("/login", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        LoginRequest request,
        ICommandHandler<LoginCommand, LoginResponse> handler,
        HttpContext httpContext,
        IOptions<JwtSettings> options,
        CancellationToken ct
    )
    {
        var jwtSettings = options.Value;

        var deviceId = httpContext.Request.Headers[HeaderKeys.DeviceId].ToString();
        var deviceMetadata = httpContext.Request.Headers[HeaderKeys.DeviceMetadata].ToString();

        if (string.IsNullOrEmpty(deviceId))
        {
            return Results.BadRequest(new { Error = "Device identification headers are missing." });
        }

        var command = new LoginCommand(request.Email, request.Password, deviceId, deviceMetadata);
        var result = await handler.Handle(command, ct);

        if (result.IsSuccess)
        {
            var isSecure = jwtSettings.RequireHttpsMetadata;

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = isSecure,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(14),
            };

            var accessToken = result.Value.AccessToken;
            var rawRefreshToken = result.Value.RefreshToken;

            httpContext.Response.Cookies.Append(CookieKeys.AccessToken, accessToken, cookieOptions);
            httpContext.Response.Cookies.Append(CookieKeys.RefreshToken, rawRefreshToken, cookieOptions);

            return Results.Ok(result.Value);
        }

        return result.ToProblemDetails();
    }
}
