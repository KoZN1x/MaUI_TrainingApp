using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Domain.Exceptions;
using MauiTrainApp.Domain.Interfaces;

namespace MauiTrainApp.Application.CQRS.Commands.UpdateExercise;

internal sealed class UpdateExerciseCommandHandler
    : ICommandHandler<UpdateExerciseCommand, UpdateExerciseResult>
{
    private readonly IWriteRepository<Exercise> _exercises;

    public UpdateExerciseCommandHandler(IWriteRepository<Exercise> exercises)
    {
        _exercises = exercises;
    }

    public async Task<UpdateExerciseResult> HandleAsync(
        UpdateExerciseCommand command,
        CancellationToken cancellationToken = default)
    {
        var exercise = await _exercises.GetByIdAsync(command.ExerciseId, cancellationToken)
            ?? throw NotFoundException.For<Exercise>(command.ExerciseId);

        exercise.SetName(command.Name);
        exercise.SetDescription(command.Description);
        exercise.SetMuscleGroup(command.MuscleGroup);

        await _exercises.UpdateAsync(exercise, cancellationToken);

        return new UpdateExerciseResult(exercise.Id);
    }
}
