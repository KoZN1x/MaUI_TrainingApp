using MauiTrainApp.Domain.ReadModels;
using MauiTrainApp.Domain.Interfaces;

namespace MauiTrainApp.Domain.Interfaces;

public interface IExerciseReadRepository : IReadRepository<ExerciseReadModel, ExerciseReadModel>
{
    Task<IReadOnlyCollection<ExerciseReadModel>> SearchByNameAsync(
        string searchTerm,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsWithNameAsync(string name, CancellationToken cancellationToken = default);

    Task<bool> IsUsedAsync(Guid exerciseId, CancellationToken cancellationToken = default);
}
