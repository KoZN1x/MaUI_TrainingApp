using MauiTrainApp.Core.CQRS;
using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Domain.Enums;

namespace MauiTrainApp.Application.CQRS.Commands.CreateExercise;

public sealed record CreateExerciseCommand(
    string Name,
    string? Description = null,
    MuscleGroup MuscleGroup = MuscleGroup.Other) : ICommand<CreateExerciseResult>;
