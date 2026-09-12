using MauiTrainApp.Domain.ReadModels;

namespace MauiTrainApp.Application.CQRS.Queries.GetTrainingPlans;

public sealed record GetTrainingPlansResult(IReadOnlyCollection<TrainingPlanListItemReadModel> TrainingPlans);
