using MauiTrainApp.Domain.Exceptions;

namespace MauiTrainApp.Core.ExceptionHandler.Interfaces;

public interface IExceptionHandler
{
    bool CanHandle(Exception exception);

    AppException Handle(Exception exception);
}
