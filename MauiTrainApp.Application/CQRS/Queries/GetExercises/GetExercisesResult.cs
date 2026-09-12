using MauiTrainApp.Domain.ReadModels;

namespace MauiTrainApp.Application.CQRS.Queries.GetExercises;

public sealed record GetExercisesResult(IReadOnlyCollection<ExerciseReadModel> Exercises);
