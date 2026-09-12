using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Core.CQRS;
using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Domain.Exceptions;
using MauiTrainApp.Domain.Interfaces;
using MauiTrainApp.Domain.ValueObjects;

namespace MauiTrainApp.Application.CQRS.Commands.CreateTrainingPlan;

internal sealed class CreateTrainingPlanCommandHandler
    : ICommandHandler<CreateTrainingPlanCommand, CreateTrainingPlanResult>
{
    private readonly IWriteRepository<TrainingPlan> _trainingPlans;
    private readonly IWriteRepository<Exercise> _exercises;

    public CreateTrainingPlanCommandHandler(
        IWriteRepository<TrainingPlan> trainingPlans,
        IWriteRepository<Exercise> exercises)
    {
        _trainingPlans = trainingPlans;
        _exercises = exercises;
    }

    public async Task<CreateTrainingPlanResult> HandleAsync(
        CreateTrainingPlanCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command.ExerciseSets);

        var exerciseSets = new List<ExerciseSet>(command.ExerciseSets.Count);

        foreach (var plannedSet in command.ExerciseSets)
        {
            var exercise = await _exercises.GetByIdAsync(plannedSet.ExerciseId, cancellationToken)
                ?? throw NotFoundException.For<Exercise>(plannedSet.ExerciseId);

            exerciseSets.Add(new ExerciseSet(exercise, plannedSet.WorkingSets.Select(ToWorkingSet)));
        }

        var trainingPlan = new TrainingPlan(command.Name, exerciseSets);

        await _trainingPlans.AddAsync(trainingPlan, cancellationToken);

        return new CreateTrainingPlanResult(trainingPlan.Id);
    }

    private static WorkingSet ToWorkingSet(PlannedWorkingSet plannedWorkingSet) =>
        new()
        {
            RepScheme = new RepScheme
            {
                Reps = plannedWorkingSet.Reps,
                Weight = plannedWorkingSet.Weight
            }
        };
}
