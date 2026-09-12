namespace MauiTrainApp.Application.CQRS.Commands.UpdatePlannedExerciseSet;

public sealed record UpdatePlannedExerciseSetResult(Guid TrainingPlanId, Guid ExerciseSetId);
