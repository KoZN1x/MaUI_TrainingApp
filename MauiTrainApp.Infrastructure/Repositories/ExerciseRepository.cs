using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Domain.Interfaces;
using MauiTrainApp.Infrastructure.Database;
using MauiTrainApp.Infrastructure.Interfaces;
using MauiTrainApp.Infrastructure.Records;
using MauiTrainApp.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace MauiTrainApp.Infrastructure.Repositories
{
    internal sealed class ExerciseRepository : RepositoryBase<Exercise, ExerciseRecord>, IExerciseRepository
    {
        public ExerciseRepository(
            IRecordMapper<Exercise, ExerciseRecord> recordMapper,
            MauiTrainAppDbContext dbContext)
            : base(recordMapper, dbContext)
        {
        }

        public Task<ICollection<Exercise>> SearchByNameAsync(
            string name,
            CancellationToken cancellationToken = default)
        {
            return WhereAsync(x => EF.Functions.Like(x.Name, $"%{name}%"), cancellationToken);
        }
    }
}
