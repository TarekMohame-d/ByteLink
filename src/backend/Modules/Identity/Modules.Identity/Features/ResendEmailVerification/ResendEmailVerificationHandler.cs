using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;
using Modules.Identity.Infrastructure.Persistence;
using Modules.Identity.IntegrationEvents;
using Modules.Identity.Interfaces;
using Shared.Kernel.Messaging;
using Shared.Kernel.ResultPattern;

namespace Modules.Identity.Features.ResendEmailVerification;

internal sealed class ResendEmailVerificationHandler(
    IdentityDbContext dbContext,
    ISecureGenerator secureGenerator,
    ISecureHasher secureHasher,
    ICapPublisher capBus
) : ICommandHandler<ResendEmailVerificationCommand>
{
    public async Task<Result> Handle(ResendEmailVerificationCommand command, CancellationToken ct)
    {
        var email = command.Email.ToLowerInvariant();

        var emailVerification = await dbContext.EmailVerificationTokens.FirstOrDefaultAsync(
            t => t.Email == email,
            ct
        );

        if (emailVerification is null)
            return Result.Failure(
                Error.BadRequest("EmailNotFound", "A user with this email address was not found.")
            );

        if (emailVerification.IsUsed)
            return Result.Failure(
                Error.BadRequest("EmailAlreadyVerified", "This email address has already been verified.")
            );

        var token = secureGenerator.GenerateToken();
        var tokenHash = secureHasher.HashToken(token);

        emailVerification.Renew(tokenHash);

        TimeSpan remaining = emailVerification.ExpiresAtUtc - DateTimeOffset.UtcNow;
        int minutes = (int)Math.Ceiling(remaining.TotalMinutes);
        string expiresIn = $"{minutes}";

        var integrationEvent = new ResendEmailVerificationIntegrationEvent(email, token, expiresIn);

        await capBus.PublishAsync(
            nameof(ResendEmailVerificationIntegrationEvent),
            integrationEvent,
            cancellationToken: ct
        );

        return Result.Success();
    }
}
