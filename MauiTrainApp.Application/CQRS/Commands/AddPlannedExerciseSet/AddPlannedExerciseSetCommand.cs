using MauiTrainApp.Core.CQRS.Interfaces;

namespace MauiTrainApp.Application.CQRS.Commands.AddPlannedExerciseSet;

public sealed record AddPlannedExerciseSetCommand(
    Guid TrainingPlanId,
    Guid ExerciseId,
    int Sets = 3,
    byte Reps = 10,
    double Weight = 20) : ICommand<AddPlannedExerciseSetResult>;
