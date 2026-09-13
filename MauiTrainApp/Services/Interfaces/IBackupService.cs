namespace MauiTrainApp.Services.Interfaces
{
    public interface IBackupService
    {
        Task<string> ShareAsync(CancellationToken cancellationToken = default);
    }
}
