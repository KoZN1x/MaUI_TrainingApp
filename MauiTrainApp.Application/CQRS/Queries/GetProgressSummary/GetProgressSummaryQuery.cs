using MauiTrainApp.Core.CQRS.Interfaces;

namespace MauiTrainApp.Application.CQRS.Queries.GetProgressSummary;

public sealed record GetProgressSummaryQuery(DateOnly From, DateOnly To) : IQuery<GetProgressSummaryResult>;
