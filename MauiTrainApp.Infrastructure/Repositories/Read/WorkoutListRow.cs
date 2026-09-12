using MauiTrainApp.Domain.ReadModels;
using MauiTrainApp.Domain.ValueObjects;

namespace MauiTrainApp.Infrastructure.Repositories.Read
{
    internal sealed record WorkoutListRow(
        Guid Id,
        DateOnly WorkoutDay,
        Guid? TrainingPlanRecordId,
        string? TrainingPlanName,
        int? DurationSeconds,
        DateTimeOffset CreatedAt,
        List<ICollection<WorkingSet>> WorkingSets);

    internal static class WorkoutListRowExtensions
    {
        public static WorkoutListItemReadModel ToReadModel(this WorkoutListRow row) =>
            new(
                row.Id,
                row.WorkoutDay,
                row.TrainingPlanRecordId,
                row.TrainingPlanName,
                row.WorkingSets.CountCompletedWorkingSets(),
                row.WorkingSets.CountWorkingSets(),
                row.WorkingSets.SumCompletedVolume(),
                row.DurationSeconds is null ? null : TimeSpan.FromSeconds(row.DurationSeconds.Value));
    }
}
