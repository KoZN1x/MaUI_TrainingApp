using MauiTrainApp.Core.CQRS;
using MauiTrainApp.Core.CQRS.Interfaces;

namespace MauiTrainApp.Application.CQRS.Commands.CompleteWorkingSet;

public sealed record CompleteWorkingSetCommand(
    Guid WorkoutId,
    Guid ExerciseSetId,
    int WorkingSetIndex) : ICommand<CompleteWorkingSetResult>;
