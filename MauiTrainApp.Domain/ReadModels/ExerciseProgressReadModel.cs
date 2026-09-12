using MauiTrainApp.Domain.Enums;

namespace MauiTrainApp.Domain.ReadModels;

public sealed record ExerciseProgressReadModel(
    Guid ExerciseId,
    string Name,
    string? Description,
    MuscleGroup MuscleGroup,
    IReadOnlyCollection<string> TrainingPlanNames,
    IReadOnlyCollection<ExerciseSessionReadModel> Sessions)
{
    public int SessionCount => Sessions.Count;

    public double BestWeight => Sessions.Count == 0 ? 0 : Sessions.Max(x => x.BestWeight);

    public byte BestWeightReps => Sessions.Count == 0
        ? (byte)0
        : Sessions.OrderByDescending(x => x.BestWeight).First().BestWeightReps;

    public double BestVolume => Sessions.Count == 0 ? 0 : Sessions.Max(x => x.Volume);

    public double EstimatedOneRepMax => BestWeight <= 0
        ? 0
        : Math.Round(BestWeight * (1 + BestWeightReps / 30d));
}
