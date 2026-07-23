using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;
using Modules.Identity.Domain;
using Modules.Identity.Infrastructure.Persistence;
using Modules.Identity.IntegrationEvents;
using Modules.Identity.Interfaces;
using Shared.Kernel.Messaging;
using Shared.Kernel.ResultPattern;

namespace Modules.Identity.Features.ForgetPassword;

internal sealed class ForgetPasswordHandler(
    IdentityDbContext dbContext,
    ISecureGenerator secureGenerator,
    ISecureHasher secureHasher,
    ICapPublisher capBus
) : ICommandHandler<ForgetPasswordCommand>
{
    public async Task<Result> Handle(ForgetPasswordCommand command, CancellationToken ct)
    {
        var email = command.Email.ToLowerInvariant();

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user is null)
            return Result.Success();

        var token = secureGenerator.GenerateToken();
        var tokenHash = secureHasher.HashToken(token);

        var forgetPassword = ForgetPasswordToken.Create(email, tokenHash);

        TimeSpan remaining = forgetPassword.ExpiresAtUtc - DateTimeOffset.UtcNow;
        int minutes = (int)Math.Ceiling(remaining.TotalMinutes);
        string expiresIn = $"{minutes}";

        var integrationEvent = new ForgetPasswordIntegrationEvent(email, token, expiresIn);

        dbContext.ForgetPasswordTokens.Add(forgetPassword);

        await capBus.PublishAsync(
            nameof(ForgetPasswordIntegrationEvent),
            integrationEvent,
            cancellationToken: ct
        );

        return Result.Success();
    }
}
