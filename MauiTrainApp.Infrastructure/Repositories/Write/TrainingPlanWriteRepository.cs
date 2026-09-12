using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Infrastructure.Database;
using MauiTrainApp.Infrastructure.Interfaces;
using MauiTrainApp.Infrastructure.Records;
using MauiTrainApp.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace MauiTrainApp.Infrastructure.Repositories.Write
{
    internal sealed class TrainingPlanWriteRepository : WriteRepositoryBase<TrainingPlan, TrainingPlanRecord>
    {
        public TrainingPlanWriteRepository(
            IRecordMapper<TrainingPlan, TrainingPlanRecord> recordMapper,
            MauiTrainAppDbContext dbContext)
            : base(recordMapper, dbContext)
        {
        }

        protected override IQueryable<TrainingPlanRecord> Query => DbSet
            .Include(x => x.ExerciseSets)
            .ThenInclude(x => x.Exercise);

        public override async Task<TrainingPlan> UpdateAsync(
            TrainingPlan model,
            CancellationToken cancellationToken = default)
        {
            var record = Mapper.ToRecord(model);

            Detach<TrainingPlanRecord>(record.Id);
            DbContext.Entry(record).State = EntityState.Modified;

            await SyncChildrenAsync(
                record.ExerciseSets,
                x => x.TrainingPlanRecordId == record.Id,
                cancellationToken);

            await SaveChangesAsync(cancellationToken);

            return model;
        }
    }
}
