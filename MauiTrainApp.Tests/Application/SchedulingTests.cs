using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MauiTrainApp.Application.CQRS.Commands.CompleteWorkingSet;
using MauiTrainApp.Application.CQRS.Commands.CompleteWorkout;
using MauiTrainApp.Application.CQRS.Commands.CreateExercise;
using MauiTrainApp.Application.CQRS.Commands.CreateTrainingPlan;
using MauiTrainApp.Application.CQRS.Commands.SetTrainingPlanSchedule;
using MauiTrainApp.Application.CQRS.Commands.StartWorkoutFromPlan;
using MauiTrainApp.Application.CQRS.Queries.GetNextTrainingPlan;
using MauiTrainApp.Application.CQRS.Queries.GetTrainingPlanDetails;
using MauiTrainApp.Application.CQRS.Queries.GetWorkoutDetails;
using MauiTrainApp.Application.DI;
using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Infrastructure.DI;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MauiTrainApp.Tests.Application;

public class SchedulingTests : IAsyncLifetime
{
    private static readonly DateOnly Monday = new(2026, 9, 14);

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
    public void Monday_IsReallyAMonday()
    {
        Assert.Equal(DayOfWeek.Monday, Monday.DayOfWeek);
    }

    [Fact]
    public async Task Schedule_IsStoredAndReadBack()
    {
        var sender = Sender();
        var planId = await CreatePlanAsync(sender, "A day");

        await sender.SendAsync(
            new SetTrainingPlanScheduleCommand(planId, [DayOfWeek.Wednesday, DayOfWeek.Monday]));

        var details = (await sender.QueryAsync(new GetTrainingPlanDetailsQuery(planId))).TrainingPlan;

        Assert.Equal([DayOfWeek.Monday, DayOfWeek.Wednesday], details!.Schedule.Days);
    }

    [Fact]
    public async Task Schedule_CanBeSetWhenThePlanIsCreated()
    {
        var sender = Sender();
        var planId = await CreatePlanAsync(sender, "A day", [DayOfWeek.Friday]);

        var details = (await sender.QueryAsync(new GetTrainingPlanDetailsQuery(planId))).TrainingPlan;

        Assert.Equal([DayOfWeek.Friday], details!.Schedule.Days);
    }

    [Fact]
    public async Task Schedule_IsClearedByAnEmptyDayList()
    {
        var sender = Sender();
        var planId = await CreatePlanAsync(sender, "A day", [DayOfWeek.Friday]);

        await sender.SendAsync(new SetTrainingPlanScheduleCommand(planId, []));

        var details = (await sender.QueryAsync(new GetTrainingPlanDetailsQuery(planId))).TrainingPlan;

        Assert.True(details!.Schedule.IsEmpty);
    }

    [Fact]
    public async Task NextTrainingPlan_PrefersThePlanScheduledForToday()
    {
        var sender = Sender();

        await CreatePlanAsync(sender, "A day");
        await CreatePlanAsync(sender, "B day", [DayOfWeek.Monday]);

        var next = await sender.QueryAsync(new GetNextTrainingPlanQuery(Monday));

        Assert.Equal("B day", next.TrainingPlan!.Name);
        Assert.Equal(0, next.DaysUntil);
    }

    [Fact]
    public async Task NextTrainingPlan_TakesTheNearestScheduledDay()
    {
        var sender = Sender();

        await CreatePlanAsync(sender, "A day", [DayOfWeek.Saturday]);
        await CreatePlanAsync(sender, "B day", [DayOfWeek.Wednesday]);

        var next = await sender.QueryAsync(new GetNextTrainingPlanQuery(Monday));

        Assert.Equal("B day", next.TrainingPlan!.Name);
        Assert.Equal(2, next.DaysUntil);
    }

    [Fact]
    public async Task NextTrainingPlan_MovesOnWhenTodaysPlanIsAlreadyDone()
    {
        var sender = Sender();

        var monday = await CreatePlanAsync(sender, "A day", [DayOfWeek.Monday]);
        await CreatePlanAsync(sender, "B day", [DayOfWeek.Thursday]);

        await TrainAsync(sender, monday, Monday);

        var next = await sender.QueryAsync(new GetNextTrainingPlanQuery(Monday));

        Assert.Equal("B day", next.TrainingPlan!.Name);
        Assert.Equal(3, next.DaysUntil);
    }

    [Fact]
    public async Task NextTrainingPlan_PointsToTheNextWeekWhenTheOnlyPlanIsDone()
    {
        var sender = Sender();

        var monday = await CreatePlanAsync(sender, "A day", [DayOfWeek.Monday]);

        await TrainAsync(sender, monday, Monday);

        var next = await sender.QueryAsync(new GetNextTrainingPlanQuery(Monday));

        Assert.Equal("A day", next.TrainingPlan!.Name);
        Assert.Equal(7, next.DaysUntil);
    }

    [Fact]
    public async Task NextTrainingPlan_KeepsRotatingWhileNoPlanIsScheduled()
    {
        var sender = Sender();

        var first = await CreatePlanAsync(sender, "A day");
        await CreatePlanAsync(sender, "B day");

        await sender.SendAsync(new StartWorkoutFromPlanCommand(first, Monday));

        var next = await sender.QueryAsync(new GetNextTrainingPlanQuery(Monday));

        Assert.Equal("B day", next.TrainingPlan!.Name);
        Assert.Null(next.DaysUntil);
    }

    [Fact]
    public async Task NextTrainingPlan_IgnoresPlansWithoutASchedule()
    {
        var sender = Sender();

        await CreatePlanAsync(sender, "A day");
        await CreatePlanAsync(sender, "B day", [DayOfWeek.Sunday]);

        var next = await sender.QueryAsync(new GetNextTrainingPlanQuery(Monday));

        Assert.Equal("B day", next.TrainingPlan!.Name);
        Assert.Equal(6, next.DaysUntil);
    }

    private ISender Sender() => _provider.CreateScope().ServiceProvider.GetRequiredService<ISender>();

    private static async Task<Guid> CreatePlanAsync(ISender sender, string name, DayOfWeek[]? days = null)
    {
        var exercise = await sender.SendAsync(new CreateExerciseCommand($"{name} lift"));

        var plan = await sender.SendAsync(new CreateTrainingPlanCommand(
            name,
            [new PlannedExerciseSet(exercise.ExerciseId, [new PlannedWorkingSet(10, 60)])],
            days));

        return plan.TrainingPlanId;
    }

    private static async Task TrainAsync(ISender sender, Guid planId, DateOnly day)
    {
        var started = await sender.SendAsync(new StartWorkoutFromPlanCommand(planId, day));

        var exerciseSet = (await sender.QueryAsync(new GetWorkoutDetailsQuery(started.WorkoutId)))
            .Workout!.ExerciseSets.Single();

        await sender.SendAsync(new CompleteWorkingSetCommand(started.WorkoutId, exerciseSet.Id, 0));
        await sender.SendAsync(new CompleteWorkoutCommand(started.WorkoutId, DateTimeOffset.UtcNow));
    }
}
