namespace MauiTrainApp.Application.CQRS.Commands.AddPerformedExerciseSet;

public sealed record AddPerformedExerciseSetResult(Guid WorkoutId, Guid ExerciseSetId);
