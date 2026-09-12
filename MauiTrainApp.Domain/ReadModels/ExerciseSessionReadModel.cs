namespace MauiTrainApp.Domain.ReadModels;

public sealed record ExerciseSessionReadModel(
    Guid WorkoutId,
    DateOnly WorkoutDay,
    IReadOnlyCollection<WorkingSetReadModel> WorkingSets,
    double Volume,
    double BestWeight,
    byte BestWeightReps);
