using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MauiTrainApp.Application.CQRS.Commands.AddPerformedExerciseSet;
using MauiTrainApp.Application.CQRS.Commands.CompleteWorkingSet;
using MauiTrainApp.Application.CQRS.Commands.CompleteWorkout;
using MauiTrainApp.Application.CQRS.Commands.CreateExercise;
using MauiTrainApp.Application.CQRS.Commands.CreateTrainingPlan;
using MauiTrainApp.Application.CQRS.Commands.RemovePerformedWorkingSet;
using MauiTrainApp.Application.CQRS.Commands.StartWorkoutFromPlan;
using MauiTrainApp.Application.CQRS.Commands.UpdatePerformedWorkingSet;
using MauiTrainApp.Application.CQRS.Queries.GetWorkoutDetails;
using MauiTrainApp.Application.DI;
using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Domain.Exceptions;
using MauiTrainApp.Domain.ReadModels;
using MauiTrainApp.Infrastructure.DI;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MauiTrainApp.Tests.Application;

public class ActiveWorkoutEditingTests : IAsyncLifetime
{
    private static readonly DateOnly Day = new(2026, 9, 12);

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
    public async Task RemoveWorkingSet_TakesOutTheOneAtTheGivenIndex()
    {
        var sender = Sender();
        var workoutId = await StartAsync(sender);
        var exerciseSet = await SingleExerciseSetAsync(sender, workoutId);

        await sender.SendAsync(new UpdatePerformedWorkingSetCommand(workoutId, exerciseSet.Id, 0, 10, 50));
        await sender.SendAsync(new UpdatePerformedWorkingSetCommand(workoutId, exerciseSet.Id, 1, 10, 60));
        await sender.SendAsync(new UpdatePerformedWorkingSetCommand(workoutId, exerciseSet.Id, 2, 10, 70));

        await sender.SendAsync(new RemovePerformedWorkingSetCommand(workoutId, exerciseSet.Id, 1));

        var remaining = await SingleExerciseSetAsync(sender, workoutId);

        Assert.Equal([50, 70], remaining.WorkingSets.Select(x => x.Weight));
    }

    [Fact]
    public async Task RemoveWorkingSet_KeepsIdenticalSetsApart()
    {
        var sender = Sender();
        var workoutId = await StartAsync(sender);
        var exerciseSet = await SingleExerciseSetAsync(sender, workoutId);

        await sender.SendAsync(new CompleteWorkingSetCommand(workoutId, exerciseSet.Id, 0));

        await sender.SendAsync(new RemovePerformedWorkingSetCommand(workoutId, exerciseSet.Id, 2));

        var remaining = await SingleExerciseSetAsync(sender, workoutId);

        Assert.Equal(2, remaining.WorkingSets.Count);
        Assert.True(remaining.WorkingSets.First().IsCompleted);
    }

    [Fact]
    public async Task RemoveWorkingSet_RejectsAnIndexOutOfRange()
    {
        var sender = Sender();
        var workoutId = await StartAsync(sender);
        var exerciseSet = await SingleExerciseSetAsync(sender, workoutId);

        await Assert.ThrowsAsync<ValidationException>(
            () => sender.SendAsync(new RemovePerformedWorkingSetCommand(workoutId, exerciseSet.Id, 3)));
    }

    [Fact]
    public async Task RemoveWorkingSet_EmptiesTheExerciseWhenTheLastOneGoes()
    {
        var sender = Sender();
        var workoutId = await StartAsync(sender);
        var exerciseSet = await SingleExerciseSetAsync(sender, workoutId);

        foreach (var index in Enumerable.Range(0, 3).Reverse())
        {
            await sender.SendAsync(new RemovePerformedWorkingSetCommand(workoutId, exerciseSet.Id, index));
        }

        var remaining = await SingleExerciseSetAsync(sender, workoutId);

        Assert.Empty(remaining.WorkingSets);
    }

