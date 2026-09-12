using MauiTrainApp.Core.CQRS;
using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Domain.Interfaces;
using MauiTrainApp.Domain.ReadModels;

namespace MauiTrainApp.Application.CQRS.Queries.GetExercises;

internal sealed class GetExercisesQueryHandler : IQueryHandler<GetExercisesQuery, GetExercisesResult>
{
    private readonly IExerciseReadRepository _exercises;

    public GetExercisesQueryHandler(IExerciseReadRepository exercises)
    {
        _exercises = exercises;
    }

    public async Task<GetExercisesResult> HandleAsync(
        GetExercisesQuery query,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<ExerciseReadModel> exercises = string.IsNullOrWhiteSpace(query.SearchTerm)
            ? await _exercises.GetAllAsync(cancellationToken)
            : await _exercises.SearchByNameAsync(query.SearchTerm, cancellationToken);

        return new GetExercisesResult(exercises);
    }
}
