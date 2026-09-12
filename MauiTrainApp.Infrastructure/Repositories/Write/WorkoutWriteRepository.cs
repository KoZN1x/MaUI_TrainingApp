using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Infrastructure.Database;
using MauiTrainApp.Infrastructure.Interfaces;
using MauiTrainApp.Infrastructure.Records;
using MauiTrainApp.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace MauiTrainApp.Infrastructure.Repositories.Write
{
    internal sealed class WorkoutWriteRepository : WriteRepositoryBase<Workout, WorkoutRecord>
    {
        public WorkoutWriteRepository(
            IRecordMapper<Workout, WorkoutRecord> recordMapper,
            MauiTrainAppDbContext dbContext)
            : base(recordMapper, dbContext)
        {
        }

        protected override IQueryable<WorkoutRecord> Query => DbSet
            .Include(x => x.ExerciseSets)
            .ThenInclude(x => x.Exercise);

        public override async Task<Workout> UpdateAsync(
            Workout model,
            CancellationToken cancellationToken = default)
        {
            var record = Mapper.ToRecord(model);

            Detach<WorkoutRecord>(record.Id);
            DbContext.Entry(record).State = EntityState.Modified;

            await SyncChildrenAsync(
                record.ExerciseSets,
                x => x.WorkoutRecordId == record.Id,
                cancellationToken);

            await SaveChangesAsync(cancellationToken);

            return model;
        }

    }
}
