using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Domain.Interfaces;

namespace MauiTrainApp.Application.CQRS.Queries.GetActiveWorkout;

internal sealed class GetActiveWorkoutQueryHandler : IQueryHandler<GetActiveWorkoutQuery, GetActiveWorkoutResult>
{
    private readonly IWorkoutReadRepository _workouts;

    public GetActiveWorkoutQueryHandler(IWorkoutReadRepository workouts)
    {
        _workouts = workouts;
    }

    public async Task<GetActiveWorkoutResult> HandleAsync(
        GetActiveWorkoutQuery query,
        CancellationToken cancellationToken = default)
    {
        return new GetActiveWorkoutResult(await _workouts.GetActiveAsync(query.Day, cancellationToken));
    }
}
