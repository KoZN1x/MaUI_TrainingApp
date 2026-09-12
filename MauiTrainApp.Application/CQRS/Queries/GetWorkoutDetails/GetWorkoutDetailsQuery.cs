using MauiTrainApp.Core.CQRS;
using MauiTrainApp.Core.CQRS.Interfaces;

namespace MauiTrainApp.Application.CQRS.Queries.GetWorkoutDetails;

public sealed record GetWorkoutDetailsQuery(Guid WorkoutId) : IQuery<GetWorkoutDetailsResult>;
