using MauiTrainApp.Domain.ValueObjects;

namespace MauiTrainApp.Domain.ReadModels;

public sealed record TrainingPlanListItemReadModel(
    Guid Id,
    string Name,
    int ExerciseSetCount,
    WeekSchedule Schedule);
