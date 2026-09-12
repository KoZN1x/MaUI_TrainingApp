using MauiTrainApp.Domain.ReadModels;

namespace MauiTrainApp.Application.CQRS.Queries.GetExerciseProgress;

public sealed record GetExerciseProgressResult(ExerciseProgressReadModel Progress);
