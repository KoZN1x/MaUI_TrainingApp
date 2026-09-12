namespace MauiTrainApp.Domain.ReadModels;

public sealed record TrainingPlanDetailsReadModel(
    Guid Id,
    string Name,
    IReadOnlyCollection<ExerciseSetReadModel> ExerciseSets);
