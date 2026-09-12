using MauiTrainApp.Core.CQRS.Interfaces;

namespace MauiTrainApp.Application.CQRS.Commands.UpdatePerformedWorkingSet;

public sealed record UpdatePerformedWorkingSetCommand(
    Guid WorkoutId,
    Guid ExerciseSetId,
    int WorkingSetIndex,
    byte Reps,
    double Weight) : ICommand<UpdatePerformedWorkingSetResult>;
