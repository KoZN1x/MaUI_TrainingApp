using MauiTrainApp.Domain.ReadModels;

namespace MauiTrainApp.Application.CQRS.Queries.GetNextTrainingPlan;

public sealed record GetNextTrainingPlanResult(TrainingPlanListItemReadModel? TrainingPlan);
