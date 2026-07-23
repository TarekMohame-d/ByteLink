using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;
using Modules.Identity.Infrastructure.Persistence;
using Shared.Kernel.Messaging;
using Shared.Kernel.ResultPattern;

namespace Modules.Identity.Infrastructure.Decorators;

public sealed class IdentityTransactionDecorator<TCommand>(
    ICommandHandler<TCommand> inner,
    IdentityDbContext dbContext,
    ICapPublisher capBus
) : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    public async Task<Result> Handle(TCommand command, CancellationToken ct)
    {
        if (command is not ITransactionalCommand)
        {
            return await inner.Handle(command, ct);
        }

        using var transaction = await dbContext.Database.BeginTransactionAsync(capBus, autoCommit: false, ct);
        try
        {
            var result = await inner.Handle(command, ct);
            if (result.IsFailure)
            {
                await transaction.RollbackAsync(ct);
                return result;
            }

            await dbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return result;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
}

public sealed class IdentityTransactionDecorator<TCommand, TResponse>(
    ICommandHandler<TCommand, TResponse> inner,
    IdentityDbContext dbContext,
    ICapPublisher capBus
) : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    public async Task<Result<TResponse>> Handle(TCommand command, CancellationToken ct)
    {
        if (command is not ITransactionalCommand<TResponse>)
        {
            return await inner.Handle(command, ct);
        }

        using var transaction = await dbContext.Database.BeginTransactionAsync(capBus, autoCommit: false, ct);
        try
        {
            var result = await inner.Handle(command, ct);
            if (result.IsFailure)
            {
                await transaction.RollbackAsync(ct);
                return result;
            }

            await dbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return result;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
}
