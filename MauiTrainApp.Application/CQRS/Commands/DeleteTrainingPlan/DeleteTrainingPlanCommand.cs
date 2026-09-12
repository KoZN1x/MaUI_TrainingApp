using MauiTrainApp.Core.CQRS;
using MauiTrainApp.Core.CQRS.Interfaces;

namespace MauiTrainApp.Application.CQRS.Commands.DeleteTrainingPlan;

public sealed record DeleteTrainingPlanCommand(Guid TrainingPlanId) : ICommand<DeleteTrainingPlanResult>;
