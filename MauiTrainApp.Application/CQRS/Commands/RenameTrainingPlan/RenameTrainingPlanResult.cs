namespace MauiTrainApp.Application.CQRS.Commands.RenameTrainingPlan;

public sealed record RenameTrainingPlanResult(Guid TrainingPlanId, string Name);
