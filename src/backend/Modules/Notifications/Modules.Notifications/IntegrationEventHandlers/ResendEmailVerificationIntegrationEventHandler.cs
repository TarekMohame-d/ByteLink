using DotNetCore.CAP;
using FluentEmail.Core;
using Modules.Identity.IntegrationEvents;
using Modules.Notifications.Interfaces;
using Shared.Kernel.Messaging;

namespace Modules.Notifications.IntegrationEventHandlers;

public class ResendEmailVerificationIntegrationEventHandler(IFileReader fileReader, IFluentEmail fluentEmail)
    : IIntegrationEventHandler<ResendEmailVerificationIntegrationEvent>
{
    [CapSubscribe(nameof(ResendEmailVerificationIntegrationEvent), Group = "notifications-module-group")]
    public async Task HandleAsync(
        ResendEmailVerificationIntegrationEvent integrationEvent,
        CancellationToken ct
    )
    {
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
    }
}
