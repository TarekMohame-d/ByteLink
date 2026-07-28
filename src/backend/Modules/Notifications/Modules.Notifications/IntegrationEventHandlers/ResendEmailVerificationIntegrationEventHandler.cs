using FluentEmail.Core;
using Microsoft.Extensions.Logging;
using Modules.Identity.IntegrationEvents;
using Modules.Notifications.Interfaces;
using Shared.Infrastructure.Messaging;

namespace Modules.Notifications.IntegrationEventHandlers;

public class ResendEmailVerificationIntegrationEventHandler(
    IFileReader fileReader,
    IFluentEmail fluentEmail,
    IInboxStore inboxStore,
    ILogger<ResendEmailVerificationIntegrationEventHandler> logger
) : IIntegrationEventHandler<ResendEmailVerificationIntegrationEvent>
{
    public async Task HandleAsync(
        ResendEmailVerificationIntegrationEvent integrationEvent,
        CancellationToken ct
    )
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

        var verificationLink =
            $"{frontendUrl}/auth/verify-email?email={integrationEvent.Email}token={integrationEvent.Token}";

        var html = fileReader
            .ReadFile("EmailVerification.html")
            .Replace("{{Email}}", integrationEvent.Email)
            .Replace("{{VerificationLink}}", verificationLink)
            .Replace("{{ExpiresAt}}", integrationEvent.TokenExpiresAt);

        await fluentEmail
            .To(integrationEvent.Email)
            .Subject("Email Verification for ByteLink Account")
            .Body(html, isHtml: true)
            .SendAsync(ct);

        await inboxStore.MarkAsProcessedAsync(integrationEvent.EventId, consumerName, ct);
    }
}
