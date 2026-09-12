using MauiTrainApp.Core.CQRS.Interfaces;

namespace MauiTrainApp.Core.CQRS;

internal sealed class Sender : ISender
{
    private readonly IServiceProvider _serviceProvider;
    private readonly HandlerRegistry _registry;

    public Sender(IServiceProvider serviceProvider, HandlerRegistry registry)
    {
        _serviceProvider = serviceProvider;
        _registry = registry;
    }

    public Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        return Invoke<TResult>(command, cancellationToken);
    }

    public Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        return Invoke<TResult>(query, cancellationToken);
    }

    private Task<TResult> Invoke<TResult>(object message, CancellationToken cancellationToken)
    {
        var invoker = _registry.Resolve<TResult>(message.GetType());

        return invoker(_serviceProvider, message, cancellationToken);
    }
}
