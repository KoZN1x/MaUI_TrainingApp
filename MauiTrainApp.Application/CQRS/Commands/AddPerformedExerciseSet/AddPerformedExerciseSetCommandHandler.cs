using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Domain.Exceptions;
using MauiTrainApp.Domain.Interfaces;
using MauiTrainApp.Domain.ValueObjects;

namespace MauiTrainApp.Application.CQRS.Commands.AddPerformedExerciseSet;

internal sealed class AddPerformedExerciseSetCommandHandler
    : ICommandHandler<AddPerformedExerciseSetCommand, AddPerformedExerciseSetResult>
{
    private const int MinimumSets = 1;
    private const int MaximumSets = 20;

    private readonly IWriteRepository<Workout> _workouts;
    private readonly IWriteRepository<Exercise> _exercises;

    public AddPerformedExerciseSetCommandHandler(
        IWriteRepository<Workout> workouts,
        IWriteRepository<Exercise> exercises)
    {
        _workouts = workouts;
        _exercises = exercises;
    }

    public async Task<AddPerformedExerciseSetResult> HandleAsync(
        AddPerformedExerciseSetCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.Sets is < MinimumSets or > MaximumSets)
        {
            throw new InvariantException($"Sets count {command.Sets} is out of range");
        }

        var workout = await _workouts.GetByIdAsync(command.WorkoutId, cancellationToken)
            ?? throw NotFoundException.For<Workout>(command.WorkoutId);

        if (workout.Duration is not null)
        {
            throw new InvariantException("Completed workout couldn't be changed");
        }

        if (workout.ExerciseSets.Any(x => x.Exercise.Id == command.ExerciseId))
        {
            throw new InvariantException("Exercise is already a part of the workout");
        }

        var exercise = await _exercises.GetByIdAsync(command.ExerciseId, cancellationToken)
            ?? throw NotFoundException.For<Exercise>(command.ExerciseId);

        var workingSet = new WorkingSet
        {
            RepScheme = new RepScheme
            {
                Reps = command.Reps,
                Weight = command.Weight
            }
        };

        var exerciseSet = new ExerciseSet(exercise, Enumerable.Repeat(workingSet, command.Sets));

        workout.AddExerciseSet(exerciseSet);

        await _workouts.UpdateAsync(workout, cancellationToken);

        return new AddPerformedExerciseSetResult(workout.Id, exerciseSet.Id);
    }
}
