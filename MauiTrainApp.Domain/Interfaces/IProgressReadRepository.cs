using MauiTrainApp.Domain.ReadModels;

namespace MauiTrainApp.Domain.Interfaces;

public interface IProgressReadRepository
{
    Task<ProgressSummaryReadModel> GetSummaryAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default);

    Task<ExerciseProgressReadModel?> GetExerciseProgressAsync(
        Guid exerciseId,
        CancellationToken cancellationToken = default);
}
