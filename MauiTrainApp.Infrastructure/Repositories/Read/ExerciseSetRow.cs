using MauiTrainApp.Domain.ReadModels;
using MauiTrainApp.Domain.ValueObjects;

namespace MauiTrainApp.Infrastructure.Repositories.Read
{
    internal sealed record ExerciseSetRow(
        Guid Id,
        Guid ExerciseId,
        string ExerciseName,
        ICollection<WorkingSet> WorkingSets);

    internal static class ExerciseSetRowExtensions
    {
        public static IReadOnlyCollection<ExerciseSetReadModel> ToReadModels(this IEnumerable<ExerciseSetRow> rows)
        {
            return rows
                .Select(x => new ExerciseSetReadModel(
                    x.Id,
                    x.ExerciseId,
                    x.ExerciseName,
                    x.WorkingSets
                        .Select(workingSet => new WorkingSetReadModel(
                            workingSet.RepScheme.Reps,
                            workingSet.RepScheme.Weight,
                            workingSet.IsCompleted))
                        .ToList()))
                .ToList();
        }

        public static int CountWorkingSets(this IEnumerable<ICollection<WorkingSet>> workingSets) =>
            workingSets.Sum(x => x.Count);

        public static int CountCompletedWorkingSets(this IEnumerable<ICollection<WorkingSet>> workingSets) =>
            workingSets.Sum(x => x.Count(workingSet => workingSet.IsCompleted));
    }
}
