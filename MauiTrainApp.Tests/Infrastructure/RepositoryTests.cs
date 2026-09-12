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

        var stored = await _database.WorkoutsWrite.GetByIdAsync(workout.Id);

        Assert.NotNull(stored);
        Assert.Equal(workout, stored);
        Assert.Equal("Squat", stored.ExerciseSets.Single().Exercise.Name);
        Assert.Equal(100, stored.ExerciseSets.Single().WorkingSets.Single().RepScheme.Weight);
    }

    [Fact]
    public async Task GetByIdAsync_DoesNotInventUpdatedTime()
    {
        var workout = await AddWorkoutAsync();

        var stored = await _database.WorkoutsWrite.GetByIdAsync(workout.Id);

        Assert.Null(stored!.UpdatedAt);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNullWhenMissing()
    {
        Assert.Null(await _database.WorkoutsWrite.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateAsync_AddsAndRemovesChildren()
    {
        var workout = await AddWorkoutAsync();
        var removedSetId = workout.ExerciseSets.Single().Id;
        var bench = await _database.ExercisesWrite.AddAsync(TestData.NewExercise("Bench press"));

        workout.ReplaceExerciseSets([TestData.NewExerciseSet(bench)]);
        await _database.WorkoutsWrite.UpdateAsync(workout);

        var stored = await _database.WorkoutsWrite.GetByIdAsync(workout.Id);

        Assert.Equal("Bench press", stored!.ExerciseSets.Single().Exercise.Name);
        Assert.False(await _database.Context.ExerciseSets.AsNoTracking().AnyAsync(x => x.Id == removedSetId));
        Assert.NotNull(stored.UpdatedAt);
    }

    [Fact]
    public async Task UpdateAsync_PersistsCompletedWorkingSets()
    {
        var workout = await AddWorkoutAsync();

        workout.ExerciseSets.Single().Complete();
        await _database.WorkoutsWrite.UpdateAsync(workout);

        var stored = await _database.WorkoutsWrite.GetByIdAsync(workout.Id);

        Assert.True(stored!.IsCompleted);
    }

    [Fact]
    public async Task DeleteAsync_RemovesAggregateWithChildren()
    {
        var workout = await AddWorkoutAsync();

        await _database.WorkoutsWrite.DeleteAsync(workout);

        Assert.False(await _database.Context.Workouts.AnyAsync());
        Assert.False(await _database.Context.ExerciseSets.AsNoTracking().AnyAsync(x => x.WorkoutRecordId != null));
    }

    [Fact]
    public async Task DeleteAsync_ThrowsWhenMissing()
    {
        await Assert.ThrowsAsync<EntityNotFoundException<Workout>>(
            () => _database.WorkoutsWrite.DeleteAsync(TestData.NewWorkout()));
    }

    [Fact]
    public async Task WorkoutStartedFromPlan_KeepsPlanLink()
    {
        var squat = await _database.ExercisesWrite.AddAsync(TestData.NewExercise());
        var plan = await _database.TrainingPlansWrite.AddAsync(
            TestData.NewTrainingPlan("Push day", TestData.NewExerciseSet(squat)));

        var workout = plan.StartWorkout(TestData.WorkoutDay);
        await _database.WorkoutsWrite.AddAsync(workout);

        var storedWorkout = await _database.WorkoutsWrite.GetByIdAsync(workout.Id);
        var storedPlan = await _database.TrainingPlansWrite.GetByIdAsync(plan.Id);

        Assert.Equal(plan.Id, storedWorkout!.TrainingPlanId);
        Assert.Equal("Push day", storedPlan!.Name);
        Assert.Single(storedPlan.ExerciseSets);
        Assert.NotEqual(storedPlan.ExerciseSets.Single().Id, storedWorkout.ExerciseSets.Single().Id);
    }

    [Fact]
    public async Task PlanChildren_AreNotAttachedToWorkouts()
    {
        var squat = await _database.ExercisesWrite.AddAsync(TestData.NewExercise());
        var plan = await _database.TrainingPlansWrite.AddAsync(
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
        var squat = await _database.ExercisesWrite.AddAsync(TestData.NewExercise());
        var plan = await _database.TrainingPlansWrite.AddAsync(
            TestData.NewTrainingPlan("Push day", TestData.NewExerciseSet(squat)));
        var workout = await _database.WorkoutsWrite.AddAsync(plan.StartWorkout(TestData.WorkoutDay));

        await _database.TrainingPlansWrite.DeleteAsync(plan);

        var stored = await _database.WorkoutsWrite.GetByIdAsync(workout.Id);

        Assert.NotNull(stored);
        Assert.Null(stored.TrainingPlanId);
        Assert.Single(stored.ExerciseSets);
    }

    private async Task<Workout> AddWorkoutAsync()
    {
        var squat = await _database.ExercisesWrite.AddAsync(TestData.NewExercise());

        return await _database.WorkoutsWrite.AddAsync(TestData.NewWorkout(TestData.NewExerciseSet(squat)));
    }
}
