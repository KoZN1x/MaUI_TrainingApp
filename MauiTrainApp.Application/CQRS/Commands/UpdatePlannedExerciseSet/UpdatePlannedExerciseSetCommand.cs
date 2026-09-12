using MauiTrainApp.Core.CQRS.Interfaces;

namespace MauiTrainApp.Application.CQRS.Commands.UpdatePlannedExerciseSet;

public sealed record UpdatePlannedExerciseSetCommand(
    Guid TrainingPlanId,
    Guid ExerciseSetId,
    int Sets,
    byte Reps,
    double Weight) : ICommand<UpdatePlannedExerciseSetResult>;
