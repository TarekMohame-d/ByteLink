using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Extensions;
using Shared.Infrastructure.Middlewares;
using Shared.Kernel.Messaging;
using Shared.Kernel.ResultPattern;

namespace Modules.Identity.Features.ForgetPassword;

public static class ForgetPasswordEndpoint
{
    public static void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPost("/forget-password", HandleAsync).RequireEmailLimit();
    }

    private static async Task<IResult> HandleAsync(
        ForgetPasswordCommand command,
        ICommandHandler<ForgetPasswordCommand> handler,
        CancellationToken ct
    )
    {
        var result = await handler.Handle(command, ct);
        return result.IsSuccess ? Results.Ok() : result.ToProblemDetails();
    }
}
