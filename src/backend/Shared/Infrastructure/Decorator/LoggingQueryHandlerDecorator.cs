using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Shared.Kernel.Messaging;
using Shared.Kernel.ResultPattern;

namespace Shared.Infrastructure.Decorator;

public sealed class LoggingQueryHandlerDecorator<TQuery, TResponse>(
    IQueryHandler<TQuery, TResponse> inner,
    ILogger<LoggingQueryHandlerDecorator<TQuery, TResponse>> logger
) : IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    public async Task<Result<TResponse>> Handle(TQuery query, CancellationToken ct)
    {
        string queryName = typeof(TQuery).Name;
        logger.LogInformation("Executing query {QueryName}", queryName);

        long startTime = Stopwatch.GetTimestamp();

        try
        {
            Result<TResponse> result = await inner.Handle(query, ct);
            TimeSpan elapsed = Stopwatch.GetElapsedTime(startTime);

            if (result.IsSuccess)
            {
                logger.LogInformation(
                    "Executed query {QueryName} successfully in {ElapsedMs}ms",
                    queryName,
                    elapsed.TotalMilliseconds
                );
            }
            else
            {
                logger.LogWarning(
                    "Query {QueryName} failed in {ElapsedMs}ms. Error: {Error}",
                    queryName,
                    elapsed.TotalMilliseconds,
                    result.Error
                );
            }

            return result;
        }
        catch (Exception ex)
        {
            TimeSpan elapsed = Stopwatch.GetElapsedTime(startTime);
            logger.LogError(
                ex,
                "Query {QueryName} threw an unhandled exception after {ElapsedMs}ms",
                queryName,
                elapsed.TotalMilliseconds
            );
            throw;
        }
    }
}
