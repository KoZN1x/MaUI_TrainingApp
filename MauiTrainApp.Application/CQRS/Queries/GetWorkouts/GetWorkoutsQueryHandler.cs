using MauiTrainApp.Core.CQRS;
using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Domain.Interfaces;

namespace MauiTrainApp.Application.CQRS.Queries.GetWorkouts;

internal sealed class GetWorkoutsQueryHandler : IQueryHandler<GetWorkoutsQuery, GetWorkoutsResult>
{
    private readonly IWorkoutReadRepository _workouts;

    public GetWorkoutsQueryHandler(IWorkoutReadRepository workouts)
    {
        _workouts = workouts;
    }

    public async Task<GetWorkoutsResult> HandleAsync(
        GetWorkoutsQuery query,
        CancellationToken cancellationToken = default)
    {
        return new GetWorkoutsResult(await _workouts.GetByPeriodAsync(query.From, query.To, cancellationToken));
    }
}
