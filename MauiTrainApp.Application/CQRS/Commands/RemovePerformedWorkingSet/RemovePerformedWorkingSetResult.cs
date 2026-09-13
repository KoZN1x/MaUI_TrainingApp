namespace MauiTrainApp.Application.CQRS.Commands.RemovePerformedWorkingSet;

public sealed record RemovePerformedWorkingSetResult(Guid WorkoutId, Guid ExerciseSetId, int RemainingCount);
