using MauiTrainApp.Domain.Enums;

namespace MauiTrainApp.Domain.ReadModels;

public sealed record ExerciseReadModel(
    Guid Id,
    string Name,
    string? Description,
    MuscleGroup MuscleGroup,
    double BestWeight,
    byte BestWeightReps)
{
    public bool HasResult => BestWeight > 0;
}
