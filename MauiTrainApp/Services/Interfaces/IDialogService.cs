namespace MauiTrainApp.Services.Interfaces
{
    public interface IDialogService
    {
        Task<bool> ConfirmAsync(string title, string message, string accept = "Да", string cancel = "Отмена");
    }
}
