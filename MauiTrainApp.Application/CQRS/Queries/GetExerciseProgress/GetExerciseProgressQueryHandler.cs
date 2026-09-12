using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Domain.Exceptions;
using MauiTrainApp.Domain.Interfaces;

namespace MauiTrainApp.Application.CQRS.Queries.GetExerciseProgress;

internal sealed class GetExerciseProgressQueryHandler
    : IQueryHandler<GetExerciseProgressQuery, GetExerciseProgressResult>
{
    private readonly IProgressReadRepository _progress;

    public GetExerciseProgressQueryHandler(IProgressReadRepository progress)
    {
        _progress = progress;
    }

    public async Task<GetExerciseProgressResult> HandleAsync(
        GetExerciseProgressQuery query,
        CancellationToken cancellationToken = default)
    {
        var progress = await _progress.GetExerciseProgressAsync(query.ExerciseId, cancellationToken)
            ?? throw NotFoundException.For<Exercise>(query.ExerciseId);

        return new GetExerciseProgressResult(progress);
    }
}
