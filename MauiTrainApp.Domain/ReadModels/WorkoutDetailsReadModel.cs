namespace MauiTrainApp.Domain.ReadModels;

public sealed record WorkoutDetailsReadModel(
    Guid Id,
    DateOnly WorkoutDay,
    Guid? TrainingPlanId,
    string? TrainingPlanName,
    IReadOnlyCollection<ExerciseSetReadModel> ExerciseSets);
