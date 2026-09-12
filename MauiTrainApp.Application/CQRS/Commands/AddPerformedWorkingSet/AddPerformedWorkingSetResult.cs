namespace MauiTrainApp.Application.CQRS.Commands.AddPerformedWorkingSet;

public sealed record AddPerformedWorkingSetResult(Guid WorkoutId, Guid ExerciseSetId, int WorkingSetIndex);
