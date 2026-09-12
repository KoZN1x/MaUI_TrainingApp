using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Core.CQRS;
using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Domain.Exceptions;
using MauiTrainApp.Domain.Interfaces;

namespace MauiTrainApp.Application.CQRS.Commands.DeleteWorkout;

internal sealed class DeleteWorkoutCommandHandler : ICommandHandler<DeleteWorkoutCommand, DeleteWorkoutResult>
{
    private readonly IWriteRepository<Workout> _workouts;

    public DeleteWorkoutCommandHandler(IWriteRepository<Workout> workouts)
    {
        _workouts = workouts;
    }

    public async Task<DeleteWorkoutResult> HandleAsync(
        DeleteWorkoutCommand command,
        CancellationToken cancellationToken = default)
    {
        var workout = await _workouts.GetByIdAsync(command.WorkoutId, cancellationToken)
            ?? throw NotFoundException.For<Workout>(command.WorkoutId);

        await _workouts.DeleteAsync(workout, cancellationToken);

        return new DeleteWorkoutResult(workout.Id);
    }
}
