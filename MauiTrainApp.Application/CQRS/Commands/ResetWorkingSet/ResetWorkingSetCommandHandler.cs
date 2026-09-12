using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Domain.Exceptions;
using MauiTrainApp.Domain.Interfaces;

namespace MauiTrainApp.Application.CQRS.Commands.ResetWorkingSet;

internal sealed class ResetWorkingSetCommandHandler
    : ICommandHandler<ResetWorkingSetCommand, ResetWorkingSetResult>
{
    private readonly IWriteRepository<Workout> _workouts;

    public ResetWorkingSetCommandHandler(IWriteRepository<Workout> workouts)
    {
        _workouts = workouts;
    }

    public async Task<ResetWorkingSetResult> HandleAsync(
        ResetWorkingSetCommand command,
        CancellationToken cancellationToken = default)
    {
        var workout = await _workouts.GetByIdAsync(command.WorkoutId, cancellationToken)
            ?? throw NotFoundException.For<Workout>(command.WorkoutId);

        var exerciseSet = workout.ExerciseSets.FirstOrDefault(x => x.Id == command.ExerciseSetId)
            ?? throw new InvariantException($"Exercise set '{command.ExerciseSetId}' is not a part of the workout");

        exerciseSet.ResetWorkingSet(command.WorkingSetIndex);

        await _workouts.UpdateAsync(workout, cancellationToken);

        return new ResetWorkingSetResult(workout.Id, workout.IsCompleted);
    }
}
