namespace MauiTrainApp.Domain.ReadModels;

public sealed record WeeklyVolumeReadModel(DateOnly WeekStart, double Volume, int WorkoutCount);
