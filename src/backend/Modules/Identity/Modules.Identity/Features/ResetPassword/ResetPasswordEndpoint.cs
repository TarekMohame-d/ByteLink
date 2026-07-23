using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Kernel.Messaging;
using Shared.Kernel.ResultPattern;

namespace Modules.Identity.Features.ResetPassword;

public static class ResetPasswordEndpoint
{
    public static void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPost("/reset-password", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        ResetPasswordCommand command,
        ICommandHandler<ResetPasswordCommand> handler,
        CancellationToken ct
    )
    {
        var result = await handler.Handle(command, ct);
        return result.IsSuccess ? Results.Ok() : result.ToProblemDetails();
    }
}
