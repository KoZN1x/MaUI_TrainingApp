using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Core.CQRS;
using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Domain.Exceptions;
using MauiTrainApp.Domain.Interfaces;

namespace MauiTrainApp.Application.CQRS.Commands.DeleteExercise;

internal sealed class DeleteExerciseCommandHandler : ICommandHandler<DeleteExerciseCommand, DeleteExerciseResult>
{
    private readonly IWriteRepository<Exercise> _exercises;

    public DeleteExerciseCommandHandler(IWriteRepository<Exercise> exercises)
    {
        _exercises = exercises;
    }

    public async Task<DeleteExerciseResult> HandleAsync(
        DeleteExerciseCommand command,
        CancellationToken cancellationToken = default)
    {
        var exercise = await _exercises.GetByIdAsync(command.ExerciseId, cancellationToken)
            ?? throw NotFoundException.For<Exercise>(command.ExerciseId);

        await _exercises.DeleteAsync(exercise, cancellationToken);

        return new DeleteExerciseResult(exercise.Id);
    }
}
