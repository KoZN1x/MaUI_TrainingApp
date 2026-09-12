using MauiTrainApp.Core.CQRS;
using MauiTrainApp.Core.CQRS.Interfaces;

namespace MauiTrainApp.Application.CQRS.Commands.CompleteExerciseSet;

public sealed record CompleteExerciseSetCommand(
    Guid WorkoutId,
    Guid ExerciseSetId) : ICommand<CompleteExerciseSetResult>;
