using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Notifications.Infrastructure.Services;
using Modules.Notifications.Interfaces;
using Shared;
using Shared.Kernel.Settings;

namespace Modules.Notifications;

internal static class DependencyInjection
{
    internal static IServiceCollection AddNotificationsModuleDI(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var emailSettings = configuration.GetSection("EmailSettings").Get<EmailSettings>()!;

        var assembly = typeof(NotificationsModuleExtensions).Assembly;

        services
            .AddFluentEmail(emailSettings.SenderEmail, emailSettings.SenderName)
            .AddSmtpSender(emailSettings.SmtpServer, int.Parse(emailSettings.SmtpPort));

        services.AddSingleton<IFileReader, FileReader>();

        services.AddIntegrationEventHandlersFromAssembly(assembly);

        return services;
    }
}
