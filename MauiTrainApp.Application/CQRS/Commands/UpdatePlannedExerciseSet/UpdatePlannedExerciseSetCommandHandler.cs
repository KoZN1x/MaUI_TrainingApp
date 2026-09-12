using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Domain.Exceptions;
using MauiTrainApp.Domain.Interfaces;
using MauiTrainApp.Domain.ValueObjects;

namespace MauiTrainApp.Application.CQRS.Commands.UpdatePlannedExerciseSet;

internal sealed class UpdatePlannedExerciseSetCommandHandler
    : ICommandHandler<UpdatePlannedExerciseSetCommand, UpdatePlannedExerciseSetResult>
{
    private readonly IWriteRepository<TrainingPlan> _trainingPlans;

    public UpdatePlannedExerciseSetCommandHandler(IWriteRepository<TrainingPlan> trainingPlans)
    {
        _trainingPlans = trainingPlans;
    }

    public async Task<UpdatePlannedExerciseSetResult> HandleAsync(
        UpdatePlannedExerciseSetCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.Sets <= 0)
        {
            throw new InvariantException("Exercise set should have at least one working set");
        }

        var trainingPlan = await _trainingPlans.GetByIdAsync(command.TrainingPlanId, cancellationToken)
            ?? throw NotFoundException.For<TrainingPlan>(command.TrainingPlanId);

        var exerciseSet = trainingPlan.ExerciseSets.FirstOrDefault(x => x.Id == command.ExerciseSetId)
            ?? throw new InvariantException($"Exercise set '{command.ExerciseSetId}' is not a part of the plan");

        var workingSet = new WorkingSet
        {
            RepScheme = new RepScheme
            {
                Reps = command.Reps,
                Weight = command.Weight
            }
        };

        exerciseSet.ReplaceWorkingSets(Enumerable.Repeat(workingSet, command.Sets));
        trainingPlan.UpdateExerciseSet(exerciseSet);

        await _trainingPlans.UpdateAsync(trainingPlan, cancellationToken);

        return new UpdatePlannedExerciseSetResult(trainingPlan.Id, exerciseSet.Id);
    }
}
