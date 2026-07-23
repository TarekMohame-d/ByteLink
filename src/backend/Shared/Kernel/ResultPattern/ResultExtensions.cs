using Microsoft.AspNetCore.Http;

namespace Shared.Kernel.ResultPattern;

public static class ResultExtensions
{
    public static IResult ToProblemDetails(this Result result)
    {
        if (result.IsSuccess)
            throw new InvalidOperationException("Cannot convert successful result to problem details.");

        (int statusCode, string title) = result.Error.Type switch
        {
            ErrorType.Validation => (StatusCodes.Status400BadRequest, "Validation Error"),
            ErrorType.BadRequest => (StatusCodes.Status400BadRequest, "Bad Request"),
            ErrorType.NotFound => (StatusCodes.Status404NotFound, "Not Found"),
            ErrorType.Conflict => (StatusCodes.Status409Conflict, "Conflict"),
            ErrorType.Unauthorized => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            ErrorType.Forbidden => (StatusCodes.Status403Forbidden, "Forbidden"),

            _ => (StatusCodes.Status500InternalServerError, "Server Error"),
        };

        var extensions = new Dictionary<string, object?>
        {
            { "errorCode", result.Error.Code },
            { "errorDescription", result.Error.Description },
        };

        return Results.Problem(
            title: title,
            detail: result.Error.Description,
            statusCode: statusCode,
            extensions: extensions
        );
    }
}
