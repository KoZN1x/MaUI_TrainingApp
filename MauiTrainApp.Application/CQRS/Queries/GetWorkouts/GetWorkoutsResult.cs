using MauiTrainApp.Domain.ReadModels;

namespace MauiTrainApp.Application.CQRS.Queries.GetWorkouts;

public sealed record GetWorkoutsResult(IReadOnlyCollection<WorkoutListItemReadModel> Workouts);
