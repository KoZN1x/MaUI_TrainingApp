using MauiTrainApp.Domain.Interfaces;
using MauiTrainApp.Domain.ReadModels;
using MauiTrainApp.Infrastructure.Database;
using MauiTrainApp.Infrastructure.Records;
using MauiTrainApp.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace MauiTrainApp.Infrastructure.Repositories.Read
{
    internal sealed class WorkoutReadRepository
        : ReadRepositoryBase<WorkoutRecord, WorkoutListItemReadModel, WorkoutDetailsReadModel>,
          IWorkoutReadRepository
    {
        private const int SameDayCandidates = 20;

        public WorkoutReadRepository(MauiTrainAppDbContext dbContext)
            : base(dbContext)
        {
        }

        public Task<IReadOnlyCollection<WorkoutListItemReadModel>> GetByPeriodAsync(
            DateOnly from,
            DateOnly to,
            CancellationToken cancellationToken = default)
        {
            return ListAsync(
                Query.Where(x => x.WorkoutDay >= from && x.WorkoutDay <= to),
                cancellationToken);
        }

        public Task<IReadOnlyCollection<WorkoutListItemReadModel>> GetByTrainingPlanAsync(
            Guid trainingPlanId,
            CancellationToken cancellationToken = default)
        {
            return ListAsync(
                Query.Where(x => x.TrainingPlanRecordId == trainingPlanId),
                cancellationToken);
        }

        public async Task<Guid?> GetLastWorkoutIdAsync(
            Guid trainingPlanId,
            CancellationToken cancellationToken = default)
        {
            var candidates = await Query
                .Where(x => x.TrainingPlanRecordId == trainingPlanId)
                .OrderByDescending(x => x.WorkoutDay)
                .Select(x => new { x.Id, x.WorkoutDay, x.CreatedAt })
                .Take(SameDayCandidates)
                .ToListAsync(cancellationToken);

            return candidates
                .OrderByDescending(x => x.WorkoutDay)
                .ThenByDescending(x => x.CreatedAt)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefault();
        }

        public async Task<WorkoutListItemReadModel?> GetActiveAsync(
            DateOnly day,
            CancellationToken cancellationToken = default)
        {
            var candidates = await Project(Query.Where(x => x.WorkoutDay == day && x.DurationSeconds == null))
                .Take(SameDayCandidates)
                .ToListAsync(cancellationToken);

            return candidates
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => x.ToReadModel())
                .FirstOrDefault(x => x.TotalWorkingSetCount > 0 && !x.IsCompleted);
        }

        protected override async Task<IReadOnlyCollection<WorkoutListItemReadModel>> ListAsync(
            IQueryable<WorkoutRecord> query,
            CancellationToken cancellationToken)
        {
            var workouts = await Project(query.OrderByDescending(x => x.WorkoutDay))
                .ToListAsync(cancellationToken);

            return workouts
                .Select(x => x.ToReadModel())
                .ToList();
        }

        protected override async Task<WorkoutDetailsReadModel?> DetailsAsync(
            IQueryable<WorkoutRecord> query,
            CancellationToken cancellationToken)
        {
            var workout = await query
                .Select(x => new
                {
                    x.Id,
                    x.WorkoutDay,
                    x.TrainingPlanRecordId,
                    x.DurationSeconds,
                    TrainingPlanName = x.TrainingPlan!.Name,
                    ExerciseSets = x.ExerciseSets
                        .Select(exerciseSet => new ExerciseSetRow(
                            exerciseSet.Id,
                            exerciseSet.ExerciseRecordId,
                            exerciseSet.Exercise.Name,
                            exerciseSet.WorkingSets))
                        .ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);

            return workout is null
                ? null
                : new WorkoutDetailsReadModel(
                    workout.Id,
                    workout.WorkoutDay,
                    workout.TrainingPlanRecordId,
                    workout.TrainingPlanName,
                    workout.ExerciseSets.ToReadModels(),
                    workout.DurationSeconds is null ? null : TimeSpan.FromSeconds(workout.DurationSeconds.Value));
        }

        private static IQueryable<WorkoutListRow> Project(IQueryable<WorkoutRecord> query)
        {
            return query.Select(x => new WorkoutListRow(
                x.Id,
                x.WorkoutDay,
                x.TrainingPlanRecordId,
                x.TrainingPlan!.Name,
                x.DurationSeconds,
                x.CreatedAt,
                x.ExerciseSets.Select(exerciseSet => exerciseSet.WorkingSets).ToList()));
        }
    }
}
