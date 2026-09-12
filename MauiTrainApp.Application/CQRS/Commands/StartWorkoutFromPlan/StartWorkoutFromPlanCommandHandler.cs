using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Domain.Exceptions;
using MauiTrainApp.Domain.Interfaces;

namespace MauiTrainApp.Application.CQRS.Commands.StartWorkoutFromPlan;

internal sealed class StartWorkoutFromPlanCommandHandler
    : ICommandHandler<StartWorkoutFromPlanCommand, StartWorkoutFromPlanResult>
{
    private readonly IWriteRepository<TrainingPlan> _trainingPlans;
    private readonly IWriteRepository<Workout> _workouts;
    private readonly IWorkoutReadRepository _workoutReader;

    public StartWorkoutFromPlanCommandHandler(
        IWriteRepository<TrainingPlan> trainingPlans,
        IWriteRepository<Workout> workouts,
        IWorkoutReadRepository workoutReader)
    {
        _trainingPlans = trainingPlans;
        _workouts = workouts;
        _workoutReader = workoutReader;
    }

    public async Task<StartWorkoutFromPlanResult> HandleAsync(
        StartWorkoutFromPlanCommand command,
        CancellationToken cancellationToken = default)
    {
        var trainingPlan = await _trainingPlans.GetByIdAsync(command.TrainingPlanId, cancellationToken)
            ?? throw NotFoundException.For<TrainingPlan>(command.TrainingPlanId);

        var previousWorkout = await LoadPreviousWorkoutAsync(trainingPlan.Id, cancellationToken);

        var workout = trainingPlan.StartWorkout(command.WorkoutDay, previousWorkout);

        await _workouts.AddAsync(workout, cancellationToken);

        return new StartWorkoutFromPlanResult(workout.Id, previousWorkout?.Id);
    }

    private async Task<Workout?> LoadPreviousWorkoutAsync(Guid trainingPlanId, CancellationToken cancellationToken)
    {
        var previousWorkoutId = await _workoutReader.GetLastWorkoutIdAsync(trainingPlanId, cancellationToken);

        return previousWorkoutId is null
            ? null
            : await _workouts.GetByIdAsync(previousWorkoutId.Value, cancellationToken);
    }
}
