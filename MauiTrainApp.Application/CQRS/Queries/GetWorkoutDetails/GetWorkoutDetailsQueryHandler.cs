using MauiTrainApp.Core.CQRS;
using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Domain.Interfaces;

namespace MauiTrainApp.Application.CQRS.Queries.GetWorkoutDetails;

internal sealed class GetWorkoutDetailsQueryHandler
    : IQueryHandler<GetWorkoutDetailsQuery, GetWorkoutDetailsResult>
{
    private readonly IWorkoutReadRepository _workouts;

    public GetWorkoutDetailsQueryHandler(IWorkoutReadRepository workouts)
    {
        _workouts = workouts;
    }

    public async Task<GetWorkoutDetailsResult> HandleAsync(
        GetWorkoutDetailsQuery query,
        CancellationToken cancellationToken = default)
    {
        return new GetWorkoutDetailsResult(await _workouts.GetByIdAsync(query.WorkoutId, cancellationToken));
    }
}
