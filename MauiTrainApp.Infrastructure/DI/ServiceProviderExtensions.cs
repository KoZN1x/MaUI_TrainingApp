using MauiTrainApp.Infrastructure.Database;
using MauiTrainApp.Infrastructure.Database.Seeding;
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

            public async Task SeedDatabaseAsync(CancellationToken cancellationToken = default)
            {
                await using var scope = serviceProvider.CreateAsyncScope();

                var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();

                await seeder.SeedAsync(cancellationToken);
            }
        }
    }
}
