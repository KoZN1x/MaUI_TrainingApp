using MauiTrainApp.Domain.ReadModels;

namespace MauiTrainApp.Application.CQRS.Queries.GetWorkoutsByPlan;

public sealed record GetWorkoutsByPlanResult(IReadOnlyCollection<WorkoutListItemReadModel> Workouts);
