using MauiTrainApp.Domain.Enums;

namespace MauiTrainApp.Domain.ReadModels;

public sealed record MuscleVolumeReadModel(MuscleGroup MuscleGroup, double Volume, int WorkingSetCount);
