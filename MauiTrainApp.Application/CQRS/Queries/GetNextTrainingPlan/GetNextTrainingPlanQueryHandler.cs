using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Domain.Interfaces;
using MauiTrainApp.Domain.ReadModels;

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
            return new GetNextTrainingPlanResult(null, null);
        }

        var scheduled = await ByScheduleAsync(trainingPlans, query.Today, cancellationToken);

        return scheduled ?? await ByRotationAsync(trainingPlans, cancellationToken);
    }

    private async Task<GetNextTrainingPlanResult?> ByScheduleAsync(
        IReadOnlyCollection<TrainingPlanListItemReadModel> trainingPlans,
        DateOnly today,
        CancellationToken cancellationToken)
    {
        if (trainingPlans.All(x => x.Schedule.IsEmpty))
        {
            return null;
        }

        var trainedToday = (await _workouts.GetByPeriodAsync(today, today, cancellationToken))
            .Where(x => x.TrainingPlanId is not null)
            .Select(x => x.TrainingPlanId!.Value)
            .ToHashSet();

        var nearest = trainingPlans
            .Select(x => new { TrainingPlan = x, DaysUntil = DaysUntil(x, today, trainedToday) })
            .Where(x => x.DaysUntil is not null)
            .OrderBy(x => x.DaysUntil)
            .FirstOrDefault();

        return nearest is null
            ? null
            : new GetNextTrainingPlanResult(nearest.TrainingPlan, nearest.DaysUntil);
    }

    private async Task<GetNextTrainingPlanResult> ByRotationAsync(
        IReadOnlyCollection<TrainingPlanListItemReadModel> trainingPlans,
        CancellationToken cancellationToken)
    {
        var lastTrainingPlanId = await _workouts.GetLastTrainingPlanIdAsync(cancellationToken);

        var ordered = trainingPlans.ToList();
        var lastIndex = ordered.FindIndex(x => x.Id == lastTrainingPlanId);

        return new GetNextTrainingPlanResult(
            lastIndex < 0 ? ordered[0] : ordered[(lastIndex + 1) % ordered.Count],
            null);
    }

    private static int? DaysUntil(
        TrainingPlanListItemReadModel trainingPlan,
        DateOnly today,
        IReadOnlySet<Guid> trainedToday)
    {
        var daysUntil = trainingPlan.Schedule.DaysUntil(today.DayOfWeek);

        if (daysUntil != 0 || !trainedToday.Contains(trainingPlan.Id))
        {
            return daysUntil;
        }

        var next = trainingPlan.Schedule.DaysUntil(today.AddDays(1).DayOfWeek);

        return next is null ? null : next + 1;
    }
}
