using MauiTrainApp.Infrastructure.Database;
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
        }
    }
}
