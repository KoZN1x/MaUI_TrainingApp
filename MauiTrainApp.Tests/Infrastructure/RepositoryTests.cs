using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Domain.Exceptions;
using MauiTrainApp.Tests.Common;
using Microsoft.EntityFrameworkCore;

namespace MauiTrainApp.Tests.Infrastructure;

public class RepositoryTests : IDisposable
{
    private readonly TestDatabase _database = new();

    public void Dispose() => _database.Dispose();

    [Fact]
    public async Task AddAsync_PersistsAggregateWithChildren()
    {
        var workout = await AddWorkoutAsync();

        var stored = await _database.Workouts.GetByIdAsync(workout.Id);

        Assert.NotNull(stored);
        Assert.Equal(workout, stored);
        Assert.Equal("Squat", stored.ExerciseSets.Single().Exercise.Name);
        Assert.Equal(100, stored.ExerciseSets.Single().WorkingSets.Single().RepScheme.Weight);
    }

    [Fact]
    public async Task GetByIdAsync_DoesNotInventUpdatedTime()
    {
        var workout = await AddWorkoutAsync();

        var stored = await _database.Workouts.GetByIdAsync(workout.Id);

        Assert.Null(stored!.UpdatedAt);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNullWhenMissing()
    {
        Assert.Null(await _database.Workouts.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateAsync_AddsAndRemovesChildren()
    {
        var workout = await AddWorkoutAsync();
        var removedSetId = workout.ExerciseSets.Single().Id;
        var bench = await _database.Exercises.AddAsync(TestData.NewExercise("Bench press"));

        workout.ReplaceExerciseSets([TestData.NewExerciseSet(bench)]);
        await _database.Workouts.UpdateAsync(workout);

        var stored = await _database.Workouts.GetByIdAsync(workout.Id);

        Assert.Equal("Bench press", stored!.ExerciseSets.Single().Exercise.Name);
        Assert.False(await _database.Context.ExerciseSets.AsNoTracking().AnyAsync(x => x.Id == removedSetId));
        Assert.NotNull(stored.UpdatedAt);
    }

    [Fact]
    public async Task UpdateAsync_PersistsCompletedWorkingSets()
    {
        var workout = await AddWorkoutAsync();

        workout.ExerciseSets.Single().Complete();
        await _database.Workouts.UpdateAsync(workout);

        var stored = await _database.Workouts.GetByIdAsync(workout.Id);

        Assert.True(stored!.IsCompleted);
    }

    [Fact]
    public async Task DeleteAsync_RemovesAggregateWithChildren()
    {
        var workout = await AddWorkoutAsync();

        await _database.Workouts.DeleteAsync(workout);

        Assert.Empty(await _database.Workouts.GetAllAsync());
        Assert.False(await _database.Context.ExerciseSets.AsNoTracking().AnyAsync(x => x.WorkoutRecordId != null));
    }

    [Fact]
    public async Task DeleteAsync_ThrowsWhenMissing()
    {
        await Assert.ThrowsAsync<EntityNotFoundException<Workout>>(
            () => _database.Workouts.DeleteAsync(TestData.NewWorkout()));
    }

    [Fact]
    public async Task WorkoutStartedFromPlan_KeepsPlanLink()
    {
        var squat = await _database.Exercises.AddAsync(TestData.NewExercise());
        var plan = await _database.TrainingPlans.AddAsync(
            TestData.NewTrainingPlan("Push day", TestData.NewExerciseSet(squat)));

        var workout = plan.StartWorkout(TestData.WorkoutDay);
        await _database.Workouts.AddAsync(workout);

        var storedWorkout = await _database.Workouts.GetByIdAsync(workout.Id);
        var storedPlan = await _database.TrainingPlans.GetByIdAsync(plan.Id);

        Assert.Equal(plan.Id, storedWorkout!.TrainingPlanId);
        Assert.Equal("Push day", storedPlan!.Name);
        Assert.Single(storedPlan.ExerciseSets);
        Assert.NotEqual(storedPlan.ExerciseSets.Single().Id, storedWorkout.ExerciseSets.Single().Id);
    }

    [Fact]
    public async Task PlanChildren_AreNotAttachedToWorkouts()
    {
        var squat = await _database.Exercises.AddAsync(TestData.NewExercise());
        var plan = await _database.TrainingPlans.AddAsync(
            TestData.NewTrainingPlan("Push day", TestData.NewExerciseSet(squat)));

        var planSets = await _database.Context.ExerciseSets
            .AsNoTracking()
            .Where(x => x.TrainingPlanRecordId == plan.Id)
            .ToListAsync();

        Assert.All(planSets, x => Assert.Null(x.WorkoutRecordId));
    }

    [Fact]
    public async Task DeletingPlan_KeepsPerformedWorkout()
    {
        var squat = await _database.Exercises.AddAsync(TestData.NewExercise());
        var plan = await _database.TrainingPlans.AddAsync(
            TestData.NewTrainingPlan("Push day", TestData.NewExerciseSet(squat)));
        var workout = await _database.Workouts.AddAsync(plan.StartWorkout(TestData.WorkoutDay));

        await _database.TrainingPlans.DeleteAsync(plan);

        var stored = await _database.Workouts.GetByIdAsync(workout.Id);

        Assert.NotNull(stored);
        Assert.Null(stored.TrainingPlanId);
        Assert.Single(stored.ExerciseSets);
    }

    [Fact]
    public async Task SearchByNameAsync_ReturnsDomainEntities()
    {
        await _database.Exercises.AddAsync(TestData.NewExercise());
        await _database.Exercises.AddAsync(TestData.NewExercise("Bench press"));

        var found = await _database.Exercises.SearchByNameAsync("ench");

        Assert.Equal("Bench press", Assert.Single(found).Name);
    }

    [Fact]
    public async Task GetByPeriodAsync_FiltersByWorkoutDay()
    {
        var squat = await _database.Exercises.AddAsync(TestData.NewExercise());
        await _database.Workouts.AddAsync(
            new Workout(new DateOnly(2026, 9, 12), [TestData.NewExerciseSet(squat)]));
        await _database.Workouts.AddAsync(
            new Workout(new DateOnly(2026, 10, 1), [TestData.NewExerciseSet(squat)]));

        var found = await _database.Workouts.GetByPeriodAsync(
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 9, 30));

        Assert.Equal(new DateOnly(2026, 9, 12), Assert.Single(found).WorkoutDay);
    }

    private async Task<Workout> AddWorkoutAsync()
    {
        var squat = await _database.Exercises.AddAsync(TestData.NewExercise());

        return await _database.Workouts.AddAsync(TestData.NewWorkout(TestData.NewExerciseSet(squat)));
    }
}
