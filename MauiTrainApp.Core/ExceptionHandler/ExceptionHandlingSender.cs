using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Core.CQRS;
using MauiTrainApp.Core.ExceptionHandler.Interfaces;
using MauiTrainApp.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace MauiTrainApp.Core.ExceptionHandler;

internal sealed class ExceptionHandlingSender : ISender
{
    private readonly Sender _sender;
    private readonly IEnumerable<IExceptionHandler> _handlers;
    private readonly ILogger<ExceptionHandlingSender> _logger;

    public ExceptionHandlingSender(
        Sender sender,
        IEnumerable<IExceptionHandler> handlers,
        ILogger<ExceptionHandlingSender> logger)
    {
        _sender = sender;
        _handlers = handlers;
        _logger = logger;
    }

    public Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        return ExecuteAsync(() => _sender.SendAsync(command, cancellationToken), command.GetType());
    }

    public Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        return ExecuteAsync(() => _sender.QueryAsync(query, cancellationToken), query.GetType());
    }

    private async Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> send, Type messageType)
    {
        try
        {
            return await send();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw Translate(exception, messageType);
        }
    }

    private AppException Translate(Exception exception, Type messageType)
    {
        if (exception is AppException appException)
        {
            _logger.LogWarning(appException, "{Message} was rejected: {Reason}", messageType.Name, appException.Message);

            return appException;
        }

        var handler = _handlers.FirstOrDefault(x => x.CanHandle(exception));

        if (handler is not null)
        {
            var handled = handler.Handle(exception);

            _logger.LogWarning(handled, "{Message} was rejected: {Reason}", messageType.Name, handled.Message);

            return handled;
        }

        _logger.LogError(exception, "{Message} failed unexpectedly", messageType.Name);

        return new UnexpectedException(exception);
    }
}
