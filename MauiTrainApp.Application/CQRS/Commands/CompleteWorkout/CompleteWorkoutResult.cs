namespace MauiTrainApp.Application.CQRS.Commands.CompleteWorkout;

public sealed record CompleteWorkoutResult(Guid WorkoutId, bool IsCompleted, double TotalVolume);
