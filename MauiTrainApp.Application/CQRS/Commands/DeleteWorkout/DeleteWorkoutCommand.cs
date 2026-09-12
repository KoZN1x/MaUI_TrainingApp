using MauiTrainApp.Core.CQRS;
using MauiTrainApp.Core.CQRS.Interfaces;

namespace MauiTrainApp.Application.CQRS.Commands.DeleteWorkout;

public sealed record DeleteWorkoutCommand(Guid WorkoutId) : ICommand<DeleteWorkoutResult>;
