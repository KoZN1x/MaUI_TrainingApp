namespace MauiTrainApp.Application.CQRS.Commands.RemovePlannedExerciseSet;

public sealed record RemovePlannedExerciseSetResult(Guid TrainingPlanId, int RemainingExerciseSetCount);
