using Microsoft.EntityFrameworkCore;
using Modules.Identity.Infrastructure.Persistence;
using Modules.Identity.Interfaces;
using Shared.Kernel.Messaging;
using Shared.Kernel.ResultPattern;

namespace Modules.Identity.Features.ResetPassword;

internal sealed class ResetPasswordHandler(IdentityDbContext dbContext, ISecureHasher secureHasher)
    : ICommandHandler<ResetPasswordCommand>
{
    public async Task<Result> Handle(ResetPasswordCommand command, CancellationToken ct)
    {
        var tokenHash = secureHasher.HashToken(command.Token);

        var forgetPassword = await dbContext.ForgetPasswordTokens.FirstOrDefaultAsync(
            t => t.TokenHash == tokenHash,
            ct
        );

        if (forgetPassword is null)
            return Result.Failure(Error.BadRequest("InvalidToken", "Invalid token."));

        if (!forgetPassword.IsActive)
            return Result.Failure(Error.BadRequest("InvalidToken", "Token used or expired."));

        forgetPassword.MarkAsUsed();

        var user =
            await dbContext.Users.FirstOrDefaultAsync(u => u.Email == forgetPassword.Email.ToLowerInvariant())
            ?? throw new InvalidOperationException("User not found.");

        user.VerifyEmail();

        var passwordHash = secureHasher.HashPassword(command.Password);
        user.ChangePassword(passwordHash);

        await dbContext.SaveChangesAsync(ct);

        return Result.Success();
    }
}
