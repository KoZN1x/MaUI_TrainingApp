using MauiTrainApp.Domain.Exceptions;
using MauiTrainApp.ExceptionHandler.Interfaces;
using Microsoft.Maui.ApplicationModel;

namespace MauiTrainApp.ExceptionHandler
{
    internal sealed class DialogExceptionPresenter : IExceptionPresenter
    {
        private const string Title = "Не получилось";
        private const string Accept = "Понятно";

        public Task PresentAsync(AppException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);

            return MainThread.InvokeOnMainThreadAsync(() =>
            {
                var page = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;

                return page is null
                    ? Task.CompletedTask
                    : page.DisplayAlertAsync(Title, exception.UserMessage, Accept);
            });
        }
    }
}
