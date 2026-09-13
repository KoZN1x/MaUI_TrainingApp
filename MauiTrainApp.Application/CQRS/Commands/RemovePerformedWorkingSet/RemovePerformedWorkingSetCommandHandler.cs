using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Domain.Exceptions;
using MauiTrainApp.Domain.Interfaces;

namespace MauiTrainApp.Application.CQRS.Commands.RemovePerformedWorkingSet;

internal sealed class RemovePerformedWorkingSetCommandHandler
    : ICommandHandler<RemovePerformedWorkingSetCommand, RemovePerformedWorkingSetResult>
{
    private readonly IWriteRepository<Workout> _workouts;

    public RemovePerformedWorkingSetCommandHandler(IWriteRepository<Workout> workouts)
    {
        _workouts = workouts;
    }

    public async Task<RemovePerformedWorkingSetResult> HandleAsync(
        RemovePerformedWorkingSetCommand command,
        CancellationToken cancellationToken = default)
    {
        var workout = await _workouts.GetByIdAsync(command.WorkoutId, cancellationToken)
            ?? throw NotFoundException.For<Workout>(command.WorkoutId);

        var exerciseSet = workout.ExerciseSets.FirstOrDefault(x => x.Id == command.ExerciseSetId)
            ?? throw new InvariantException($"Exercise set {command.ExerciseSetId} is not a part of the workout");

        exerciseSet.RemoveWorkingSetAt(command.Index);

        await _workouts.UpdateAsync(workout, cancellationToken);

        return new RemovePerformedWorkingSetResult(workout.Id, exerciseSet.Id, exerciseSet.WorkingSets.Count);
    }
}
