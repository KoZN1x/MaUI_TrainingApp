using MauiTrainApp.Services.Interfaces;
using Microsoft.Maui.ApplicationModel;

namespace MauiTrainApp.Services
{
    internal sealed class DialogService : IDialogService
    {
        public Task<bool> ConfirmAsync(string title, string message, string accept = "Да", string cancel = "Отмена")
        {
            return MainThread.InvokeOnMainThreadAsync(() =>
            {
                var page = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;

                return page is null
                    ? Task.FromResult(false)
                    : page.DisplayAlertAsync(title, message, accept, cancel);
            });
        }
    }
}
