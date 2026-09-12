using MauiTrainApp.Core.ExceptionHandler.Interfaces;
using MauiTrainApp.Domain.Exceptions;

namespace MauiTrainApp.Application.ExceptionHandler;

internal sealed class DomainExceptionHandler : IExceptionHandler
{
    public bool CanHandle(Exception exception)
    {
        return exception is InvariantException
            || IsEntityNotFound(exception)
            || exception is ArgumentException;
    }

    public AppException Handle(Exception exception)
    {
        if (IsEntityNotFound(exception))
        {
            return new NotFoundException(exception.Message, exception);
        }

        return new ValidationException(exception.Message, exception);
    }

    private static bool IsEntityNotFound(Exception exception)
    {
        var type = exception.GetType();

        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(EntityNotFoundException<>);
    }
}
