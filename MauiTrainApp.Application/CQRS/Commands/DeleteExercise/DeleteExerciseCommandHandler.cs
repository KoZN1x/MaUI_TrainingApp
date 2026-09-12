using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Domain.Exceptions;
using MauiTrainApp.Domain.Interfaces;

namespace MauiTrainApp.Application.CQRS.Commands.DeleteExercise;

internal sealed class DeleteExerciseCommandHandler : ICommandHandler<DeleteExerciseCommand, DeleteExerciseResult>
{
    private readonly IWriteRepository<Exercise> _exercises;
    private readonly IExerciseReadRepository _exerciseReader;

    public DeleteExerciseCommandHandler(
        IWriteRepository<Exercise> exercises,
        IExerciseReadRepository exerciseReader)
    {
        _exercises = exercises;
        _exerciseReader = exerciseReader;
    }

    public async Task<DeleteExerciseResult> HandleAsync(
        DeleteExerciseCommand command,
        CancellationToken cancellationToken = default)
    {
        var exercise = await _exercises.GetByIdAsync(command.ExerciseId, cancellationToken)
            ?? throw NotFoundException.For<Exercise>(command.ExerciseId);

        if (await _exerciseReader.IsUsedAsync(exercise.Id, cancellationToken))
        {
            throw new InvariantException(
                $"Упражнение «{exercise.Name}» входит в планы или проведённые тренировки и не может быть удалено");
        }

        await _exercises.DeleteAsync(exercise, cancellationToken);

        return new DeleteExerciseResult(exercise.Id);
    }
}
