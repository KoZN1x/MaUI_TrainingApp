using MauiTrainApp.Core.CQRS;
using MauiTrainApp.Core.CQRS.Interfaces;

namespace MauiTrainApp.Application.CQRS.Commands.CreateTrainingPlan;

public sealed record PlannedWorkingSet(byte Reps, double Weight);

public sealed record PlannedExerciseSet(Guid ExerciseId, IReadOnlyCollection<PlannedWorkingSet> WorkingSets);

public sealed record CreateTrainingPlanCommand(
    string Name,
    IReadOnlyCollection<PlannedExerciseSet> ExerciseSets) : ICommand<CreateTrainingPlanResult>;
