using MauiTrainApp.Core.CQRS.Interfaces;

namespace MauiTrainApp.Application.CQRS.Queries.GetExerciseProgress;

public sealed record GetExerciseProgressQuery(Guid ExerciseId) : IQuery<GetExerciseProgressResult>;
