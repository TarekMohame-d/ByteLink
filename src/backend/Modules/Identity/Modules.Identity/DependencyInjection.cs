using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Modules.Identity.Infrastructure.Auth;
using Modules.Identity.Infrastructure.Persistence;
using Modules.Identity.Infrastructure.Services;
using Modules.Identity.Interfaces;
using Shared;
using Shared.Kernel.Settings;

namespace Modules.Identity;

internal static class DependencyInjection
{
    internal static IServiceCollection AddIdentityModuleDI(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        if (!configuration.GetValue<bool>("IntegrationTests"))
        {
            services.AddCustomAuthentication(configuration);
            services.AddAuthorization();
        }

        services.AddIdentityDbContext(configuration);

        services.AddScoped<ITokenService, TokenService>();
        services.AddSingleton<ISecureHasher, SecureHasher>();
        services.AddSingleton<ISecureGenerator, SecureGenerator>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        var asm = typeof(DependencyInjection).Assembly;

        services.AddHandlersFromAssembly(asm);
        services.AddIntegrationEventHandlersFromAssembly(asm);
        services.AddResilientIntegrationEventHandlers();

        services.AddValidatorsFromAssembly(asm, ServiceLifetime.Scoped, includeInternalTypes: true);

        return services;
    }

    private static IServiceCollection AddIdentityDbContext(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddDbContext<IdentityDbContext>(opt =>
        {
            opt.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
                .UseSnakeCaseNamingConvention();
        });

        return services;
    }

    private static IServiceCollection AddCustomAuthentication(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()!;

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;

                options.RequireHttpsMetadata = jwtSettings.RequireHttpsMetadata;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Cookies.TryGetValue("X-Access-Token", out var token))
                        {
                            context.Token = token;
                        }
                        return Task.CompletedTask;
                    },
                };
            });

        return services;
    }
}
