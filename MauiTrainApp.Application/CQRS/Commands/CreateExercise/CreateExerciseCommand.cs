using MauiTrainApp.Core.CQRS;
using MauiTrainApp.Core.CQRS.Interfaces;

namespace MauiTrainApp.Application.CQRS.Commands.CreateExercise;

public sealed record CreateExerciseCommand(string Name, string? Description = null) : ICommand<CreateExerciseResult>;
