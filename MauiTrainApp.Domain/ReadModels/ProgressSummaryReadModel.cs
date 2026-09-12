namespace MauiTrainApp.Domain.ReadModels;

public sealed record ProgressSummaryReadModel(
    int WorkoutCount,
    double TotalVolume,
    int CompletedWorkingSetCount,
    int RecordCount,
    TimeSpan AverageDuration,
    double PreviousPeriodVolume,
    IReadOnlyCollection<WeeklyVolumeReadModel> WeeklyVolume,
    IReadOnlyCollection<MuscleVolumeReadModel> MuscleVolume)
{
    public double VolumeChangeRatio => PreviousPeriodVolume <= 0
        ? 0
        : (TotalVolume - PreviousPeriodVolume) / PreviousPeriodVolume;

    public double WorkoutsPerWeek => WeeklyVolume.Count == 0
        ? 0
        : (double)WorkoutCount / WeeklyVolume.Count;
}
