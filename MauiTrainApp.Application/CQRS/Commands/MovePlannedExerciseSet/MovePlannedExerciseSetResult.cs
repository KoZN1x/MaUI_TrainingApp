namespace MauiTrainApp.Application.CQRS.Commands.MovePlannedExerciseSet;

public sealed record MovePlannedExerciseSetResult(Guid TrainingPlanId, IReadOnlyList<Guid> Order);
