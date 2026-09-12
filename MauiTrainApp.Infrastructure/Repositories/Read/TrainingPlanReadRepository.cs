using MauiTrainApp.Domain.Interfaces;
using MauiTrainApp.Domain.ReadModels;
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
            return await query
                .OrderBy(x => x.Name)
                .Select(x => new TrainingPlanListItemReadModel(x.Id, x.Name, x.ExerciseSets.Count))
                .ToListAsync(cancellationToken);
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
                    ExerciseSets = x.ExerciseSets
                        .Select(exerciseSet => new ExerciseSetRow(
                            exerciseSet.Id,
                            exerciseSet.ExerciseRecordId,
                            exerciseSet.Exercise.Name,
                            exerciseSet.WorkingSets))
                        .ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);

            return trainingPlan is null
                ? null
                : new TrainingPlanDetailsReadModel(
                    trainingPlan.Id,
                    trainingPlan.Name,
                    trainingPlan.ExerciseSets.ToReadModels());
        }
    }
}
