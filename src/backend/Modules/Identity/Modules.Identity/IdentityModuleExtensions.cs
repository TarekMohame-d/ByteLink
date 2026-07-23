using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Identity.Features.ForgetPassword;
using Modules.Identity.Features.Login;
using Modules.Identity.Features.Logout;
using Modules.Identity.Features.RefreshToken;
using Modules.Identity.Features.Register;
using Modules.Identity.Features.ResendEmailVerification;
using Modules.Identity.Features.ResetPassword;
using Modules.Identity.Features.UserData;
using Modules.Identity.Features.VerifyEmail;

namespace Modules.Identity;

public static class IdentityModuleExtensions
{
    public static IServiceCollection AddIdentityModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddIdentityModuleDI(configuration);

        return services;
    }

    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/identity").WithTags("Identity");

        LoginEndpoint.MapEndpoint(group);
        LogoutEndpoint.MapEndpoint(group);
        RegisterEndpoint.MapEndpoint(group);
        ResendEmailVerificationEndpoint.MapEndpoint(group);
        VerifyEmailEndpoint.MapEndpoint(group);
        UserDataEndpoint.MapEndpoint(group);
        RefreshTokenEndpoint.MapEndpoint(group);
        ForgetPasswordEndpoint.MapEndpoint(group);
        ResetPasswordEndpoint.MapEndpoint(group);

        return app;
    }

    // public static IApplicationBuilder UseIdentityModuleBackgroundJobs(
    //     this WebApplication app,
    //     IWebHostEnvironment environment
    // )
    // {
    //     if (environment.IsEnvironment("Testing"))
    //         return app;

    //     IRecurringJobManager recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();

    //     recurringJobManager.AddOrUpdate<ProcessIdentityOutboxJob>(
    //         "identity-outbox-processor",
    //         job => job.ProcessPendingMessagesAsync(),
    //         "*/15 * * * * *" // Run every 15 seconds
    //     );

    //     return app;
    // }
}
