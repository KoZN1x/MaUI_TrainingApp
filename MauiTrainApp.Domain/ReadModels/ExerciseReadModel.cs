using MauiTrainApp.Domain.Enums;

namespace MauiTrainApp.Domain.ReadModels;

public sealed record ExerciseReadModel(Guid Id, string Name, string? Description, MuscleGroup MuscleGroup);
