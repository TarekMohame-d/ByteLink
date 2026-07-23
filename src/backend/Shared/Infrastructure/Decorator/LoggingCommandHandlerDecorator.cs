using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Shared.Kernel.Messaging;
using Shared.Kernel.ResultPattern;

namespace Shared.Infrastructure.Decorator;

public sealed class LoggingCommandHandlerDecorator<TCommand>(
    ICommandHandler<TCommand> inner,
    ILogger<LoggingCommandHandlerDecorator<TCommand>> logger
) : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    public async Task<Result> Handle(TCommand command, CancellationToken ct)
    {
        string commandName = typeof(TCommand).Name;
        logger.LogInformation("Processing command {CommandName}", commandName);

        long startTime = Stopwatch.GetTimestamp();

        try
        {
            Result result = await inner.Handle(command, ct);
            TimeSpan elapsed = Stopwatch.GetElapsedTime(startTime);

            if (result.IsSuccess)
            {
                logger.LogInformation(
                    "Processed command {CommandName} successfully in {ElapsedMs}ms",
                    commandName,
                    elapsed.TotalMilliseconds
                );
            }
            else
            {
                logger.LogWarning(
                    "Command {CommandName} failed in {ElapsedMs}ms. Error: {Error}",
                    commandName,
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
                "Command {CommandName} threw an unhandled exception after {ElapsedMs}ms",
                commandName,
                elapsed.TotalMilliseconds
            );
            throw;
        }
    }
}

public sealed class LoggingCommandHandlerDecorator<TCommand, TResponse>(
    ICommandHandler<TCommand, TResponse> inner,
    ILogger<LoggingCommandHandlerDecorator<TCommand, TResponse>> logger
) : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    public async Task<Result<TResponse>> Handle(TCommand command, CancellationToken ct)
    {
        string commandName = typeof(TCommand).Name;
        logger.LogInformation("Processing command {CommandName}", commandName);

        long startTime = Stopwatch.GetTimestamp();

        try
        {
            Result<TResponse> result = await inner.Handle(command, ct);
            TimeSpan elapsed = Stopwatch.GetElapsedTime(startTime);

            if (result.IsSuccess)
            {
                logger.LogInformation(
                    "Processed command {CommandName} successfully in {ElapsedMs}ms",
                    commandName,
                    elapsed.TotalMilliseconds
                );
            }
            else
            {
                logger.LogWarning(
                    "Command {CommandName} failed in {ElapsedMs}ms. Error: {Error}",
                    commandName,
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
                "Command {CommandName} threw an unhandled exception after {ElapsedMs}ms",
                commandName,
                elapsed.TotalMilliseconds
            );
            throw;
        }
    }
}
