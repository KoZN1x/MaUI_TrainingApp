using MauiTrainApp.Domain.ReadModels;

namespace MauiTrainApp.Application.CQRS.Queries.GetTrainingPlanDetails;

public sealed record GetTrainingPlanDetailsResult(TrainingPlanDetailsReadModel? TrainingPlan);
