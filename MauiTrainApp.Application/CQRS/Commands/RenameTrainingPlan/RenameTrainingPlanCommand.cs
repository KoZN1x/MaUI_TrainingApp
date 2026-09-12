using MauiTrainApp.Core.CQRS.Interfaces;

namespace MauiTrainApp.Application.CQRS.Commands.RenameTrainingPlan;

public sealed record RenameTrainingPlanCommand(Guid TrainingPlanId, string Name)
    : ICommand<RenameTrainingPlanResult>;
