using MauiTrainApp.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MauiTrainApp.Infrastructure.DI
{
    public static class ServiceProviderExtensions
    {
        /// <summary>
        /// Применяет миграции к файлу базы. Вызывать один раз при старте приложения.
        /// </summary>
        public static async Task MigrateDatabaseAsync(
            this IServiceProvider serviceProvider,
            CancellationToken cancellationToken = default)
        {
            await using var scope = serviceProvider.CreateAsyncScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<MauiTrainAppDbContext>();

            await dbContext.Database.MigrateAsync(cancellationToken);
        }
    }
}
