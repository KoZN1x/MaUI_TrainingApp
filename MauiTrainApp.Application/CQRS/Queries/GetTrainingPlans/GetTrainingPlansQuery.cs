using MauiTrainApp.Core.CQRS;
using MauiTrainApp.Core.CQRS.Interfaces;

namespace MauiTrainApp.Application.CQRS.Queries.GetTrainingPlans;

public sealed record GetTrainingPlansQuery : IQuery<GetTrainingPlansResult>;
