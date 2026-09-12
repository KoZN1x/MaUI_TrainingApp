using MauiTrainApp.Domain.ReadModels;

namespace MauiTrainApp.Application.CQRS.Queries.GetActiveWorkout;

public sealed record GetActiveWorkoutResult(WorkoutListItemReadModel? Workout)
{
    public bool HasActiveWorkout => Workout is not null;
}
