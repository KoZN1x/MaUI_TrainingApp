using MauiTrainApp.Domain.Enums;
using MauiTrainApp.Domain.Interfaces;
using MauiTrainApp.Domain.ReadModels;
using MauiTrainApp.Domain.ValueObjects;
using MauiTrainApp.Infrastructure.Database;
using MauiTrainApp.Infrastructure.Records;
using Microsoft.EntityFrameworkCore;

namespace MauiTrainApp.Infrastructure.Repositories.Read
{
    internal sealed class ProgressReadRepository : IProgressReadRepository
    {
        private readonly MauiTrainAppDbContext _dbContext;

        public ProgressReadRepository(MauiTrainAppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private IQueryable<WorkoutRecord> Workouts => _dbContext.Set<WorkoutRecord>().AsNoTracking();

        public async Task<ProgressSummaryReadModel> GetSummaryAsync(
            DateOnly from,
            DateOnly to,
            CancellationToken cancellationToken = default)
        {
            var previousFrom = from.AddDays(-(to.DayNumber - from.DayNumber + 1));

            var rows = await Workouts
                .Where(x => x.WorkoutDay >= previousFrom && x.WorkoutDay <= to)
                .Select(x => new WorkoutVolumeRow(
                    x.WorkoutDay,
                    x.DurationSeconds,
                    x.ExerciseSets.Select(set => new MuscleWorkingSets(
                        set.Exercise.MuscleGroup,
                        set.WorkingSets)).ToList()))
                .ToListAsync(cancellationToken);

            var current = rows.Where(x => x.WorkoutDay >= from).ToList();

            return new ProgressSummaryReadModel(
                current.Count,
                current.Sum(CompletedVolume),
                current.Sum(CompletedWorkingSetCount),
                await CountRecordsAsync(from, to, cancellationToken),
                AverageDuration(current),
                rows.Where(x => x.WorkoutDay < from).Sum(CompletedVolume),
                WeeklyVolume(current, from, to),
                MuscleVolume(current));
        }

        public async Task<ExerciseProgressReadModel?> GetExerciseProgressAsync(
            Guid exerciseId,
            CancellationToken cancellationToken = default)
        {
            var exercise = await _dbContext.Set<ExerciseRecord>()
                .AsNoTracking()
                .Where(x => x.Id == exerciseId)
                .Select(x => new { x.Id, x.Name, x.Description, x.MuscleGroup })
                .FirstOrDefaultAsync(cancellationToken);

            if (exercise is null)
            {
                return null;
            }

            var planNames = await _dbContext.Set<ExerciseSetRecord>()
                .AsNoTracking()
                .Where(x => x.ExerciseRecordId == exerciseId && x.TrainingPlanRecordId != null)
                .Select(x => x.TrainingPlanRecordId!.Value)
                .Distinct()
                .Join(
                    _dbContext.Set<TrainingPlanRecord>().AsNoTracking(),
                    id => id,
                    plan => plan.Id,
                    (_, plan) => plan.Name)
                .ToListAsync(cancellationToken);

            var sessions = await Workouts
                .Where(x => x.ExerciseSets.Any(set => set.ExerciseRecordId == exerciseId))
                .OrderByDescending(x => x.WorkoutDay)
                .Select(x => new ExerciseSessionRow(
                    x.Id,
                    x.WorkoutDay,
                    x.ExerciseSets
                        .Where(set => set.ExerciseRecordId == exerciseId)
                        .Select(set => set.WorkingSets)
                        .ToList()))
                .ToListAsync(cancellationToken);

            return new ExerciseProgressReadModel(
                exercise.Id,
                exercise.Name,
                exercise.Description,
                exercise.MuscleGroup,
                planNames,
                sessions.Select(ToSession).Where(x => x.WorkingSets.Count > 0).ToList());
        }

        private async Task<int> CountRecordsAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken)
        {
            var rows = await Workouts
                .Where(x => x.WorkoutDay <= to)
                .Select(x => new ExerciseBestWorkout(
                    x.WorkoutDay,
                    x.ExerciseSets
                        .Select(set => new ExerciseWorkingSets(set.ExerciseRecordId, set.WorkingSets))
                        .ToList()))
                .ToListAsync(cancellationToken);

            var sessions = rows
                .SelectMany(workout => workout.ExerciseSets.Select(set => new
                {
                    set.ExerciseId,
                    workout.WorkoutDay,
                    BestWeight = set.WorkingSets
                        .Where(workingSet => workingSet.IsCompleted)
                        .Select(workingSet => workingSet.RepScheme.Weight)
                        .DefaultIfEmpty(0)
                        .Max()
                }))
                .Where(x => x.BestWeight > 0)
                .GroupBy(x => new { x.ExerciseId, x.WorkoutDay })
                .Select(x => new
                {
                    x.Key.ExerciseId,
                    x.Key.WorkoutDay,
                    BestWeight = x.Max(item => item.BestWeight)
                });

            var records = 0;

            foreach (var exercise in sessions.GroupBy(x => x.ExerciseId))
            {
                var best = 0d;
                var isFirstSession = true;

                foreach (var session in exercise.OrderBy(x => x.WorkoutDay))
                {
                    if (!isFirstSession && session.BestWeight > best && session.WorkoutDay >= from)
                    {
                        records++;
                    }

                    best = Math.Max(best, session.BestWeight);
                    isFirstSession = false;
                }
            }

            return records;
        }

