using MauiTrainApp.Domain.Entities;

namespace MauiTrainApp.Domain.Interfaces;

public interface IExerciseRepository : IRepository<Exercise>
{
    Task<ICollection<Exercise>> SearchByNameAsync(string name, CancellationToken cancellationToken = default);
}
