using MauiTrainApp.Infrastructure.DI;
using MauiTrainApp.Services.Interfaces;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Storage;

namespace MauiTrainApp.Services
{
    internal sealed class BackupService : IBackupService
    {
        private const string BackupFolder = "backup";

        private readonly IServiceProvider _serviceProvider;

        public BackupService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<string> ShareAsync(CancellationToken cancellationToken = default)
        {
            var path = await _serviceProvider.BackupDatabaseAsync(
                Path.Combine(FileSystem.CacheDirectory, BackupFolder),
                cancellationToken);

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Копия базы тренировок",
                File = new ShareFile(path)
            });

            return Path.GetFileName(path);
        }
    }
}
