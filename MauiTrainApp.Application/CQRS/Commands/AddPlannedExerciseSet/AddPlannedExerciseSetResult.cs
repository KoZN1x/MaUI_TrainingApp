namespace MauiTrainApp.Application.CQRS.Commands.AddPlannedExerciseSet;

public sealed record AddPlannedExerciseSetResult(Guid TrainingPlanId, Guid ExerciseSetId);
