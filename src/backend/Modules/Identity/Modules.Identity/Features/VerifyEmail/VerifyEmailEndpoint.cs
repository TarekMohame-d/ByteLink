using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Kernel.Messaging;
using Shared.Kernel.ResultPattern;

namespace Modules.Identity.Features.VerifyEmail;

public static class VerifyEmailEndpoint
{
    public static void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPost("/verify-email", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        VerifyEmailCommand command,
        ICommandHandler<VerifyEmailCommand> handler,
        CancellationToken ct
    )
    {
        var result = await handler.Handle(command, ct);
        return result.IsSuccess ? Results.Ok() : result.ToProblemDetails();
    }
}
