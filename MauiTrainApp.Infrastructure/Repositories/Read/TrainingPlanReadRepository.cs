using MauiTrainApp.Domain.Interfaces;
using MauiTrainApp.Domain.ReadModels;
using MauiTrainApp.Domain.ValueObjects;
using MauiTrainApp.Infrastructure.Database;
using MauiTrainApp.Infrastructure.Records;
using MauiTrainApp.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace MauiTrainApp.Infrastructure.Repositories.Read
{
    internal sealed class TrainingPlanReadRepository
        : ReadRepositoryBase<TrainingPlanRecord, TrainingPlanListItemReadModel, TrainingPlanDetailsReadModel>,
          ITrainingPlanReadRepository
    {
        public TrainingPlanReadRepository(MauiTrainAppDbContext dbContext)
            : base(dbContext)
        {
        }

        protected override async Task<IReadOnlyCollection<TrainingPlanListItemReadModel>> ListAsync(
            IQueryable<TrainingPlanRecord> query,
            CancellationToken cancellationToken)
        {
            var trainingPlans = await query
                .OrderBy(x => x.Name)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.ScheduleMask,
                    ExerciseSetCount = x.ExerciseSets.Count
                })
                .ToListAsync(cancellationToken);

            return [.. trainingPlans.Select(x => new TrainingPlanListItemReadModel(
                x.Id,
                x.Name,
                x.ExerciseSetCount,
                WeekSchedule.FromMask(x.ScheduleMask)))];
        }

        protected override async Task<TrainingPlanDetailsReadModel?> DetailsAsync(
            IQueryable<TrainingPlanRecord> query,
            CancellationToken cancellationToken)
        {
            var trainingPlan = await query
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.ScheduleMask,
                    ExerciseSets = x.ExerciseSets
                        .Select(exerciseSet => new ExerciseSetRow(
                            exerciseSet.Id,
                            exerciseSet.ExerciseRecordId,
                            exerciseSet.Exercise.Name,
                            exerciseSet.WorkingSets,
                            exerciseSet.CreatedAt,
                            exerciseSet.Position))
                        .ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);

            return trainingPlan is null
                ? null
                : new TrainingPlanDetailsReadModel(
                    trainingPlan.Id,
                    trainingPlan.Name,
                    trainingPlan.ExerciseSets.ToReadModels(),
                    WeekSchedule.FromMask(trainingPlan.ScheduleMask));
        }
    }
}
