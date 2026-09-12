using MauiTrainApp.Core.CQRS;
using MauiTrainApp.Core.CQRS.Interfaces;

namespace MauiTrainApp.Application.CQRS.Queries.GetWorkouts;

public sealed record GetWorkoutsQuery(DateOnly From, DateOnly To) : IQuery<GetWorkoutsResult>;
