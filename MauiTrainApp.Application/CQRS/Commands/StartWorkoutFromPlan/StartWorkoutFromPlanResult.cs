namespace MauiTrainApp.Application.CQRS.Commands.StartWorkoutFromPlan;

public sealed record StartWorkoutFromPlanResult(Guid WorkoutId, Guid? CarriedFromWorkoutId)
{
    public bool WeightsCarriedForward => CarriedFromWorkoutId is not null;
}
