
namespace MauiTrainApp.Application.CQRS.Commands.CompleteExerciseSet;

public sealed record CompleteExerciseSetResult(Guid WorkoutId, bool IsWorkoutCompleted);
