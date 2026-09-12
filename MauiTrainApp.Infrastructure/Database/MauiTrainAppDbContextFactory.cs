using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MauiTrainApp.Infrastructure.Database
{
    /// <summary>
    /// Используется только инструментами EF (dotnet ef migrations). В рантайме контекст берётся из DI.
    /// </summary>
    internal sealed class MauiTrainAppDbContextFactory : IDesignTimeDbContextFactory<MauiTrainAppDbContext>
    {
        public MauiTrainAppDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<MauiTrainAppDbContext>()
                .UseSqlite("Data Source=mauitrain.db")
                .Options;

            return new MauiTrainAppDbContext(options);
        }
    }
}
