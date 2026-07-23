using Microsoft.EntityFrameworkCore;
using Modules.Identity.Infrastructure.Persistence;
using Modules.Identity.Interfaces;
using Shared.Kernel.Messaging;
using Shared.Kernel.ResultPattern;

namespace Modules.Identity.Features.Logout;

internal sealed class LogoutHandler(IdentityDbContext dbContext, ISecureHasher secureHasher)
    : ICommandHandler<LogoutCommand>
{
    public async Task<Result> Handle(LogoutCommand command, CancellationToken ct)
    {
        string hashedToken = secureHasher.HashToken(command.RefreshToken);

        var storedToken = await dbContext.UserRefreshTokens.FirstOrDefaultAsync(
            t => t.TokenHash == hashedToken,
            ct
        );

        if (storedToken != null)
        {
            storedToken.Revoke();
            await dbContext.SaveChangesAsync(ct);
        }

        return Result.Success();
    }
}
