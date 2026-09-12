using System;
using System.Linq;
using System.Threading.Tasks;
using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Tests.Common;
using Xunit;

namespace MauiTrainApp.Tests.Infrastructure;

public class ReadRepositoryTests : IDisposable
{
    private readonly TestDatabase _database = new();

    public void Dispose() => _database.Dispose();

    [Fact]
    public async Task Exercises_AreFilteredBySearchTerm()
    {
        await _database.ExercisesWrite.AddAsync(TestData.NewExercise());
        await _database.ExercisesWrite.AddAsync(TestData.NewExercise("Bench press"));

        var all = await _database.ExerciseReader.GetAllAsync();
        var found = await _database.ExerciseReader.SearchByNameAsync("ench");

        Assert.Equal(2, all.Count);
        Assert.Equal("Bench press", Assert.Single(found).Name);
    }

    [Fact]
    public async Task ExistsWithName_IsCaseSensitiveMatchOnStoredName()
    {
        await _database.ExercisesWrite.AddAsync(TestData.NewExercise());

        Assert.True(await _database.ExerciseReader.ExistsWithNameAsync("Squat"));
        Assert.False(await _database.ExerciseReader.ExistsWithNameAsync("Deadlift"));
    }

    [Fact]
    public async Task TrainingPlanList_CountsExerciseSetsWithoutLoadingThem()
    {
        var squat = await _database.ExercisesWrite.AddAsync(TestData.NewExercise());
        await _database.TrainingPlansWrite.AddAsync(TestData.NewTrainingPlan(
            "Push day",
            TestData.NewExerciseSet(squat),
            TestData.NewExerciseSet(squat)));

        var plan = Assert.Single(await _database.TrainingPlanReader.GetAllAsync());

        Assert.Equal("Push day", plan.Name);
        Assert.Equal(2, plan.ExerciseSetCount);
    }

    [Fact]
    public async Task TrainingPlanDetails_IncludeExerciseNamesAndWorkingSets()
    {
        var squat = await _database.ExercisesWrite.AddAsync(TestData.NewExercise());
        var plan = await _database.TrainingPlansWrite.AddAsync(TestData.NewTrainingPlan(
            "Push day",
            TestData.NewExerciseSet(squat, TestData.NewWorkingSet(5, 100), TestData.NewWorkingSet(3, 120))));

        var details = await _database.TrainingPlanReader.GetByIdAsync(plan.Id);

        var exerciseSet = Assert.Single(details!.ExerciseSets);
        Assert.Equal("Squat", exerciseSet.ExerciseName);
        Assert.Equal(squat.Id, exerciseSet.ExerciseId);
        Assert.Equal([100d, 120d], exerciseSet.WorkingSets.Select(x => x.Weight));
        Assert.False(exerciseSet.IsCompleted);
    }

