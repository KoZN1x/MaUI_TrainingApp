using MauiTrainApp.Core.CQRS.Interfaces;

namespace MauiTrainApp.Application.CQRS.Commands.AddPerformedWorkingSet;

public sealed record AddPerformedWorkingSetCommand(Guid WorkoutId, Guid ExerciseSetId)
    : ICommand<AddPerformedWorkingSetResult>;
