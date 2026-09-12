using MauiTrainApp.Core.CQRS.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace MauiTrainApp.Core.CQRS;

internal sealed class HandlerRegistry
{
    private readonly Dictionary<Type, object> _invokers = [];

    public void RegisterCommand<TCommand, TResult, THandler>()
        where TCommand : ICommand<TResult>
        where THandler : class, ICommandHandler<TCommand, TResult>
    {
        Add<TCommand, TResult>((provider, message, cancellationToken) =>
            provider.GetRequiredService<THandler>().HandleAsync((TCommand)message, cancellationToken));
    }

    public void RegisterQuery<TQuery, TResult, THandler>()
        where TQuery : IQuery<TResult>
        where THandler : class, IQueryHandler<TQuery, TResult>
    {
        Add<TQuery, TResult>((provider, message, cancellationToken) =>
            provider.GetRequiredService<THandler>().HandleAsync((TQuery)message, cancellationToken));
    }

    public Func<IServiceProvider, object, CancellationToken, Task<TResult>> Resolve<TResult>(Type messageType)
    {
        if (!_invokers.TryGetValue(messageType, out var invoker))
        {
            throw new InvalidOperationException($"Handler for '{messageType.Name}' is not registered");
        }

        return (Func<IServiceProvider, object, CancellationToken, Task<TResult>>)invoker;
    }

    private void Add<TMessage, TResult>(Func<IServiceProvider, object, CancellationToken, Task<TResult>> invoker)
    {
        if (!_invokers.TryAdd(typeof(TMessage), invoker))
        {
            throw new InvalidOperationException($"Handler for '{typeof(TMessage).Name}' is already registered");
        }
    }
}
