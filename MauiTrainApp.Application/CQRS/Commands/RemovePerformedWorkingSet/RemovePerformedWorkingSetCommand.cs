using MauiTrainApp.Core.CQRS.Interfaces;

namespace MauiTrainApp.Application.CQRS.Commands.RemovePerformedWorkingSet;

public sealed record RemovePerformedWorkingSetCommand(
    Guid WorkoutId,
    Guid ExerciseSetId,
    int Index) : ICommand<RemovePerformedWorkingSetResult>;
