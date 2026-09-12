using MauiTrainApp.Domain.ReadModels;

namespace MauiTrainApp.Application.CQRS.Queries.GetWorkoutDetails;

public sealed record GetWorkoutDetailsResult(WorkoutDetailsReadModel? Workout);
