using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Shared.Kernel.Messaging;

namespace Shared.Infrastructure.Decorator;

public static class TransactionDecoratorExtensions
{
    public static IServiceCollection DecorateModuleTransactionalHandlers(
        this IServiceCollection services,
        Assembly moduleAssembly,
        Type decoratorWithoutResponse,
        Type decoratorWithResponse
    )
    {
        var descriptors = services.ToList();

        foreach (var descriptor in descriptors)
        {
            var serviceType = descriptor.ServiceType;

            if (!serviceType.IsGenericType)
                continue;

            var genericTypeDefinition = serviceType.GetGenericTypeDefinition();

            if (genericTypeDefinition == typeof(ICommandHandler<>))
            {
                var commandType = serviceType.GetGenericArguments()[0];

                if (
                    commandType.Assembly == moduleAssembly
                    && typeof(ITransactionalCommand).IsAssignableFrom(commandType)
                )
                {
                    var closedDecorator = decoratorWithoutResponse.MakeGenericType(commandType);
                    services.Decorate(serviceType, closedDecorator);
                }
            }
            else if (genericTypeDefinition == typeof(ICommandHandler<,>))
            {
                var genericArgs = serviceType.GetGenericArguments();
                var commandType = genericArgs[0];
                var responseType = genericArgs[1];

                var isTransactional = commandType
                    .GetInterfaces()
                    .Any(i =>
                        i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ITransactionalCommand<>)
                    );

                if (commandType.Assembly == moduleAssembly && isTransactional)
                {
                    var closedDecorator = decoratorWithResponse.MakeGenericType(commandType, responseType);
                    services.Decorate(serviceType, closedDecorator);
                }
            }
        }

        return services;
    }
}
