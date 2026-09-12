using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Domain.Interfaces;
using MauiTrainApp.Infrastructure.Database;
using MauiTrainApp.Infrastructure.Interfaces;
using MauiTrainApp.Infrastructure.Records;
using MauiTrainApp.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace MauiTrainApp.Infrastructure.Repositories
{
    internal sealed class WorkoutRepository : RepositoryBase<Workout, WorkoutRecord>, IWorkoutRepository
    {
        public WorkoutRepository(
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

        public Task<ICollection<Workout>> GetByPeriodAsync(
            DateOnly from,
            DateOnly to,
            CancellationToken cancellationToken = default)
        {
            return WhereAsync(x => x.WorkoutDay >= from && x.WorkoutDay <= to, cancellationToken);
        }

        public Task<ICollection<Workout>> GetByTrainingPlanAsync(
            Guid trainingPlanId,
            CancellationToken cancellationToken = default)
        {
            return WhereAsync(x => x.TrainingPlanRecordId == trainingPlanId, cancellationToken);
        }
    }
}
