using Microsoft.EntityFrameworkCore;
using Modules.Identity.Domain;
using Modules.Identity.Infrastructure.Persistence;
using Modules.Identity.IntegrationEvents;
using Modules.Identity.Interfaces;
using Shared.Infrastructure.Messaging;
using Shared.Kernel.Messaging;
using Shared.Kernel.ResultPattern;

namespace Modules.Identity.Features.Register;

internal sealed class RegisterHandler(
    IdentityDbContext dbContext,
    ISecureHasher secureHasher,
    ISecureGenerator secureGenerator,
    IOutboxWriter<IdentityDbContext> outboxWriter,
    OutboxSignalChannel signalChannel
) : ICommandHandler<RegisterCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RegisterCommand command, CancellationToken ct)
    {
        var email = command.Email.ToLowerInvariant();
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user is not null)
            return Error.BadRequest("EmailAlreadyExists", "A user with this email address already exists.");

        user = User.Create(
            command.FirstName,
            command.LastName,
            email,
            secureHasher.HashPassword(command.Password),
            UserRole.User
        );

        var token = secureGenerator.GenerateToken();
        var tokenHash = secureHasher.HashToken(token);
        var emailVerification = EmailVerificationToken.Create(email, tokenHash);

        TimeSpan remaining = emailVerification.ExpiresAtUtc - DateTimeOffset.UtcNow;
        int minutes = (int)Math.Ceiling(remaining.TotalMinutes);
        string expiresIn = $"{minutes}";

        var integrationEvent = new UserRegisteredIntegrationEvent(user.Id, command.Email, token, expiresIn);

        dbContext.Users.Add(user);
        dbContext.EmailVerificationTokens.Add(emailVerification);

        outboxWriter.Write(integrationEvent);

        await dbContext.SaveChangesAsync(ct);

        signalChannel.Signal();

        return user.Id;
    }
}
