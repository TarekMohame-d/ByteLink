using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Decorator;
using Shared.Infrastructure.Dispatchers;
using Shared.Infrastructure.Messaging;
using Shared.Infrastructure.Messaging.Jobs;
using Shared.Infrastructure.Persistence;
using Shared.Kernel.Messaging;
using Shared.Kernel.Settings;

namespace Shared;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.Configure<AdminSettings>(configuration.GetSection("AdminSettings"));
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        services.AddMessagingInfrastructure(configuration);

        return services;
    }

    public static IServiceCollection AddHandlersFromAssembly(
        this IServiceCollection services,
        Assembly assembly
    )
    {
        services.Scan(scan =>
            scan.FromAssemblies(assembly)
                .AddClasses(c => c.AssignableTo(typeof(ICommandHandler<>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithTransientLifetime()
                .AddClasses(c => c.AssignableTo(typeof(ICommandHandler<,>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithTransientLifetime()
                .AddClasses(c => c.AssignableTo(typeof(IQueryHandler<,>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithTransientLifetime()
        );

        return services;
    }

    public static IServiceCollection AddGlobalMessagingDecorators(this IServiceCollection services)
    {
        services.Decorate(typeof(IQueryHandler<,>), typeof(CachingQueryDecorator<,>));
        services.Decorate(typeof(ICommandHandler<>), typeof(CachingCommandInvalidationDecorator<>));
        services.Decorate(typeof(ICommandHandler<,>), typeof(CommandInvalidationDecorator<,>));

        services.Decorate(typeof(ICommandHandler<>), typeof(ValidationCommandHandlerDecorator<>));
        services.Decorate(typeof(ICommandHandler<,>), typeof(ValidationCommandHandlerDecorator<,>));

        services.Decorate(typeof(IQueryHandler<,>), typeof(LoggingQueryHandlerDecorator<,>));
        services.Decorate(typeof(ICommandHandler<>), typeof(LoggingCommandHandlerDecorator<>));
        services.Decorate(typeof(ICommandHandler<,>), typeof(LoggingCommandHandlerDecorator<,>));

        return services;
    }

    public static IServiceCollection AddIntegrationEventHandlersFromAssembly(
        this IServiceCollection services,
        Assembly assembly
    )
    {
        services.AddScoped<IIntegrationEventDispatcher, IntegrationEventDispatcher>();

        services.Scan(scan =>
            scan.FromAssemblies(assembly)
                .AddClasses(c => c.AssignableTo(typeof(IIntegrationEventHandler<>)), publicOnly: false)
                .AsImplementedInterfaces()
                .AsSelf()
                .WithScopedLifetime()
        );

        return services;
    }

    public static IServiceCollection AddResilientIntegrationEventHandlers(this IServiceCollection services)
    {
        bool hasHandlers = services.Any(sd =>
            sd.ServiceType.IsGenericType
            && sd.ServiceType.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>)
        );

        if (hasHandlers)
        {
            services.Decorate(typeof(IIntegrationEventHandler<>), typeof(ResilientIntegrationEventHandler<>));
        }

        return services;
    }

    private static IServiceCollection AddMessagingInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddDbContext<MessagingDbContext>(options =>
            options
                .UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
                .UseSnakeCaseNamingConvention()
        );

        services.AddScoped(typeof(IOutboxWriter<>), typeof(OutboxWriter<>));
        services.AddScoped(typeof(IInboxStore<>), typeof(InboxStore<>));
        services.AddScoped<IInboxStore, InboxStore>();

        services.AddSingleton<OutboxSignalChannel>();

        services.AddHostedService<OutboxProcessorBackgroundService>();

        services.AddScoped<IOutboxInboxCleanupService, OutboxInboxCleanupService>();

        return services;
    }
}
