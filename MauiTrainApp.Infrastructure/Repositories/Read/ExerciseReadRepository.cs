using MauiTrainApp.Domain.Interfaces;
using MauiTrainApp.Domain.ReadModels;
using MauiTrainApp.Domain.ValueObjects;
using MauiTrainApp.Infrastructure.Database;
using MauiTrainApp.Infrastructure.Records;
using MauiTrainApp.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace MauiTrainApp.Infrastructure.Repositories.Read
{
    internal sealed class ExerciseReadRepository
        : ReadRepositoryBase<ExerciseRecord, ExerciseReadModel, ExerciseReadModel>, IExerciseReadRepository
    {
        public ExerciseReadRepository(MauiTrainAppDbContext dbContext)
            : base(dbContext)
        {
        }

        public Task<IReadOnlyCollection<ExerciseReadModel>> SearchByNameAsync(
            string searchTerm,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(searchTerm);

            return ListAsync(
                Query.Where(x => EF.Functions.Like(x.Name, $"%{searchTerm.Trim()}%")),
                cancellationToken);
        }

        public Task<bool> ExistsWithNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return Query.AnyAsync(x => x.Name == name, cancellationToken);
        }

        public Task<bool> IsUsedAsync(Guid exerciseId, CancellationToken cancellationToken = default)
        {
            return DbContext.Set<ExerciseSetRecord>()
                .AsNoTracking()
                .AnyAsync(x => x.ExerciseRecordId == exerciseId, cancellationToken);
        }

        protected override async Task<IReadOnlyCollection<ExerciseReadModel>> ListAsync(
            IQueryable<ExerciseRecord> query,
            CancellationToken cancellationToken)
        {
            var exercises = await Project(query.OrderBy(x => x.Name)).ToListAsync(cancellationToken);

            return await WithBestResultsAsync(exercises, cancellationToken);
        }

        protected override async Task<ExerciseReadModel?> DetailsAsync(
            IQueryable<ExerciseRecord> query,
            CancellationToken cancellationToken)
        {
            var exercise = await Project(query).FirstOrDefaultAsync(cancellationToken);

            return exercise is null
                ? null
                : (await WithBestResultsAsync([exercise], cancellationToken)).First();
        }

        private async Task<IReadOnlyCollection<ExerciseReadModel>> WithBestResultsAsync(
            IReadOnlyCollection<ExerciseReadModel> exercises,
            CancellationToken cancellationToken)
        {
            if (exercises.Count == 0)
            {
                return exercises;
            }

            var best = await BestResultsAsync(exercises.Select(x => x.Id).ToList(), cancellationToken);

            return exercises
                .Select(x => best.TryGetValue(x.Id, out var result)
                    ? x with { BestWeight = result.Weight, BestWeightReps = result.Reps }
                    : x)
                .ToList();
        }

        private async Task<Dictionary<Guid, BestResult>> BestResultsAsync(
            IReadOnlyCollection<Guid> exerciseIds,
            CancellationToken cancellationToken)
        {
            var rows = await DbContext.Set<ExerciseSetRecord>()
                .AsNoTracking()
                .Where(x => x.WorkoutRecordId != null && exerciseIds.Contains(x.ExerciseRecordId))
                .Select(x => new PerformedExerciseSet(x.ExerciseRecordId, x.WorkingSets))
                .ToListAsync(cancellationToken);

            return rows
                .SelectMany(row => row.WorkingSets
                    .Where(workingSet => workingSet.IsCompleted)
                    .Select(workingSet => new
                    {
                        row.ExerciseId,
                        workingSet.RepScheme.Weight,
                        workingSet.RepScheme.Reps
                    }))
                .GroupBy(x => x.ExerciseId)
                .ToDictionary(
                    x => x.Key,
                    x => x
                        .OrderByDescending(item => item.Weight)
                        .ThenByDescending(item => item.Reps)
                        .Select(item => new BestResult(item.Weight, item.Reps))
                        .First());
        }

        private static IQueryable<ExerciseReadModel> Project(IQueryable<ExerciseRecord> query)
        {
            return query.Select(x => new ExerciseReadModel(x.Id, x.Name, x.Description, x.MuscleGroup, 0, 0));
        }

        private sealed record PerformedExerciseSet(Guid ExerciseId, ICollection<WorkingSet> WorkingSets);

        private sealed record BestResult(double Weight, byte Reps);
    }
}
