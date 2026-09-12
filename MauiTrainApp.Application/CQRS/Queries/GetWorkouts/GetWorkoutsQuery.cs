using MauiTrainApp.Core.CQRS.Interfaces;

namespace MauiTrainApp.Application.CQRS.Queries.GetWorkouts;

public sealed record GetWorkoutsQuery(DateOnly? From = null, DateOnly? To = null) : IQuery<GetWorkoutsResult>;
