using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Domain.Exceptions;
using MauiTrainApp.Domain.Interfaces;
using MauiTrainApp.Domain.ValueObjects;

namespace MauiTrainApp.Application.CQRS.Commands.AddPlannedExerciseSet;

internal sealed class AddPlannedExerciseSetCommandHandler
    : ICommandHandler<AddPlannedExerciseSetCommand, AddPlannedExerciseSetResult>
{
    private readonly IWriteRepository<TrainingPlan> _trainingPlans;
    private readonly IWriteRepository<Exercise> _exercises;

    public AddPlannedExerciseSetCommandHandler(
        IWriteRepository<TrainingPlan> trainingPlans,
        IWriteRepository<Exercise> exercises)
    {
        _trainingPlans = trainingPlans;
        _exercises = exercises;
    }

    public async Task<AddPlannedExerciseSetResult> HandleAsync(
        AddPlannedExerciseSetCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.Sets <= 0)
        {
            throw new InvariantException("Planned exercise needs at least one working set");
        }

        var trainingPlan = await _trainingPlans.GetByIdAsync(command.TrainingPlanId, cancellationToken)
            ?? throw NotFoundException.For<TrainingPlan>(command.TrainingPlanId);

        var exercise = await _exercises.GetByIdAsync(command.ExerciseId, cancellationToken)
            ?? throw NotFoundException.For<Exercise>(command.ExerciseId);

        if (trainingPlan.ExerciseSets.Any(x => x.Exercise.Id == exercise.Id))
        {
            throw new InvariantException($"Exercise {exercise.Name} is already a part of {trainingPlan.Name}");
        }

        var workingSet = new WorkingSet
        {
            RepScheme = new RepScheme
            {
                Reps = command.Reps,
                Weight = command.Weight
            }
        };

        var exerciseSet = new ExerciseSet(exercise, Enumerable.Repeat(workingSet, command.Sets));

        trainingPlan.AddExerciseSet(exerciseSet);

        await _trainingPlans.UpdateAsync(trainingPlan, cancellationToken);

        return new AddPlannedExerciseSetResult(trainingPlan.Id, exerciseSet.Id);
    }
}
