using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Kernel.Messaging;
using Shared.Kernel.ResultPattern;

namespace Modules.Identity.Features.Register;

public static class RegisterEndpoint
{
    public static void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPost("/register", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        RegisterCommand command,
        ICommandHandler<RegisterCommand, Guid> handler,
        CancellationToken ct
    )
    {
        var result = await handler.Handle(command, ct);
        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
    }
}
