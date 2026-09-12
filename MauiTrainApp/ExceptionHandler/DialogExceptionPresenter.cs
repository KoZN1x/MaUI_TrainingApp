using MauiTrainApp.Domain.Exceptions;
using MauiTrainApp.ExceptionHandler.Interfaces;

namespace MauiTrainApp.ExceptionHandler
{
    internal sealed class DialogExceptionPresenter : IExceptionPresenter
    {
        private const string Title = "Error";
        private const string Accept = "OK";

        public Task PresentAsync(AppException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);

            return MainThread.InvokeOnMainThreadAsync(() =>
            {
                var page = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;

                return page is null
                    ? Task.CompletedTask
                    : page.DisplayAlert(Title, exception.UserMessage, Accept);
            });
        }
    }
}
