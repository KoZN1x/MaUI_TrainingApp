using MauiTrainApp.Application.CQRS.Commands.CompleteExerciseSet;
using MauiTrainApp.Application.CQRS.Commands.CreateExercise;
using MauiTrainApp.Application.CQRS.Commands.CreateTrainingPlan;
using MauiTrainApp.Application.CQRS.Commands.StartWorkoutFromPlan;
using MauiTrainApp.Application.CQRS.Commands.UpdatePerformedWorkingSet;
using MauiTrainApp.Application.CQRS.Commands.UpdatePlannedExerciseSet;
using MauiTrainApp.Application.CQRS.Queries.GetTrainingPlanDetails;
using MauiTrainApp.Application.CQRS.Queries.GetWorkoutDetails;
using MauiTrainApp.Application.DI;
using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Domain.Exceptions;
using MauiTrainApp.Domain.ReadModels;
using MauiTrainApp.Infrastructure.DI;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace MauiTrainApp.Tests.Application;

public class ProgressionTests : IAsyncLifetime
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"mauitrain-{Guid.NewGuid():N}.db");
    private ServiceProvider _provider = null!;

    public Task InitializeAsync()
    {
        _provider = new ServiceCollection()
            .AddInfrastructure(_databasePath)
            .AddApplication()
            .BuildServiceProvider();

        return _provider.MigrateDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        await _provider.DisposeAsync();

        SqliteConnection.ClearAllPools();

        foreach (var file in Directory.GetFiles(Path.GetTempPath(), $"{Path.GetFileName(_databasePath)}*"))
        {
            File.Delete(file);
        }
    }

    [Fact]
    public async Task NextWorkout_StartsFromWeightsOfThePreviousOne()
    {
        var sender = Sender();
        var planId = await CreateSquatPlanAsync(sender);

        var first = await sender.SendAsync(new StartWorkoutFromPlanCommand(planId, new DateOnly(2026, 9, 10)));
        Assert.False(first.WeightsCarriedForward);

        var squat = await SingleExerciseSetAsync(sender, first.WorkoutId);
        Assert.All(squat.WorkingSets, x => Assert.Equal(60, x.Weight));

        foreach (var index in Enumerable.Range(0, squat.WorkingSets.Count))
        {
            await sender.SendAsync(new UpdatePerformedWorkingSetCommand(
                first.WorkoutId, squat.Id, index, Reps: 10, Weight: 65));
        }

        await sender.SendAsync(new CompleteExerciseSetCommand(first.WorkoutId, squat.Id));

        var second = await sender.SendAsync(new StartWorkoutFromPlanCommand(planId, new DateOnly(2026, 9, 17)));

        Assert.True(second.WeightsCarriedForward);
        Assert.Equal(first.WorkoutId, second.CarriedFromWorkoutId);

        var carried = await SingleExerciseSetAsync(sender, second.WorkoutId);

        Assert.All(carried.WorkingSets, x => Assert.Equal(65, x.Weight));
        Assert.All(carried.WorkingSets, x => Assert.Equal(10, x.Reps));
        Assert.All(carried.WorkingSets, x => Assert.False(x.IsCompleted));
    }

    [Fact]
    public async Task PlanItself_StaysUntouchedByWorkoutProgress()
    {
        var sender = Sender();
        var planId = await CreateSquatPlanAsync(sender);

        var workout = await sender.SendAsync(new StartWorkoutFromPlanCommand(planId, new DateOnly(2026, 9, 10)));
        var squat = await SingleExerciseSetAsync(sender, workout.WorkoutId);

        await sender.SendAsync(new UpdatePerformedWorkingSetCommand(
            workout.WorkoutId, squat.Id, 0, Reps: 10, Weight: 65));

        var plan = (await sender.QueryAsync(new GetTrainingPlanDetailsQuery(planId))).TrainingPlan;

        Assert.All(plan!.ExerciseSets.Single().WorkingSets, x => Assert.Equal(60, x.Weight));
    }

    [Fact]
    public async Task ManualEdit_ChangesPlanAndNextWorkoutWithoutHistory()
    {
        var sender = Sender();
        var planId = await CreateSquatPlanAsync(sender);

        var plan = (await sender.QueryAsync(new GetTrainingPlanDetailsQuery(planId))).TrainingPlan;
        var plannedSquat = plan!.ExerciseSets.Single();

        await sender.SendAsync(new UpdatePlannedExerciseSetCommand(
            planId, plannedSquat.Id, Sets: 4, Reps: 8, Weight: 70));

        var updated = (await sender.QueryAsync(new GetTrainingPlanDetailsQuery(planId))).TrainingPlan;
        var workout = await sender.SendAsync(new StartWorkoutFromPlanCommand(planId, new DateOnly(2026, 9, 10)));
        var started = await SingleExerciseSetAsync(sender, workout.WorkoutId);

        Assert.Equal(4, updated!.ExerciseSets.Single().WorkingSets.Count);
        Assert.Equal(4, started.WorkingSets.Count);
        Assert.All(started.WorkingSets, x => Assert.Equal(70, x.Weight));
        Assert.All(started.WorkingSets, x => Assert.Equal(8, x.Reps));
    }

    [Fact]
    public async Task ManualEdit_DoesNotOverrideCarriedWeights()
    {
        var sender = Sender();
        var planId = await CreateSquatPlanAsync(sender);

        var first = await sender.SendAsync(new StartWorkoutFromPlanCommand(planId, new DateOnly(2026, 9, 10)));
        var squat = await SingleExerciseSetAsync(sender, first.WorkoutId);

        await sender.SendAsync(new UpdatePerformedWorkingSetCommand(
            first.WorkoutId, squat.Id, 0, Reps: 10, Weight: 65));

        var planSquatId = (await sender.QueryAsync(new GetTrainingPlanDetailsQuery(planId)))
            .TrainingPlan!.ExerciseSets.Single().Id;

        await sender.SendAsync(new UpdatePlannedExerciseSetCommand(
            planId, planSquatId, Sets: 3, Reps: 10, Weight: 50));

        var second = await sender.SendAsync(new StartWorkoutFromPlanCommand(planId, new DateOnly(2026, 9, 17)));
        var carried = await SingleExerciseSetAsync(sender, second.WorkoutId);

        Assert.Equal(65, carried.WorkingSets.First().Weight);
    }

    [Fact]
    public async Task UpdatePerformedWorkingSet_RejectsIndexOutOfRange()
    {
        var sender = Sender();
        var planId = await CreateSquatPlanAsync(sender);
        var workout = await sender.SendAsync(new StartWorkoutFromPlanCommand(planId, new DateOnly(2026, 9, 10)));
        var squat = await SingleExerciseSetAsync(sender, workout.WorkoutId);

        await Assert.ThrowsAsync<ValidationException>(
            () => sender.SendAsync(new UpdatePerformedWorkingSetCommand(
                workout.WorkoutId, squat.Id, 7, Reps: 10, Weight: 65)));
    }

    private static async Task<Guid> CreateSquatPlanAsync(ISender sender)
    {
        var exercise = await sender.SendAsync(new CreateExerciseCommand("Присед в Смите"));

        var plan = await sender.SendAsync(new CreateTrainingPlanCommand(
            "Фуллбади · Четверг",
            [
                new PlannedExerciseSet(exercise.ExerciseId,
                [
                    new PlannedWorkingSet(10, 60),
                    new PlannedWorkingSet(10, 60),
                    new PlannedWorkingSet(10, 60)
                ])
            ]));

        return plan.TrainingPlanId;
    }

    private static async Task<ExerciseSetReadModel> SingleExerciseSetAsync(ISender sender, Guid workoutId)
    {
        var workout = await sender.QueryAsync(new GetWorkoutDetailsQuery(workoutId));

        return workout.Workout!.ExerciseSets.Single();
    }

    private ISender Sender() => _provider.CreateScope().ServiceProvider.GetRequiredService<ISender>();
}