    [Fact]
    public async Task AddExerciseSet_AppendsItToTheWorkout()
    {
        var sender = Sender();
        var workoutId = await StartAsync(sender);

        var extra = await sender.SendAsync(new CreateExerciseCommand("Разгибания"));

        await sender.SendAsync(new AddPerformedExerciseSetCommand(workoutId, extra.ExerciseId, 4, 12, 25));

        var workout = await WorkoutAsync(sender, workoutId);
        var added = workout.ExerciseSets.Single(x => x.ExerciseId == extra.ExerciseId);

        Assert.Equal(2, workout.ExerciseSets.Count);
        Assert.Equal(4, added.WorkingSets.Count);
        Assert.All(added.WorkingSets, x => Assert.Equal(25, x.Weight));
        Assert.All(added.WorkingSets, x => Assert.Equal(12, x.Reps));
        Assert.All(added.WorkingSets, x => Assert.False(x.IsCompleted));
    }

    [Fact]
    public async Task AddExerciseSet_RefusesAnExerciseAlreadyInTheWorkout()
    {
        var sender = Sender();
        var workoutId = await StartAsync(sender);
        var workout = await WorkoutAsync(sender, workoutId);
        var existing = workout.ExerciseSets.Single().ExerciseId;

        await Assert.ThrowsAsync<ValidationException>(
            () => sender.SendAsync(new AddPerformedExerciseSetCommand(workoutId, existing, 3, 10, 20)));
    }

    [Fact]
    public async Task AddExerciseSet_RefusesACompletedWorkout()
    {
        var sender = Sender();
        var workoutId = await StartAsync(sender);
        var exerciseSet = await SingleExerciseSetAsync(sender, workoutId);

        await sender.SendAsync(new CompleteWorkingSetCommand(workoutId, exerciseSet.Id, 0));
        await sender.SendAsync(new CompleteWorkoutCommand(workoutId, DateTimeOffset.UtcNow));

        var extra = await sender.SendAsync(new CreateExerciseCommand("Разгибания"));

        await Assert.ThrowsAsync<ValidationException>(
            () => sender.SendAsync(new AddPerformedExerciseSetCommand(workoutId, extra.ExerciseId, 3, 10, 20)));
    }

    [Fact]
    public async Task AddExerciseSet_RefusesASetCountOutOfRange()
    {
        var sender = Sender();
        var workoutId = await StartAsync(sender);

        var extra = await sender.SendAsync(new CreateExerciseCommand("Разгибания"));

        await Assert.ThrowsAsync<ValidationException>(
            () => sender.SendAsync(new AddPerformedExerciseSetCommand(workoutId, extra.ExerciseId, 0, 10, 20)));
    }

    [Fact]
    public async Task AddExerciseSet_AcceptsAFractionalWeight()
    {
        var sender = Sender();
        var workoutId = await StartAsync(sender);

        var extra = await sender.SendAsync(new CreateExerciseCommand("Разгибания"));

        await sender.SendAsync(new AddPerformedExerciseSetCommand(workoutId, extra.ExerciseId, 1, 10, 62.5));

        var workout = await WorkoutAsync(sender, workoutId);
        var added = workout.ExerciseSets.Single(x => x.ExerciseId == extra.ExerciseId);

        Assert.Equal(62.5, added.WorkingSets.Single().Weight);
    }

    private ISender Sender() => _provider.CreateScope().ServiceProvider.GetRequiredService<ISender>();

    private static async Task<Guid> StartAsync(ISender sender)
    {
        var exercise = await sender.SendAsync(new CreateExerciseCommand("Присед"));

        var plan = await sender.SendAsync(new CreateTrainingPlanCommand(
            "A day",
            [
                new PlannedExerciseSet(
                    exercise.ExerciseId,
                    [
                        new PlannedWorkingSet(10, 60),
                        new PlannedWorkingSet(10, 60),
                        new PlannedWorkingSet(10, 60)
                    ])
            ]));

        var started = await sender.SendAsync(new StartWorkoutFromPlanCommand(plan.TrainingPlanId, Day));

        return started.WorkoutId;
    }

    private static async Task<WorkoutDetailsReadModel> WorkoutAsync(ISender sender, Guid workoutId) =>
        (await sender.QueryAsync(new GetWorkoutDetailsQuery(workoutId))).Workout!;

    private static async Task<ExerciseSetReadModel> SingleExerciseSetAsync(ISender sender, Guid workoutId) =>
        (await WorkoutAsync(sender, workoutId)).ExerciseSets.First();
}
