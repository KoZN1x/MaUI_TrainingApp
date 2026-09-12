using MauiTrainApp.Core.CQRS;
using MauiTrainApp.Core.CQRS.Interfaces;

namespace MauiTrainApp.Application.CQRS.Commands.DeleteExercise;

public sealed record DeleteExerciseCommand(Guid ExerciseId) : ICommand<DeleteExerciseResult>;
