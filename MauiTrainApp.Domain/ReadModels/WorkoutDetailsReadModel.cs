namespace MauiTrainApp.Domain.ReadModels;

public sealed record WorkoutDetailsReadModel(
    Guid Id,
    DateOnly WorkoutDay,
    Guid? TrainingPlanId,
    string? TrainingPlanName,
    IReadOnlyCollection<ExerciseSetReadModel> ExerciseSets,
    TimeSpan? Duration)
{
    public double TotalVolume => ExerciseSets.Sum(x => x.CompletedVolume);

    public bool IsCompleted => ExerciseSets.Count > 0 && ExerciseSets.All(x => x.IsCompleted);
}
