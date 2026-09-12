using MauiTrainApp.Core.CQRS.Interfaces;

namespace MauiTrainApp.Application.CQRS.Commands.CompleteWorkout;

public sealed record CompleteWorkoutCommand(Guid WorkoutId, DateTimeOffset CompletedAt)
    : ICommand<CompleteWorkoutResult>;
