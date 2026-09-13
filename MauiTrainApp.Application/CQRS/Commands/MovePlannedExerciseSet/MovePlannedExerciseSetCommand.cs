using MauiTrainApp.Core.CQRS.Interfaces;

namespace MauiTrainApp.Application.CQRS.Commands.MovePlannedExerciseSet;

public sealed record MovePlannedExerciseSetCommand(
    Guid TrainingPlanId,
    Guid ExerciseSetId,
    int NewIndex) : ICommand<MovePlannedExerciseSetResult>;
