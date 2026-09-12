using MauiTrainApp.Application.CQRS.Commands.CompleteExerciseSet;
using MauiTrainApp.Application.CQRS.Commands.CompleteWorkingSet;
using MauiTrainApp.Application.CQRS.Commands.CreateExercise;
using MauiTrainApp.Application.CQRS.Commands.CreateTrainingPlan;
using MauiTrainApp.Application.CQRS.Commands.DeleteWorkout;
using MauiTrainApp.Application.CQRS.Commands.StartWorkoutFromPlan;
using MauiTrainApp.Application.CQRS.Queries.GetExercises;
using MauiTrainApp.Application.CQRS.Queries.GetTrainingPlans;
using MauiTrainApp.Application.CQRS.Queries.GetWorkoutDetails;
using MauiTrainApp.Application.CQRS.Queries.GetWorkouts;
using MauiTrainApp.Application.CQRS.Queries.GetWorkoutsByPlan;
using MauiTrainApp.Application.DI;
using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Core.CQRS;
using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Domain.Exceptions;
using MauiTrainApp.Domain.Interfaces;
using MauiTrainApp.Domain.ReadModels;
using MauiTrainApp.Infrastructure.DI;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace MauiTrainApp.Tests.Application;

public class CqrsPipelineTests : IAsyncLifetime
{
    private static readonly DateOnly WorkoutDay = new(2026, 9, 12);

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
    public async Task GymScenario_FromPlanToCompletedWorkout()
    {
        await using var scope = _provider.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var squat = await sender.SendAsync(new CreateExerciseCommand("Squat", "Back squat"));
        var bench = await sender.SendAsync(new CreateExerciseCommand("Bench press"));

        var plan = await sender.SendAsync(new CreateTrainingPlanCommand(
            "Push day",
            [
                new PlannedExerciseSet(squat.ExerciseId, [new PlannedWorkingSet(5, 100), new PlannedWorkingSet(5, 105)]),
                new PlannedExerciseSet(bench.ExerciseId, [new PlannedWorkingSet(8, 60)])
            ]));

        var plans = await sender.QueryAsync(new GetTrainingPlansQuery());
        Assert.Equal(2, Assert.Single(plans.TrainingPlans).ExerciseSetCount);

        var started = await sender.SendAsync(new StartWorkoutFromPlanCommand(plan.TrainingPlanId, WorkoutDay));

        var workout = (await sender.QueryAsync(new GetWorkoutDetailsQuery(started.WorkoutId))).Workout;
        Assert.Equal("Push day", workout!.TrainingPlanName);
        Assert.Equal(2, workout.ExerciseSets.Count);
        Assert.All(workout.ExerciseSets, x => Assert.False(x.IsCompleted));

        CompleteExerciseSetResult? completion = null;

        foreach (var exerciseSet in workout.ExerciseSets)
        {
            completion = await sender.SendAsync(
                new CompleteExerciseSetCommand(started.WorkoutId, exerciseSet.Id));
        }

        Assert.True(completion!.IsWorkoutCompleted);

        var listItem = Assert.Single(
            (await sender.QueryAsync(new GetWorkoutsByPlanQuery(plan.TrainingPlanId))).Workouts);

        Assert.True(listItem.IsCompleted);
        Assert.Equal(3, listItem.TotalWorkingSetCount);
        Assert.Equal(3, listItem.CompletedWorkingSetCount);
    }

    [Fact]
    public async Task CompleteWorkingSet_MarksSingleSet()
    {
        await using var scope = _provider.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var exercise = await sender.SendAsync(new CreateExerciseCommand("Squat"));
        var plan = await sender.SendAsync(new CreateTrainingPlanCommand(
            "Leg day",
            [new PlannedExerciseSet(exercise.ExerciseId, [new PlannedWorkingSet(5, 100), new PlannedWorkingSet(5, 105)])]));
        var started = await sender.SendAsync(new StartWorkoutFromPlanCommand(plan.TrainingPlanId, WorkoutDay));

        var exerciseSetId = (await sender.QueryAsync(new GetWorkoutDetailsQuery(started.WorkoutId)))
            .Workout!.ExerciseSets.Single().Id;

        var result = await sender.SendAsync(
            new CompleteWorkingSetCommand(started.WorkoutId, exerciseSetId, 1));

        var workingSets = (await sender.QueryAsync(new GetWorkoutDetailsQuery(started.WorkoutId)))
            .Workout!.ExerciseSets.Single().WorkingSets;

        Assert.False(result.IsWorkoutCompleted);
        Assert.Equal([false, true], workingSets.Select(x => x.IsCompleted));
    }

    [Fact]
    public async Task CreateExercise_RejectsDuplicateName()
    {
        await using var scope = _provider.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        await sender.SendAsync(new CreateExerciseCommand("Squat"));

        await Assert.ThrowsAsync<ValidationException>(
            () => sender.SendAsync(new CreateExerciseCommand(" Squat ")));
    }

    [Fact]
    public async Task CreateTrainingPlan_RejectsUnknownExercise()
    {
        await using var scope = _provider.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        await Assert.ThrowsAsync<NotFoundException>(
            () => sender.SendAsync(new CreateTrainingPlanCommand(
                "Broken",
                [new PlannedExerciseSet(Guid.NewGuid(), [new PlannedWorkingSet(5, 100)])])));
    }

    [Fact]
    public async Task DeleteWorkout_RemovesItFromQueries()
    {
        await using var scope = _provider.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var exercise = await sender.SendAsync(new CreateExerciseCommand("Squat"));
        var plan = await sender.SendAsync(new CreateTrainingPlanCommand(
            "Leg day",
            [new PlannedExerciseSet(exercise.ExerciseId, [new PlannedWorkingSet(5, 100)])]));
        var started = await sender.SendAsync(new StartWorkoutFromPlanCommand(plan.TrainingPlanId, WorkoutDay));

        var deleted = await sender.SendAsync(new DeleteWorkoutCommand(started.WorkoutId));

        Assert.Equal(started.WorkoutId, deleted.WorkoutId);
        Assert.Null((await sender.QueryAsync(new GetWorkoutDetailsQuery(started.WorkoutId))).Workout);
        Assert.Empty((await sender.QueryAsync(new GetWorkoutsQuery(WorkoutDay, WorkoutDay))).Workouts);
    }

    [Fact]
    public async Task Queries_ReturnReadModelsNotEntities()
    {
        await using var scope = _provider.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        await sender.SendAsync(new CreateExerciseCommand("Squat"));

        var result = await sender.QueryAsync(new GetExercisesQuery());

        Assert.IsType<ExerciseReadModel>(Assert.Single(result.Exercises));
    }

    [Fact]
    public void WriteSide_IsRegisteredPerEntity()
    {
        using var scope = _provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IWriteRepository<Exercise>>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IWriteRepository<Workout>>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IWriteRepository<TrainingPlan>>());
    }

    [Fact]
    public async Task UnregisteredMessage_FailsLoudly()
    {
        await using var scope = _provider.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var exception = await Assert.ThrowsAsync<UnexpectedException>(
            () => sender.QueryAsync(new UnregisteredQuery()));

        Assert.IsType<InvalidOperationException>(exception.InnerException);
    }

    private sealed record UnregisteredQuery : IQuery<string>;
}
