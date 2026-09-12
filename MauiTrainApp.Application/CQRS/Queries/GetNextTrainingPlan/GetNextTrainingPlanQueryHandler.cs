using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Domain.Interfaces;

namespace MauiTrainApp.Application.CQRS.Queries.GetNextTrainingPlan;

internal sealed class GetNextTrainingPlanQueryHandler
    : IQueryHandler<GetNextTrainingPlanQuery, GetNextTrainingPlanResult>
{
    private readonly ITrainingPlanReadRepository _trainingPlans;
    private readonly IWorkoutReadRepository _workouts;

    public GetNextTrainingPlanQueryHandler(
        ITrainingPlanReadRepository trainingPlans,
        IWorkoutReadRepository workouts)
    {
        _trainingPlans = trainingPlans;
        _workouts = workouts;
    }

    public async Task<GetNextTrainingPlanResult> HandleAsync(
        GetNextTrainingPlanQuery query,
        CancellationToken cancellationToken = default)
    {
        var trainingPlans = await _trainingPlans.GetAllAsync(cancellationToken);

        if (trainingPlans.Count == 0)
        {
            return new GetNextTrainingPlanResult(null);
        }

        var lastTrainingPlanId = await _workouts.GetLastTrainingPlanIdAsync(cancellationToken);

        var ordered = trainingPlans.ToList();
        var lastIndex = ordered.FindIndex(x => x.Id == lastTrainingPlanId);

        return new GetNextTrainingPlanResult(lastIndex < 0
            ? ordered[0]
            : ordered[(lastIndex + 1) % ordered.Count]);
    }
}
