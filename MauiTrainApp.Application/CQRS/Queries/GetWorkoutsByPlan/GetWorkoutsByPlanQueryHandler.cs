using MauiTrainApp.Core.CQRS;
using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Domain.Interfaces;

namespace MauiTrainApp.Application.CQRS.Queries.GetWorkoutsByPlan;

internal sealed class GetWorkoutsByPlanQueryHandler
    : IQueryHandler<GetWorkoutsByPlanQuery, GetWorkoutsByPlanResult>
{
    private readonly IWorkoutReadRepository _workouts;

    public GetWorkoutsByPlanQueryHandler(IWorkoutReadRepository workouts)
    {
        _workouts = workouts;
    }

    public async Task<GetWorkoutsByPlanResult> HandleAsync(
        GetWorkoutsByPlanQuery query,
        CancellationToken cancellationToken = default)
    {
        return new GetWorkoutsByPlanResult(
            await _workouts.GetByTrainingPlanAsync(query.TrainingPlanId, cancellationToken));
    }
}
