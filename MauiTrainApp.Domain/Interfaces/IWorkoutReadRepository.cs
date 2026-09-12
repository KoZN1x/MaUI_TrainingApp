using MauiTrainApp.Domain.ReadModels;
using MauiTrainApp.Domain.Interfaces;

namespace MauiTrainApp.Domain.Interfaces;

public interface IWorkoutReadRepository : IReadRepository<WorkoutListItemReadModel, WorkoutDetailsReadModel>
{
    Task<IReadOnlyCollection<WorkoutListItemReadModel>> GetByPeriodAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<WorkoutListItemReadModel>> GetByTrainingPlanAsync(
        Guid trainingPlanId,
        CancellationToken cancellationToken = default);
}
