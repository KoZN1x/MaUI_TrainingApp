using MauiTrainApp.Domain.Entities;

namespace MauiTrainApp.Domain.Interfaces;

public interface IWorkoutRepository : IRepository<Workout>
{
    Task<ICollection<Workout>> GetByPeriodAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);

    Task<ICollection<Workout>> GetByTrainingPlanAsync(Guid trainingPlanId, CancellationToken cancellationToken = default);
}
