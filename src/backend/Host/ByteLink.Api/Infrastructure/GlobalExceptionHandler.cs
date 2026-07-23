using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ByteLink.Api.Infrastructure;

internal sealed partial class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger
) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        if (exception is ValidationException validationException)
        {
            var errorsDictionary = validationException
                .Errors.GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            var validationProblemDetails = new HttpValidationProblemDetails(errorsDictionary)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                Detail = "One or more validation errors occurred.",
            };

            validationProblemDetails.Extensions["errorCode"] = "ValidationError";
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

            return await problemDetailsService.TryWriteAsync(
                new ProblemDetailsContext
                {
                    HttpContext = httpContext,
                    Exception = exception,
                    ProblemDetails = validationProblemDetails,
                }
            );
        }

        LogUnhandledException(logger, httpContext.Request.Path, exception);

        const string message = "An unexpected error occurred on the server. Please try again later.";
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Server Error",
            Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
            Detail = message,
        };

        problemDetails.Extensions["errorCode"] = "InternalServerError";
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        return await problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = problemDetails,
            }
        );
    }

    [LoggerMessage(
        EventId = 101,
        Level = LogLevel.Error,
        Message = "An unhandled exception occurred during request: {Path}"
    )]
    private static partial void LogUnhandledException(ILogger logger, string path, Exception exception);
}
