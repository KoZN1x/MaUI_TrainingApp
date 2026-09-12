using MauiTrainApp.Core.CQRS.Interfaces;

namespace MauiTrainApp.Application.CQRS.Commands.RemovePlannedExerciseSet;

public sealed record RemovePlannedExerciseSetCommand(Guid TrainingPlanId, Guid ExerciseSetId)
    : ICommand<RemovePlannedExerciseSetResult>;
