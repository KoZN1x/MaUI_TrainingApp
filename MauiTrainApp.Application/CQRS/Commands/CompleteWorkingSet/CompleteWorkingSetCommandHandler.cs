using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Core.CQRS;
using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Domain.Exceptions;
using MauiTrainApp.Domain.Interfaces;

namespace MauiTrainApp.Application.CQRS.Commands.CompleteWorkingSet;

internal sealed class CompleteWorkingSetCommandHandler
    : ICommandHandler<CompleteWorkingSetCommand, CompleteWorkingSetResult>
{
    private readonly IWriteRepository<Workout> _workouts;

    public CompleteWorkingSetCommandHandler(IWriteRepository<Workout> workouts)
    {
        _workouts = workouts;
    }

    public async Task<CompleteWorkingSetResult> HandleAsync(
        CompleteWorkingSetCommand command,
        CancellationToken cancellationToken = default)
    {
        var workout = await _workouts.GetByIdAsync(command.WorkoutId, cancellationToken)
            ?? throw NotFoundException.For<Workout>(command.WorkoutId);

        var exerciseSet = workout.ExerciseSets.FirstOrDefault(x => x.Id == command.ExerciseSetId)
            ?? throw new InvariantException($"Exercise set '{command.ExerciseSetId}' is not a part of the workout");

        exerciseSet.CompleteWorkingSet(command.WorkingSetIndex);

        await _workouts.UpdateAsync(workout, cancellationToken);

        return new CompleteWorkingSetResult(workout.Id, workout.IsCompleted);
    }
}
