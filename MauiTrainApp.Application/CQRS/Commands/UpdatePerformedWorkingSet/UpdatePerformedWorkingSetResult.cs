namespace MauiTrainApp.Application.CQRS.Commands.UpdatePerformedWorkingSet;

public sealed record UpdatePerformedWorkingSetResult(
    Guid WorkoutId,
    Guid ExerciseSetId,
    int WorkingSetIndex);
