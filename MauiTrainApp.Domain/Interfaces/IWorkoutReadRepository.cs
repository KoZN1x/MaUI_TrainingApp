using MauiTrainApp.Domain.ReadModels;

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

    Task<Guid?> GetLastWorkoutIdAsync(Guid trainingPlanId, CancellationToken cancellationToken = default);

    Task<WorkoutListItemReadModel?> GetActiveAsync(DateOnly day, CancellationToken cancellationToken = default);

    Task<Guid?> GetLastTrainingPlanIdAsync(CancellationToken cancellationToken = default);
}
