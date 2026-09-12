using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Domain.Exceptions;
using MauiTrainApp.Domain.Interfaces;
using MauiTrainApp.Domain.ValueObjects;

namespace MauiTrainApp.Application.CQRS.Commands.UpdatePerformedWorkingSet;

internal sealed class UpdatePerformedWorkingSetCommandHandler
    : ICommandHandler<UpdatePerformedWorkingSetCommand, UpdatePerformedWorkingSetResult>
{
    private readonly IWriteRepository<Workout> _workouts;

    public UpdatePerformedWorkingSetCommandHandler(IWriteRepository<Workout> workouts)
    {
        _workouts = workouts;
    }

    public async Task<UpdatePerformedWorkingSetResult> HandleAsync(
        UpdatePerformedWorkingSetCommand command,
        CancellationToken cancellationToken = default)
    {
        var workout = await _workouts.GetByIdAsync(command.WorkoutId, cancellationToken)
            ?? throw NotFoundException.For<Workout>(command.WorkoutId);

        var exerciseSet = workout.ExerciseSets.FirstOrDefault(x => x.Id == command.ExerciseSetId)
            ?? throw new InvariantException($"Exercise set '{command.ExerciseSetId}' is not a part of the workout");

        if (command.WorkingSetIndex < 0 || command.WorkingSetIndex >= exerciseSet.WorkingSets.Count)
        {
            throw new InvariantException($"Working set index {command.WorkingSetIndex} is out of range");
        }

        var current = exerciseSet.WorkingSets.ElementAt(command.WorkingSetIndex);

        exerciseSet.UpdateWorkingSet(command.WorkingSetIndex, new WorkingSet
        {
            RepScheme = new RepScheme
            {
                Reps = command.Reps,
                Weight = command.Weight
            },
            IsCompleted = current.IsCompleted
        });

        await _workouts.UpdateAsync(workout, cancellationToken);

        return new UpdatePerformedWorkingSetResult(workout.Id, exerciseSet.Id, command.WorkingSetIndex);
    }
}
