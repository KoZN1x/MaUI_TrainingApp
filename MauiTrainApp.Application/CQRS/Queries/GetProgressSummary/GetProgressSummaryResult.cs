using MauiTrainApp.Domain.ReadModels;

namespace MauiTrainApp.Application.CQRS.Queries.GetProgressSummary;

public sealed record GetProgressSummaryResult(ProgressSummaryReadModel Summary);
