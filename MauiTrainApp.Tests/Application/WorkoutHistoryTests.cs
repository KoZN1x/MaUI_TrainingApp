using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MauiTrainApp.Application.CQRS.Commands.CreateExercise;
using MauiTrainApp.Application.CQRS.Commands.CreateTrainingPlan;
using MauiTrainApp.Application.CQRS.Commands.StartWorkoutFromPlan;
using MauiTrainApp.Application.CQRS.Queries.GetWorkouts;
using MauiTrainApp.Application.DI;
using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Domain.Exceptions;
using MauiTrainApp.Infrastructure.DI;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MauiTrainApp.Tests.Application;

public class WorkoutHistoryTests : IAsyncLifetime
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
    public async Task GetWorkouts_WithoutPeriodReturnsTheWholeHistory()
    {
        var sender = Sender();
        var planId = await CreatePlanAsync(sender);

        await sender.SendAsync(new StartWorkoutFromPlanCommand(planId, Day.AddYears(-3)));
        await sender.SendAsync(new StartWorkoutFromPlanCommand(planId, Day));

        var workouts = await sender.QueryAsync(new GetWorkoutsQuery());

        Assert.Equal(2, workouts.Workouts.Count);
    }

    [Fact]
    public async Task GetWorkouts_WithPeriodStillFiltersByDay()
    {
        var sender = Sender();
        var planId = await CreatePlanAsync(sender);

        await sender.SendAsync(new StartWorkoutFromPlanCommand(planId, Day.AddYears(-3)));
        await sender.SendAsync(new StartWorkoutFromPlanCommand(planId, Day));

        var workouts = await sender.QueryAsync(new GetWorkoutsQuery(Day.AddDays(-7), Day));

        Assert.Equal(Day, Assert.Single(workouts.Workouts).WorkoutDay);
    }

    [Fact]
    public async Task GetWorkouts_IsOrderedFromTheMostRecentDay()
    {
        var sender = Sender();
        var planId = await CreatePlanAsync(sender);

        await sender.SendAsync(new StartWorkoutFromPlanCommand(planId, Day.AddDays(-10)));
        await sender.SendAsync(new StartWorkoutFromPlanCommand(planId, Day));
        await sender.SendAsync(new StartWorkoutFromPlanCommand(planId, Day.AddDays(-5)));

        var workouts = await sender.QueryAsync(new GetWorkoutsQuery());

        Assert.Equal(
            [Day, Day.AddDays(-5), Day.AddDays(-10)],
            workouts.Workouts.Select(x => x.WorkoutDay).ToArray());
    }

    [Fact]
    public async Task GetWorkouts_RejectsAnInvertedPeriod()
    {
        var sender = Sender();

        await Assert.ThrowsAsync<ValidationException>(
            () => sender.QueryAsync(new GetWorkoutsQuery(Day, Day.AddDays(-1))));
    }

    private ISender Sender() => _provider.CreateScope().ServiceProvider.GetRequiredService<ISender>();

    private static async Task<Guid> CreatePlanAsync(ISender sender)
    {
        var exercise = await sender.SendAsync(new CreateExerciseCommand("Squat"));

        var plan = await sender.SendAsync(new CreateTrainingPlanCommand(
            "Leg day",
            [new PlannedExerciseSet(exercise.ExerciseId, [new PlannedWorkingSet(10, 60)])]));

        return plan.TrainingPlanId;
    }
}
