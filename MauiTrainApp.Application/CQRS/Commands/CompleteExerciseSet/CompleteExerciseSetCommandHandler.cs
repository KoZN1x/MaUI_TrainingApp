using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Core.CQRS;
using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Domain.Exceptions;
using MauiTrainApp.Domain.Interfaces;

namespace MauiTrainApp.Application.CQRS.Commands.CompleteExerciseSet;

internal sealed class CompleteExerciseSetCommandHandler
    : ICommandHandler<CompleteExerciseSetCommand, CompleteExerciseSetResult>
{
    private readonly IWriteRepository<Workout> _workouts;

    public CompleteExerciseSetCommandHandler(IWriteRepository<Workout> workouts)
    {
        _workouts = workouts;
    }

    public async Task<CompleteExerciseSetResult> HandleAsync(
        CompleteExerciseSetCommand command,
        CancellationToken cancellationToken = default)
    {
        var workout = await _workouts.GetByIdAsync(command.WorkoutId, cancellationToken)
            ?? throw NotFoundException.For<Workout>(command.WorkoutId);

        var exerciseSet = workout.ExerciseSets.FirstOrDefault(x => x.Id == command.ExerciseSetId)
            ?? throw new InvariantException($"Exercise set '{command.ExerciseSetId}' is not a part of the workout");

        exerciseSet.Complete();

        await _workouts.UpdateAsync(workout, cancellationToken);

        return new CompleteExerciseSetResult(workout.Id, workout.IsCompleted);
    }
}
