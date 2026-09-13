using MauiTrainApp.Infrastructure.Database;
using MauiTrainApp.Infrastructure.Database.Seeding;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MauiTrainApp.Infrastructure.DI
{
    public static class ServiceProviderExtensions
    {
        extension(IServiceProvider serviceProvider)
        {
            public async Task MigrateDatabaseAsync(CancellationToken cancellationToken = default)
            {
                await using var scope = serviceProvider.CreateAsyncScope();

                var dbContext = scope.ServiceProvider.GetRequiredService<MauiTrainAppDbContext>();

                await dbContext.Database.MigrateAsync(cancellationToken);
            }

            public async Task<string> BackupDatabaseAsync(
                string targetDirectory,
                CancellationToken cancellationToken = default)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(targetDirectory);

                string source;

                await using (var scope = serviceProvider.CreateAsyncScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<MauiTrainAppDbContext>();

                    await dbContext.Database.CloseConnectionAsync();

                    source = dbContext.Database.GetDbConnection().DataSource;
                }

                SqliteConnection.ClearAllPools();

                Directory.CreateDirectory(targetDirectory);

                var target = Path.Combine(
                    targetDirectory,
                    $"mauitrain-{DateTime.Now:yyyy-MM-dd-HHmm}.db");

                await using (var input = File.OpenRead(source))
                await using (var output = File.Create(target))
                {
                    await input.CopyToAsync(output, cancellationToken);
                }

                return target;
            }

            public async Task SeedDatabaseAsync(CancellationToken cancellationToken = default)
            {
                await using var scope = serviceProvider.CreateAsyncScope();

                var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();

                await seeder.SeedAsync(cancellationToken);
            }
        }
    }
}
