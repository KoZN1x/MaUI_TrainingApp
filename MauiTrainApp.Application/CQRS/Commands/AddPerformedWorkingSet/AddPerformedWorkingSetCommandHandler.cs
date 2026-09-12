using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Domain.Exceptions;
using MauiTrainApp.Domain.Interfaces;
using MauiTrainApp.Domain.ValueObjects;

namespace MauiTrainApp.Application.CQRS.Commands.AddPerformedWorkingSet;

internal sealed class AddPerformedWorkingSetCommandHandler
    : ICommandHandler<AddPerformedWorkingSetCommand, AddPerformedWorkingSetResult>
{
    private const byte DefaultReps = 10;

    private readonly IWriteRepository<Workout> _workouts;

    public AddPerformedWorkingSetCommandHandler(IWriteRepository<Workout> workouts)
    {
        _workouts = workouts;
    }

    public async Task<AddPerformedWorkingSetResult> HandleAsync(
        AddPerformedWorkingSetCommand command,
        CancellationToken cancellationToken = default)
    {
        var workout = await _workouts.GetByIdAsync(command.WorkoutId, cancellationToken)
            ?? throw NotFoundException.For<Workout>(command.WorkoutId);

        var exerciseSet = workout.ExerciseSets.FirstOrDefault(x => x.Id == command.ExerciseSetId)
            ?? throw new InvariantException($"Exercise set {command.ExerciseSetId} is not a part of the workout");

        var last = exerciseSet.WorkingSets.LastOrDefault();

        exerciseSet.AddWorkingSet(new WorkingSet
        {
            RepScheme = new RepScheme
            {
                Reps = last.RepScheme.Reps == 0 ? DefaultReps : last.RepScheme.Reps,
                Weight = last.RepScheme.Weight
            }
        });

        await _workouts.UpdateAsync(workout, cancellationToken);

        return new AddPerformedWorkingSetResult(workout.Id, exerciseSet.Id, exerciseSet.WorkingSets.Count - 1);
    }
}
