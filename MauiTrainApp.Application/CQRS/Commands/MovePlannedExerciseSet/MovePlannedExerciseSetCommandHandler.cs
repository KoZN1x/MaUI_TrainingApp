using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Domain.Exceptions;
using MauiTrainApp.Domain.Interfaces;

namespace MauiTrainApp.Application.CQRS.Commands.MovePlannedExerciseSet;

internal sealed class MovePlannedExerciseSetCommandHandler
    : ICommandHandler<MovePlannedExerciseSetCommand, MovePlannedExerciseSetResult>
{
    private readonly IWriteRepository<TrainingPlan> _trainingPlans;

    public MovePlannedExerciseSetCommandHandler(IWriteRepository<TrainingPlan> trainingPlans)
    {
        _trainingPlans = trainingPlans;
    }

    public async Task<MovePlannedExerciseSetResult> HandleAsync(
        MovePlannedExerciseSetCommand command,
        CancellationToken cancellationToken = default)
    {
        var trainingPlan = await _trainingPlans.GetByIdAsync(command.TrainingPlanId, cancellationToken)
            ?? throw NotFoundException.For<TrainingPlan>(command.TrainingPlanId);

        trainingPlan.MoveExerciseSet(command.ExerciseSetId, command.NewIndex);

        await _trainingPlans.UpdateAsync(trainingPlan, cancellationToken);

        return new MovePlannedExerciseSetResult(
            trainingPlan.Id,
            [.. trainingPlan.ExerciseSets.Select(x => x.Id)]);
    }
}
