using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Domain.Exceptions;
using MauiTrainApp.Domain.Interfaces;

namespace MauiTrainApp.Application.CQRS.Commands.CompleteWorkout;

internal sealed class CompleteWorkoutCommandHandler
    : ICommandHandler<CompleteWorkoutCommand, CompleteWorkoutResult>
{
    private readonly IWriteRepository<Workout> _workouts;

    public CompleteWorkoutCommandHandler(IWriteRepository<Workout> workouts)
    {
        _workouts = workouts;
    }

    public async Task<CompleteWorkoutResult> HandleAsync(
        CompleteWorkoutCommand command,
        CancellationToken cancellationToken = default)
    {
        var workout = await _workouts.GetByIdAsync(command.WorkoutId, cancellationToken)
            ?? throw NotFoundException.For<Workout>(command.WorkoutId);

        if (workout.ExerciseSets.All(x => !x.WorkingSets.Any(workingSet => workingSet.IsCompleted)))
        {
            throw new InvariantException("Workout without a single completed working set couldn't be finished");
        }

        workout.SetDuration(command.Duration);

        await _workouts.UpdateAsync(workout, cancellationToken);

        return new CompleteWorkoutResult(workout.Id, workout.IsCompleted, workout.TotalVolume);
    }
}
