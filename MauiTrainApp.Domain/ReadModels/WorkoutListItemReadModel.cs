namespace MauiTrainApp.Domain.ReadModels;

public sealed record WorkoutListItemReadModel(
    Guid Id,
    DateOnly WorkoutDay,
    Guid? TrainingPlanId,
    string? TrainingPlanName,
    int CompletedWorkingSetCount,
    int TotalWorkingSetCount)
{
    public bool IsCompleted => TotalWorkingSetCount > 0 && CompletedWorkingSetCount == TotalWorkingSetCount;
}
