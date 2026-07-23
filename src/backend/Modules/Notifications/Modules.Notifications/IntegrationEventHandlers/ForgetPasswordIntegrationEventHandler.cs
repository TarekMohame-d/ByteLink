using DotNetCore.CAP;
using FluentEmail.Core;
using Modules.Identity.IntegrationEvents;
using Modules.Notifications.Interfaces;
using Shared.Kernel.Messaging;

namespace Modules.Notifications.IntegrationEventHandlers;

public class ForgetPasswordIntegrationEventHandler(IFileReader fileReader, IFluentEmail fluentEmail)
    : IIntegrationEventHandler<ForgetPasswordIntegrationEvent>
{
    [CapSubscribe(nameof(ForgetPasswordIntegrationEvent))]
    public async Task HandleAsync(ForgetPasswordIntegrationEvent integrationEvent, CancellationToken ct)
    {
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
    }
}
