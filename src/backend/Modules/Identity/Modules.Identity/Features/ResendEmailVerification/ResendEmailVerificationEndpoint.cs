using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Extensions;
using Shared.Kernel.Messaging;
using Shared.Kernel.ResultPattern;

namespace Modules.Identity.Features.ResendEmailVerification;

public static class ResendEmailVerificationEndpoint
{
    public static void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPost("/resend-verification", HandleAsync).RequireEmailLimit();
    }

    private static async Task<IResult> HandleAsync(
        ResendEmailVerificationCommand command,
        ICommandHandler<ResendEmailVerificationCommand> handler,
        CancellationToken ct
    )
    {
        var result = await handler.Handle(command, ct);
        return result.IsSuccess ? Results.Ok() : result.ToProblemDetails();
    }
}
