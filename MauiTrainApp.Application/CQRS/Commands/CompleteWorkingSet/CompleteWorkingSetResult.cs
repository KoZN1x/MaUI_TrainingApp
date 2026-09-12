
namespace MauiTrainApp.Application.CQRS.Commands.CompleteWorkingSet;

public sealed record CompleteWorkingSetResult(Guid WorkoutId, bool IsWorkoutCompleted);
