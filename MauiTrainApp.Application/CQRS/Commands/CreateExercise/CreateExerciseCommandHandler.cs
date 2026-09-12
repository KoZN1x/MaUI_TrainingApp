using MauiTrainApp.Core.CQRS;
using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Domain.Exceptions;
using MauiTrainApp.Domain.Interfaces;

namespace MauiTrainApp.Application.CQRS.Commands.CreateExercise;

internal sealed class CreateExerciseCommandHandler : ICommandHandler<CreateExerciseCommand, CreateExerciseResult>
{
    private readonly IWriteRepository<Exercise> _exercises;
    private readonly IExerciseReadRepository _exerciseReader;

    public CreateExerciseCommandHandler(
        IWriteRepository<Exercise> exercises,
        IExerciseReadRepository exerciseReader)
    {
        _exercises = exercises;
        _exerciseReader = exerciseReader;
    }

    public async Task<CreateExerciseResult> HandleAsync(
        CreateExerciseCommand command,
        CancellationToken cancellationToken = default)
    {
        var exercise = new Exercise(command.Name, command.Description);

        if (await _exerciseReader.ExistsWithNameAsync(exercise.Name, cancellationToken))
        {
            throw new InvariantException($"Exercise '{exercise.Name}' already exists");
        }

        await _exercises.AddAsync(exercise, cancellationToken);

        return new CreateExerciseResult(exercise.Id);
    }
}
