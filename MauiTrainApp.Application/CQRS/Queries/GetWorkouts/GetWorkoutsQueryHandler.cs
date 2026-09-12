using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Domain.Exceptions;
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
        if (query.From is { } from && query.To is { } to && to < from)
        {
            throw new InvariantException("Period end couldn't be earlier than its start");
        }

        return new GetWorkoutsResult(query switch
        {
            { From: null, To: null } => await _workouts.GetAllAsync(cancellationToken),
            _ => await _workouts.GetByPeriodAsync(
                query.From ?? DateOnly.MinValue,
                query.To ?? DateOnly.MaxValue,
                cancellationToken)
        });
    }
}
