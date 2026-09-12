namespace MauiTrainApp.Application.CQRS.Commands.ResetWorkingSet;

public sealed record ResetWorkingSetResult(Guid WorkoutId, bool IsWorkoutCompleted);