    [Fact]
    public async Task TrainingPlanDetails_ReturnNullWhenMissing()
    {
        Assert.Null(await _database.TrainingPlanReader.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task WorkoutList_IsFilteredByPeriodAndCarriesProgress()
    {
        var squat = await _database.ExercisesWrite.AddAsync(TestData.NewExercise());
        var inPeriod = await _database.WorkoutsWrite.AddAsync(new Workout(
            new DateOnly(2026, 9, 12),
            [TestData.NewExerciseSet(squat, TestData.NewWorkingSet(), TestData.NewWorkingSet(3, 120))]));
        await _database.WorkoutsWrite.AddAsync(new Workout(
            new DateOnly(2026, 10, 1),
            [TestData.NewExerciseSet(squat)]));

        inPeriod.ExerciseSets.Single().CompleteWorkingSet(0);
        await _database.WorkoutsWrite.UpdateAsync(inPeriod);

        var found = await _database.WorkoutReader.GetByPeriodAsync(
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 9, 30));

        var workout = Assert.Single(found);
        Assert.Equal(new DateOnly(2026, 9, 12), workout.WorkoutDay);
        Assert.Equal(1, workout.CompletedWorkingSetCount);
        Assert.Equal(2, workout.TotalWorkingSetCount);
        Assert.False(workout.IsCompleted);
    }

    [Fact]
    public async Task WorkoutList_CarriesTrainingPlanName()
    {
        var squat = await _database.ExercisesWrite.AddAsync(TestData.NewExercise());
        var plan = await _database.TrainingPlansWrite.AddAsync(
            TestData.NewTrainingPlan("Push day", TestData.NewExerciseSet(squat)));
        await _database.WorkoutsWrite.AddAsync(plan.StartWorkout(TestData.WorkoutDay));

        var byPlan = Assert.Single(await _database.WorkoutReader.GetByTrainingPlanAsync(plan.Id));

        Assert.Equal(plan.Id, byPlan.TrainingPlanId);
        Assert.Equal("Push day", byPlan.TrainingPlanName);
    }

    [Fact]
    public async Task WorkoutDetails_CarryPlanNameAndSets()
    {
        var squat = await _database.ExercisesWrite.AddAsync(TestData.NewExercise());
        var plan = await _database.TrainingPlansWrite.AddAsync(
            TestData.NewTrainingPlan("Push day", TestData.NewExerciseSet(squat)));
        var workout = await _database.WorkoutsWrite.AddAsync(plan.StartWorkout(TestData.WorkoutDay));

        var details = await _database.WorkoutReader.GetByIdAsync(workout.Id);

        Assert.Equal("Push day", details!.TrainingPlanName);
        Assert.Equal("Squat", Assert.Single(details.ExerciseSets).ExerciseName);
    }

    [Fact]
    public async Task WorkoutDetails_HaveNoPlanNameForStandaloneWorkout()
    {
        var squat = await _database.ExercisesWrite.AddAsync(TestData.NewExercise());
        var workout = await _database.WorkoutsWrite.AddAsync(TestData.NewWorkout(TestData.NewExerciseSet(squat)));

        var details = await _database.WorkoutReader.GetByIdAsync(workout.Id);

        Assert.Null(details!.TrainingPlanId);
        Assert.Null(details.TrainingPlanName);
    }

    [Fact]
    public async Task ActiveWorkout_IsTodayWorkoutWithUnfinishedWorkingSets()
    {
        var squat = await _database.ExercisesWrite.AddAsync(TestData.NewExercise());
        var workout = await _database.WorkoutsWrite.AddAsync(TestData.NewWorkout(TestData.NewExerciseSet(squat)));

        var active = await _database.WorkoutReader.GetActiveAsync(TestData.WorkoutDay);

        Assert.Equal(workout.Id, active!.Id);
    }

    [Fact]
    public async Task ActiveWorkout_IgnoresFullyCompletedWorkout()
    {
        var squat = await _database.ExercisesWrite.AddAsync(TestData.NewExercise());
        var workout = TestData.NewWorkout(TestData.NewExerciseSet(squat));

        workout.ExerciseSets.Single().Complete();

        await _database.WorkoutsWrite.AddAsync(workout);

        Assert.Null(await _database.WorkoutReader.GetActiveAsync(TestData.WorkoutDay));
    }

    [Fact]
    public async Task ActiveWorkout_IgnoresWorkoutWithRecordedDuration()
    {
        var squat = await _database.ExercisesWrite.AddAsync(TestData.NewExercise());
        var workout = TestData.NewWorkout(TestData.NewExerciseSet(squat));

        workout.SetDuration(TimeSpan.FromMinutes(45));

        await _database.WorkoutsWrite.AddAsync(workout);

        Assert.Null(await _database.WorkoutReader.GetActiveAsync(TestData.WorkoutDay));
    }

    [Fact]
    public async Task ActiveWorkout_IgnoresOtherDays()
    {
        var squat = await _database.ExercisesWrite.AddAsync(TestData.NewExercise());
        await _database.WorkoutsWrite.AddAsync(TestData.NewWorkout(TestData.NewExerciseSet(squat)));

        Assert.Null(await _database.WorkoutReader.GetActiveAsync(TestData.WorkoutDay.AddDays(1)));
    }
}
