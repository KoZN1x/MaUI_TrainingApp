using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Core.CQRS;
using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Domain.Exceptions;
using MauiTrainApp.Domain.Interfaces;

namespace MauiTrainApp.Application.CQRS.Commands.StartWorkoutFromPlan;

internal sealed class StartWorkoutFromPlanCommandHandler
    : ICommandHandler<StartWorkoutFromPlanCommand, StartWorkoutFromPlanResult>
{
    private readonly IWriteRepository<TrainingPlan> _trainingPlans;
    private readonly IWriteRepository<Workout> _workouts;

    public StartWorkoutFromPlanCommandHandler(
        IWriteRepository<TrainingPlan> trainingPlans,
        IWriteRepository<Workout> workouts)
    {
        _trainingPlans = trainingPlans;
        _workouts = workouts;
    }

    public async Task<StartWorkoutFromPlanResult> HandleAsync(
        StartWorkoutFromPlanCommand command,
        CancellationToken cancellationToken = default)
    {
        var trainingPlan = await _trainingPlans.GetByIdAsync(command.TrainingPlanId, cancellationToken)
            ?? throw NotFoundException.For<TrainingPlan>(command.TrainingPlanId);

        var workout = trainingPlan.StartWorkout(command.WorkoutDay);

        await _workouts.AddAsync(workout, cancellationToken);

        return new StartWorkoutFromPlanResult(workout.Id);
    }
}
