using MauiTrainApp.Domain.Exceptions;

namespace MauiTrainApp.ExceptionHandler.Interfaces;

public interface IExceptionPresenter
{
    Task PresentAsync(AppException exception);
}
