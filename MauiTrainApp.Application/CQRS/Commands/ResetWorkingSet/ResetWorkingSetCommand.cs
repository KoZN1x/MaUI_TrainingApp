using MauiTrainApp.Core.CQRS.Interfaces;

namespace MauiTrainApp.Application.CQRS.Commands.ResetWorkingSet;

public sealed record ResetWorkingSetCommand(
    Guid WorkoutId,
    Guid ExerciseSetId,
    int WorkingSetIndex) : ICommand<ResetWorkingSetResult>;