        private static ExerciseSessionReadModel ToSession(ExerciseSessionRow row)
        {
            var completed = row.WorkingSets.SelectMany(x => x).Where(x => x.IsCompleted).ToList();
            var best = completed.OrderByDescending(x => x.RepScheme.Weight).FirstOrDefault();

            return new ExerciseSessionReadModel(
                row.WorkoutId,
                row.WorkoutDay,
                completed
                    .Select(x => new WorkingSetReadModel(x.RepScheme.Reps, x.RepScheme.Weight, true))
                    .ToList(),
                completed.Sum(x => x.RepScheme.Reps * x.RepScheme.Weight),
                best.RepScheme.Weight,
                best.RepScheme.Reps);
        }

        private static double CompletedVolume(WorkoutVolumeRow row) => row.ExerciseSets
            .SelectMany(x => x.WorkingSets)
            .Where(x => x.IsCompleted)
            .Sum(x => x.RepScheme.Reps * x.RepScheme.Weight);

        private static int CompletedWorkingSetCount(WorkoutVolumeRow row) => row.ExerciseSets
            .SelectMany(x => x.WorkingSets)
            .Count(x => x.IsCompleted);

        private static TimeSpan AverageDuration(IReadOnlyCollection<WorkoutVolumeRow> rows)
        {
            var durations = rows.Where(x => x.DurationSeconds is > 0).ToList();

            return durations.Count == 0
                ? TimeSpan.Zero
                : TimeSpan.FromSeconds(durations.Average(x => x.DurationSeconds!.Value));
        }

        private static IReadOnlyCollection<WeeklyVolumeReadModel> WeeklyVolume(
            IReadOnlyCollection<WorkoutVolumeRow> rows,
            DateOnly from,
            DateOnly to)
        {
            var weeks = new List<WeeklyVolumeReadModel>();

            for (var weekStart = StartOfWeek(from); weekStart <= to; weekStart = weekStart.AddDays(7))
            {
                var weekEnd = weekStart.AddDays(6);
                var inWeek = rows.Where(x => x.WorkoutDay >= weekStart && x.WorkoutDay <= weekEnd).ToList();

                weeks.Add(new WeeklyVolumeReadModel(weekStart, inWeek.Sum(CompletedVolume), inWeek.Count));
            }

            return weeks;
        }

        private static DateOnly StartOfWeek(DateOnly date)
        {
            var shift = ((int)date.DayOfWeek + 6) % 7;

            return date.AddDays(-shift);
        }

        private static IReadOnlyCollection<MuscleVolumeReadModel> MuscleVolume(
            IReadOnlyCollection<WorkoutVolumeRow> rows)
        {
            return rows
                .SelectMany(x => x.ExerciseSets)
                .SelectMany(x => x.WorkingSets.Where(set => set.IsCompleted).Select(set => new
                {
                    x.MuscleGroup,
                    Volume = set.RepScheme.Reps * set.RepScheme.Weight
                }))
                .GroupBy(x => x.MuscleGroup)
                .Select(x => new MuscleVolumeReadModel(x.Key, x.Sum(item => item.Volume), x.Count()))
                .OrderByDescending(x => x.Volume)
                .ToList();
        }

        private sealed record WorkoutVolumeRow(
            DateOnly WorkoutDay,
            int? DurationSeconds,
            ICollection<MuscleWorkingSets> ExerciseSets);

        private sealed record MuscleWorkingSets(MuscleGroup MuscleGroup, ICollection<WorkingSet> WorkingSets);

        private sealed record ExerciseBestWorkout(
            DateOnly WorkoutDay,
            ICollection<ExerciseWorkingSets> ExerciseSets);

        private sealed record ExerciseWorkingSets(Guid ExerciseId, ICollection<WorkingSet> WorkingSets);

        private sealed record ExerciseSessionRow(
            Guid WorkoutId,
            DateOnly WorkoutDay,
            ICollection<ICollection<WorkingSet>> WorkingSets);
    }
}
