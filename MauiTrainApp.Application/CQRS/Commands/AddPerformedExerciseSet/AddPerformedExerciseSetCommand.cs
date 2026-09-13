using MauiTrainApp.Core.CQRS.Interfaces;

namespace MauiTrainApp.Application.CQRS.Commands.AddPerformedExerciseSet;

public sealed record AddPerformedExerciseSetCommand(
    Guid WorkoutId,
    Guid ExerciseId,
    int Sets,
    byte Reps,
    double Weight) : ICommand<AddPerformedExerciseSetResult>;
