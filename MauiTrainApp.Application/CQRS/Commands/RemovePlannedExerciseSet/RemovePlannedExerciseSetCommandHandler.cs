using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Domain.Exceptions;
using MauiTrainApp.Domain.Interfaces;

namespace MauiTrainApp.Application.CQRS.Commands.RemovePlannedExerciseSet;

internal sealed class RemovePlannedExerciseSetCommandHandler
    : ICommandHandler<RemovePlannedExerciseSetCommand, RemovePlannedExerciseSetResult>
{
    private readonly IWriteRepository<TrainingPlan> _trainingPlans;

    public RemovePlannedExerciseSetCommandHandler(IWriteRepository<TrainingPlan> trainingPlans)
    {
        _trainingPlans = trainingPlans;
    }

    public async Task<RemovePlannedExerciseSetResult> HandleAsync(
        RemovePlannedExerciseSetCommand command,
        CancellationToken cancellationToken = default)
    {
        var trainingPlan = await _trainingPlans.GetByIdAsync(command.TrainingPlanId, cancellationToken)
            ?? throw NotFoundException.For<TrainingPlan>(command.TrainingPlanId);

        trainingPlan.RemoveExerciseSet(command.ExerciseSetId);

        await _trainingPlans.UpdateAsync(trainingPlan, cancellationToken);

        return new RemovePlannedExerciseSetResult(trainingPlan.Id, trainingPlan.ExerciseSets.Count);
    }
}
