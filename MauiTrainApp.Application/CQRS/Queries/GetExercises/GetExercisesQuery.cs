using MauiTrainApp.Core.CQRS;
using MauiTrainApp.Core.CQRS.Interfaces;

namespace MauiTrainApp.Application.CQRS.Queries.GetExercises;

public sealed record GetExercisesQuery(string? SearchTerm = null) : IQuery<GetExercisesResult>;
