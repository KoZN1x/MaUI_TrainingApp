using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MauiTrainApp.Infrastructure.Database
{
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
