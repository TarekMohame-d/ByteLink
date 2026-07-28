using System.Net;
using System.Net.Mail;
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

        var smtpClient = new SmtpClient(emailSettings.SmtpServer, int.Parse(emailSettings.SmtpPort))
        {
            EnableSsl = emailSettings.EnableSsl,
            Credentials = string.IsNullOrWhiteSpace(emailSettings.AppPassword)
                ? null
                : new NetworkCredential(emailSettings.SenderEmail, emailSettings.AppPassword),
        };

        services
            .AddFluentEmail(emailSettings.SenderEmail, emailSettings.SenderName)
            .AddSmtpSender(smtpClient);

        services.AddSingleton<IFileReader, FileReader>();
        services.AddIntegrationEventHandlersFromAssembly(assembly);
        services.AddResilientIntegrationEventHandlers();

        return services;
    }
}
