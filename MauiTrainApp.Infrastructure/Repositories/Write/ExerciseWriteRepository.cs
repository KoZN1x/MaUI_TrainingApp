using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Infrastructure.Database;
using MauiTrainApp.Infrastructure.Interfaces;
using MauiTrainApp.Infrastructure.Records;
using MauiTrainApp.Infrastructure.Repositories.Base;

namespace MauiTrainApp.Infrastructure.Repositories.Write
{
    internal sealed class ExerciseWriteRepository : WriteRepositoryBase<Exercise, ExerciseRecord>
    {
        public ExerciseWriteRepository(
            IRecordMapper<Exercise, ExerciseRecord> recordMapper,
            MauiTrainAppDbContext dbContext)
            : base(recordMapper, dbContext)
        {
        }
    }
}
