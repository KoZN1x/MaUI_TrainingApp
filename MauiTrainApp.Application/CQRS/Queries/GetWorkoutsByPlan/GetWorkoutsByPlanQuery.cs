using MauiTrainApp.Core.CQRS;
using MauiTrainApp.Core.CQRS.Interfaces;

namespace MauiTrainApp.Application.CQRS.Queries.GetWorkoutsByPlan;

public sealed record GetWorkoutsByPlanQuery(Guid TrainingPlanId) : IQuery<GetWorkoutsByPlanResult>;
