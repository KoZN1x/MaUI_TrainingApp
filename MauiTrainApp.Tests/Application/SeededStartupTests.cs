using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MauiTrainApp.Application.CQRS.Queries.GetActiveWorkout;
using MauiTrainApp.Application.CQRS.Queries.GetExercises;
using MauiTrainApp.Application.CQRS.Queries.GetNextTrainingPlan;
using MauiTrainApp.Application.CQRS.Queries.GetProgressSummary;
using MauiTrainApp.Application.CQRS.Queries.GetTrainingPlanDetails;
using MauiTrainApp.Application.CQRS.Queries.GetTrainingPlans;
using MauiTrainApp.Application.CQRS.Queries.GetWorkouts;
using MauiTrainApp.Application.DI;
using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Infrastructure.DI;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MauiTrainApp.Tests.Application;

public class SeededStartupTests : IAsyncLifetime
{
    private static readonly DateOnly Today = new(2026, 9, 12);

    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"mauitrain-{Guid.NewGuid():N}.db");
    private ServiceProvider _provider = null!;

    public async Task InitializeAsync()
    {
        _provider = new ServiceCollection()
            .AddInfrastructure(_databasePath)
            .AddApplication()
            .BuildServiceProvider();

        await _provider.MigrateDatabaseAsync();
        await _provider.SeedDatabaseAsync();
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
    public async Task TodayScreen_LoadsEverythingItNeeds()
    {
        var sender = Sender();

        var active = await sender.QueryAsync(new GetActiveWorkoutQuery(Today));
        var next = await sender.QueryAsync(new GetNextTrainingPlanQuery(Today));
        var history = await sender.QueryAsync(new GetWorkoutsQuery(Today.AddDays(-179), Today));
        var summary = await sender.QueryAsync(new GetProgressSummaryQuery(Today.AddDays(-29), Today));

        Assert.Null(active.Workout);
        Assert.NotNull(next.TrainingPlan);
        Assert.Empty(history.Workouts);
        Assert.Equal(0, summary.Summary.WorkoutCount);
    }

    [Fact]
    public async Task PlansScreen_LoadsDetailsForEveryPlan()
    {
        var sender = Sender();

        var plans = await sender.QueryAsync(new GetTrainingPlansQuery());

        Assert.NotEmpty(plans.TrainingPlans);

        foreach (var plan in plans.TrainingPlans)
        {
            var details = await sender.QueryAsync(new GetTrainingPlanDetailsQuery(plan.Id));

            Assert.NotNull(details.TrainingPlan);
            Assert.NotEmpty(details.TrainingPlan!.ExerciseSets);
        }
    }

    [Fact]
    public async Task ProgressScreen_LoadsBothPeriods()
    {
        var sender = Sender();

        var summary = await sender.QueryAsync(new GetProgressSummaryQuery(Today.AddDays(-29), Today));
        var chart = await sender.QueryAsync(new GetProgressSummaryQuery(Today.AddDays(-42), Today));

        Assert.Equal(0, summary.Summary.RecordCount);
        Assert.NotEmpty(chart.Summary.WeeklyVolume);
    }

    [Fact]
    public async Task ExercisesScreen_LoadsTheSeededCatalog()
    {
        var exercises = await Sender().QueryAsync(new GetExercisesQuery());

        Assert.NotEmpty(exercises.Exercises);
        Assert.All(exercises.Exercises, x => Assert.False(string.IsNullOrWhiteSpace(x.Name)));
    }

    private ISender Sender() => _provider.CreateScope().ServiceProvider.GetRequiredService<ISender>();
}
