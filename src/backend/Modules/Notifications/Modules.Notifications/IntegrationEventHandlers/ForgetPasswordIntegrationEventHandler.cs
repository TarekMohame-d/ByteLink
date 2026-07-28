using FluentEmail.Core;
using Microsoft.Extensions.Logging;
using Modules.Identity.IntegrationEvents;
using Modules.Notifications.Interfaces;
using Shared.Infrastructure.Messaging;

namespace Modules.Notifications.IntegrationEventHandlers;

public class ForgetPasswordIntegrationEventHandler(
    IFileReader fileReader,
    IFluentEmail fluentEmail,
    IInboxStore inboxStore,
    ILogger<ForgetPasswordIntegrationEventHandler> logger
) : IIntegrationEventHandler<ForgetPasswordIntegrationEvent>
{
    public async Task HandleAsync(ForgetPasswordIntegrationEvent integrationEvent, CancellationToken ct)
    {
        string consumerName = GetType().Name;

        if (await inboxStore.HasBeenProcessedAsync(integrationEvent.EventId, consumerName, ct))
        {
            logger.LogInformation(
                "Event {EventId} was already processed by {ConsumerName}. Skipping email.",
                integrationEvent.EventId,
                consumerName
            );
            return;
        }

        var frontendUrl = Environment.GetEnvironmentVariable("FrontendUrl");
        var resetLink = $"{frontendUrl}/auth/reset-password?token={integrationEvent.Token}";

        var html = fileReader
            .ReadFile("ForgetPassword.html")
            .Replace("{{Email}}", integrationEvent.Email)
            .Replace("{{ResetLink}}", resetLink)
            .Replace("{{ExpiresAt}}", integrationEvent.TokenExpiresAt);

        await fluentEmail
            .To(integrationEvent.Email)
            .Subject("Forget Password for ByteLink Account")
            .Body(html, isHtml: true)
            .SendAsync(ct);

        await inboxStore.MarkAsProcessedAsync(integrationEvent.EventId, consumerName, ct);
    }
}
