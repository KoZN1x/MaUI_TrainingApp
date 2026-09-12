using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Domain.Interfaces;
using MauiTrainApp.Infrastructure.DI;
using MauiTrainApp.Tests.Common;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace MauiTrainApp.Tests.Infrastructure;

public class DependencyInjectionTests : IAsyncLifetime
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"mauitrain-{Guid.NewGuid():N}.db");
    private ServiceProvider _provider = null!;

    public Task InitializeAsync()
    {
        _provider = new ServiceCollection()
            .AddInfrastructure(_databasePath)
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
    public async Task Migration_CreatesUsableDatabase()
    {
        await using var scope = _provider.CreateAsyncScope();

        var exercises = scope.ServiceProvider.GetRequiredService<IExerciseRepository>();
        var plans = scope.ServiceProvider.GetRequiredService<ITrainingPlanRepository>();
        var workouts = scope.ServiceProvider.GetRequiredService<IWorkoutRepository>();

        var squat = await exercises.AddAsync(TestData.NewExercise());
        var plan = await plans.AddAsync(TestData.NewTrainingPlan("Push day", TestData.NewExerciseSet(squat)));
        var workout = await workouts.AddAsync(plan.StartWorkout(TestData.WorkoutDay));

        var stored = await workouts.GetByTrainingPlanAsync(plan.Id);

        Assert.Equal(workout, Assert.Single(stored));
        Assert.Equal("Squat", stored.Single().ExerciseSets.Single().Exercise.Name);
    }

    [Fact]
    public void GenericContract_ResolvesToTheSameRepository()
    {
        using var scope = _provider.CreateScope();

        var generic = scope.ServiceProvider.GetRequiredService<IRepository<Workout>>();
        var specific = scope.ServiceProvider.GetRequiredService<IWorkoutRepository>();

        Assert.Same(specific, generic);
    }

    [Fact]
    public void Repositories_AreScoped()
    {
        using var first = _provider.CreateScope();
        using var second = _provider.CreateScope();

        Assert.NotSame(
            first.ServiceProvider.GetRequiredService<IWorkoutRepository>(),
            second.ServiceProvider.GetRequiredService<IWorkoutRepository>());

        Assert.Same(
            first.ServiceProvider.GetRequiredService<IWorkoutRepository>(),
            first.ServiceProvider.GetRequiredService<IWorkoutRepository>());
    }
}
