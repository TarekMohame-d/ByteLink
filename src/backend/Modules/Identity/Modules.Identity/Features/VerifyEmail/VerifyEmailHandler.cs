using Microsoft.EntityFrameworkCore;
using Modules.Identity.Infrastructure.Persistence;
using Modules.Identity.Interfaces;
using Shared.Kernel.Messaging;
using Shared.Kernel.ResultPattern;

namespace Modules.Identity.Features.VerifyEmail;

internal sealed class VerifyEmailHandler(IdentityDbContext dbContext, ISecureHasher secureHasher)
    : ICommandHandler<VerifyEmailCommand>
{
    public async Task<Result> Handle(VerifyEmailCommand command, CancellationToken ct)
    {
        var tokenHash = secureHasher.HashToken(command.Token);

        var emailVerification = await dbContext.EmailVerificationTokens.FirstOrDefaultAsync(
            t => t.TokenHash == tokenHash,
            ct
        );

        if (emailVerification is null)
            return Result.Failure(Error.BadRequest("InvalidToken", "Invalid token."));

        if (!emailVerification.IsActive)
            return Result.Failure(Error.BadRequest("InvalidToken", "Token used or expired."));

        emailVerification.MarkAsUsed();

        var user =
            await dbContext.Users.FirstOrDefaultAsync(u =>
                u.Email == emailVerification.Email.ToLowerInvariant()
            ) ?? throw new InvalidOperationException("User not found.");

        user.VerifyEmail();
        user.Activate();

        await dbContext.SaveChangesAsync(ct);

        return Result.Success();
    }
}
