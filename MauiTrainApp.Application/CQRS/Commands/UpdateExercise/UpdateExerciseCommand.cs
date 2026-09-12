using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Domain.Enums;

namespace MauiTrainApp.Application.CQRS.Commands.UpdateExercise;

public sealed record UpdateExerciseCommand(
    Guid ExerciseId,
    string Name,
    string? Description,
    MuscleGroup MuscleGroup) : ICommand<UpdateExerciseResult>;
