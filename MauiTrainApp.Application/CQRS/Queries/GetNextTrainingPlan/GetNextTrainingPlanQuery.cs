using MauiTrainApp.Core.CQRS.Interfaces;

namespace MauiTrainApp.Application.CQRS.Queries.GetNextTrainingPlan;

public sealed record GetNextTrainingPlanQuery(DateOnly Today) : IQuery<GetNextTrainingPlanResult>;
