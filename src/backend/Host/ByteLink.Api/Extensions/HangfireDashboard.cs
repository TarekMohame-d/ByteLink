using ByteLink.Api.Filters;
using Hangfire;

namespace ByteLink.Api.Extensions;

public static class HangfireDashboard
{
    public static IApplicationBuilder UseCustomHangfireDashboard(
        this WebApplication app,
        IConfiguration configuration
    )
    {
        app.UseHangfireDashboard(
            "/hangfire",
            new DashboardOptions
            {
                Authorization =
                [
                    new HangfireBasicAuthenticationFilter(
                        configuration.GetValue<string>("Hangfire_Username")!,
                        configuration.GetValue<string>("Hangfire_Password")!
                    ),
                ],

                DarkModeEnabled = true,
            }
        );

        return app;
    }
}
