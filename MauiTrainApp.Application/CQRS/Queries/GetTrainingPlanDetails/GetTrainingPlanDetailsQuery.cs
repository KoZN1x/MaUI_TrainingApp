using MauiTrainApp.Core.CQRS;
using MauiTrainApp.Core.CQRS.Interfaces;

namespace MauiTrainApp.Application.CQRS.Queries.GetTrainingPlanDetails;

public sealed record GetTrainingPlanDetailsQuery(Guid TrainingPlanId) : IQuery<GetTrainingPlanDetailsResult>;
