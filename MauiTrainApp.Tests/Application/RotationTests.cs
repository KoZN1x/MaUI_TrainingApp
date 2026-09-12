using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MauiTrainApp.Application.CQRS.Commands.CompleteWorkingSet;
using MauiTrainApp.Application.CQRS.Commands.CompleteWorkout;
using MauiTrainApp.Application.CQRS.Commands.CreateExercise;
using MauiTrainApp.Application.CQRS.Commands.CreateTrainingPlan;
using MauiTrainApp.Application.CQRS.Commands.DeleteTrainingPlan;
using MauiTrainApp.Application.CQRS.Commands.StartWorkoutFromPlan;
using MauiTrainApp.Application.CQRS.Queries.GetNextTrainingPlan;
using MauiTrainApp.Application.CQRS.Queries.GetWorkoutDetails;
using MauiTrainApp.Application.DI;
using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Domain.Exceptions;
using MauiTrainApp.Infrastructure.DI;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MauiTrainApp.Tests.Application;

public class RotationTests : IAsyncLifetime
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
    public async Task NextTrainingPlan_IsNullWithoutAnyPlans()
    {
        var next = await Sender().QueryAsync(new GetNextTrainingPlanQuery());

        Assert.Null(next.TrainingPlan);
    }

    [Fact]
    public async Task NextTrainingPlan_IsTheFirstOneUntilSomethingIsTrained()
    {
        var sender = Sender();

        await CreatePlanAsync(sender, "A day");
        await CreatePlanAsync(sender, "B day");

        var next = await sender.QueryAsync(new GetNextTrainingPlanQuery());

        Assert.Equal("A day", next.TrainingPlan!.Name);
    }

    [Fact]
    public async Task NextTrainingPlan_FollowsThePlanOfTheLastWorkout()
    {
        var sender = Sender();

        var first = await CreatePlanAsync(sender, "A day");
        await CreatePlanAsync(sender, "B day");

        await sender.SendAsync(new StartWorkoutFromPlanCommand(first, Day));

        var next = await sender.QueryAsync(new GetNextTrainingPlanQuery());

        Assert.Equal("B day", next.TrainingPlan!.Name);
    }

    [Fact]
    public async Task NextTrainingPlan_WrapsAroundAfterTheLastPlan()
    {
        var sender = Sender();

        await CreatePlanAsync(sender, "A day");
        var last = await CreatePlanAsync(sender, "B day");

        await sender.SendAsync(new StartWorkoutFromPlanCommand(last, Day));

        var next = await sender.QueryAsync(new GetNextTrainingPlanQuery());

        Assert.Equal("A day", next.TrainingPlan!.Name);
    }

    [Fact]
    public async Task NextTrainingPlan_FallsBackToTheFirstWhenThePlanIsGone()
    {
        var sender = Sender();

        await CreatePlanAsync(sender, "A day");
        var removed = await CreatePlanAsync(sender, "B day");

        await sender.SendAsync(new StartWorkoutFromPlanCommand(removed, Day));
        await sender.SendAsync(new DeleteTrainingPlanCommand(removed));

        var next = await sender.QueryAsync(new GetNextTrainingPlanQuery());

        Assert.Equal("A day", next.TrainingPlan!.Name);
    }

    [Fact]
    public async Task CompleteWorkout_MeasuresTheDurationFromTheMomentItStarted()
    {
        var sender = Sender();

        var planId = await CreatePlanAsync(sender, "A day");

        var beforeStart = DateTimeOffset.UtcNow;
        var started = await sender.SendAsync(new StartWorkoutFromPlanCommand(planId, Day));

        var exerciseSet = (await sender.QueryAsync(new GetWorkoutDetailsQuery(started.WorkoutId)))
            .Workout!.ExerciseSets.Single();

        await sender.SendAsync(new CompleteWorkingSetCommand(started.WorkoutId, exerciseSet.Id, 0));

        var completed = await sender.SendAsync(
            new CompleteWorkoutCommand(started.WorkoutId, beforeStart.AddMinutes(47)));

        var workout = (await sender.QueryAsync(new GetWorkoutDetailsQuery(started.WorkoutId))).Workout;

        Assert.InRange(completed.Duration, TimeSpan.FromMinutes(46.9), TimeSpan.FromMinutes(47));
        Assert.Equal(TimeSpan.FromSeconds((int)completed.Duration.TotalSeconds), workout!.Duration);
    }

    [Fact]
    public async Task CompleteWorkout_CountsThePauseBetweenSessions()
    {
        var sender = Sender();

        var planId = await CreatePlanAsync(sender, "A day");
        var started = await sender.SendAsync(new StartWorkoutFromPlanCommand(planId, Day));

        var details = (await sender.QueryAsync(new GetWorkoutDetailsQuery(started.WorkoutId))).Workout!;

        await sender.SendAsync(new CompleteWorkingSetCommand(started.WorkoutId, details.ExerciseSets.Single().Id, 0));

        var completed = await sender.SendAsync(
            new CompleteWorkoutCommand(started.WorkoutId, details.StartedAt.AddHours(2)));

        Assert.Equal(TimeSpan.FromHours(2), completed.Duration);
    }

    [Fact]
    public async Task CompleteWorkout_IsRejectedBeforeItStarted()
    {
        var sender = Sender();

        var planId = await CreatePlanAsync(sender, "A day");
        var started = await sender.SendAsync(new StartWorkoutFromPlanCommand(planId, Day));

        var details = (await sender.QueryAsync(new GetWorkoutDetailsQuery(started.WorkoutId))).Workout!;

        await sender.SendAsync(new CompleteWorkingSetCommand(started.WorkoutId, details.ExerciseSets.Single().Id, 0));

        await Assert.ThrowsAsync<ValidationException>(
            () => sender.SendAsync(new CompleteWorkoutCommand(started.WorkoutId, details.StartedAt.AddSeconds(-1))));
    }

    [Fact]
    public async Task CompleteWorkout_IsRejectedWithoutASingleCompletedWorkingSet()
    {
        var sender = Sender();

        var planId = await CreatePlanAsync(sender, "A day");
        var started = await sender.SendAsync(new StartWorkoutFromPlanCommand(planId, Day));

        await Assert.ThrowsAsync<ValidationException>(
            () => sender.SendAsync(new CompleteWorkoutCommand(started.WorkoutId, DateTimeOffset.UtcNow)));
    }

    private ISender Sender() => _provider.CreateScope().ServiceProvider.GetRequiredService<ISender>();

    private static async Task<Guid> CreatePlanAsync(ISender sender, string name)
    {
        var exercise = await sender.SendAsync(new CreateExerciseCommand($"{name} lift"));

        var plan = await sender.SendAsync(new CreateTrainingPlanCommand(
            name,
            [new PlannedExerciseSet(exercise.ExerciseId, [new PlannedWorkingSet(10, 60)])]));

        return plan.TrainingPlanId;
    }
}
