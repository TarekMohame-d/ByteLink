using Microsoft.EntityFrameworkCore;
using Modules.Identity.Domain;
using Modules.Identity.Infrastructure.Persistence;
using Modules.Identity.IntegrationEvents;
using Modules.Identity.Interfaces;
using Shared.Infrastructure.Messaging;
using Shared.Kernel.Messaging;
using Shared.Kernel.ResultPattern;

namespace Modules.Identity.Features.ForgetPassword;

internal sealed class ForgetPasswordHandler(
    IdentityDbContext dbContext,
    ISecureGenerator secureGenerator,
    ISecureHasher secureHasher,
    IOutboxWriter<IdentityDbContext> outboxWriter,
    OutboxSignalChannel signalChannel
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

        outboxWriter.Write(integrationEvent);

        await dbContext.SaveChangesAsync(ct);

        signalChannel.Signal();

        return Result.Success();
    }
}
