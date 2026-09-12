using MauiTrainApp.Infrastructure.DI;
using Microsoft.Extensions.Logging;

namespace MauiTrainApp.Startup
{
    public sealed class AppStartup
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AppStartup> _logger;
        private readonly Lazy<Task> _ready;

        public AppStartup(IServiceProvider serviceProvider, ILogger<AppStartup> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _ready = new Lazy<Task>(() => Task.Run(InitializeAsync));
        }

        public Task EnsureReadyAsync() => _ready.Value;

        private async Task InitializeAsync()
        {
            try
            {
                await _serviceProvider.MigrateDatabaseAsync();
                await _serviceProvider.SeedDatabaseAsync();

                _logger.LogInformation("Database is ready");
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Database initialization failed");

                throw;
            }
        }
    }
}
