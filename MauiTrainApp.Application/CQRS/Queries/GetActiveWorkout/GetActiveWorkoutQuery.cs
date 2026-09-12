using MauiTrainApp.Core.CQRS.Interfaces;

namespace MauiTrainApp.Application.CQRS.Queries.GetActiveWorkout;

public sealed record GetActiveWorkoutQuery(DateOnly Day) : IQuery<GetActiveWorkoutResult>;
