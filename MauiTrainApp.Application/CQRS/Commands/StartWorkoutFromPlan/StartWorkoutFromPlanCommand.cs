using MauiTrainApp.Core.CQRS;
using MauiTrainApp.Core.CQRS.Interfaces;

namespace MauiTrainApp.Application.CQRS.Commands.StartWorkoutFromPlan;

public sealed record StartWorkoutFromPlanCommand(
    Guid TrainingPlanId,
    DateOnly WorkoutDay) : ICommand<StartWorkoutFromPlanResult>;
