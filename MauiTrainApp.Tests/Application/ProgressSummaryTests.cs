using System;
using System.Linq;
using System.Threading.Tasks;
using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Domain.Enums;
using MauiTrainApp.Domain.ReadModels;
using MauiTrainApp.Infrastructure.Database;
using MauiTrainApp.Infrastructure.Repositories.Read;
using MauiTrainApp.Tests.Common;
using Xunit;

namespace MauiTrainApp.Tests.Application;

public class ProgressSummaryTests : IDisposable
{
    private static readonly DateOnly Monday = new(2026, 8, 31);

    private readonly TestDatabase _database = new();
    private readonly ProgressReadRepository _progress;

    public ProgressSummaryTests()
    {
        _progress = new ProgressReadRepository(_database.Context);
    }

    public void Dispose() => _database.Dispose();

    [Fact]
    public async Task WeeklyVolume_IsSplitByMondayStartedWeeks()
    {
        var squat = await _database.ExercisesWrite.AddAsync(TestData.NewExercise());

        await AddWorkoutAsync(squat, Monday, weight: 100);
        await AddWorkoutAsync(squat, Monday.AddDays(6), weight: 100);
        await AddWorkoutAsync(squat, Monday.AddDays(7), weight: 50);

        var summary = await _progress.GetSummaryAsync(Monday, Monday.AddDays(13));

        Assert.Equal(2, summary.WeeklyVolume.Count);

        var first = summary.WeeklyVolume.First();
        var second = summary.WeeklyVolume.Last();

        Assert.Equal(Monday, first.WeekStart);
        Assert.Equal(2, first.WorkoutCount);
        Assert.Equal(4000, first.Volume);

        Assert.Equal(Monday.AddDays(7), second.WeekStart);
        Assert.Equal(1, second.WorkoutCount);
        Assert.Equal(1000, second.Volume);
    }

    [Fact]
    public async Task WeeklyVolume_KeepsEmptyWeeksInThePeriod()
    {
        var squat = await _database.ExercisesWrite.AddAsync(TestData.NewExercise());

        await AddWorkoutAsync(squat, Monday, weight: 100);

        var summary = await _progress.GetSummaryAsync(Monday, Monday.AddDays(20));

        Assert.Equal(3, summary.WeeklyVolume.Count);
        Assert.Equal([2000, 0, 0], summary.WeeklyVolume.Select(x => x.Volume).ToArray());
    }

    [Fact]
    public async Task MuscleVolume_IsGroupedByMuscleAndOrderedByVolume()
    {
        var squat = await _database.ExercisesWrite.AddAsync(new Exercise("Squat", null, MuscleGroup.Legs));
        var press = await _database.ExercisesWrite.AddAsync(new Exercise("Bench", null, MuscleGroup.Chest));
        var lunge = await _database.ExercisesWrite.AddAsync(new Exercise("Lunge", null, MuscleGroup.Legs));

        await AddWorkoutAsync(press, Monday, weight: 50);
        await AddWorkoutAsync(squat, Monday.AddDays(1), weight: 100);
        await AddWorkoutAsync(lunge, Monday.AddDays(2), weight: 60);

        var summary = await _progress.GetSummaryAsync(Monday, Monday.AddDays(6));

        Assert.Equal(2, summary.MuscleVolume.Count);

        var legs = summary.MuscleVolume.First();
        var chest = summary.MuscleVolume.Last();

        Assert.Equal(MuscleGroup.Legs, legs.MuscleGroup);
        Assert.Equal(3200, legs.Volume);
        Assert.Equal(4, legs.WorkingSetCount);

        Assert.Equal(MuscleGroup.Chest, chest.MuscleGroup);
        Assert.Equal(1000, chest.Volume);
    }

    [Fact]
    public async Task MuscleVolume_IgnoresWorkingSetsThatWereNotCompleted()
    {
        var squat = await _database.ExercisesWrite.AddAsync(new Exercise("Squat", null, MuscleGroup.Legs));

        var workout = TestData.NewWorkout(TestData.NewExerciseSet(
            squat,
            TestData.NewWorkingSet(10, 100).Complete(),
            TestData.NewWorkingSet(10, 100)));

        workout.SetWorkoutDay(Monday);

        await _database.WorkoutsWrite.AddAsync(workout);

        var summary = await _progress.GetSummaryAsync(Monday, Monday.AddDays(6));

        Assert.Equal(1000, summary.TotalVolume);
        Assert.Equal(1, Assert.Single(summary.MuscleVolume).WorkingSetCount);
    }

    [Fact]
    public async Task RecordCount_CountsOnlyImprovementsOverEarlierSessions()
    {
        var squat = await _database.ExercisesWrite.AddAsync(TestData.NewExercise());

        await AddWorkoutAsync(squat, Monday, weight: 100);
        await AddWorkoutAsync(squat, Monday.AddDays(2), weight: 105);
        await AddWorkoutAsync(squat, Monday.AddDays(4), weight: 105);
        await AddWorkoutAsync(squat, Monday.AddDays(6), weight: 110);

        var summary = await _progress.GetSummaryAsync(Monday, Monday.AddDays(6));

        Assert.Equal(2, summary.RecordCount);
    }

    [Fact]
    public async Task RecordCount_IgnoresRecordsSetBeforeThePeriod()
    {
        var squat = await _database.ExercisesWrite.AddAsync(TestData.NewExercise());

        await AddWorkoutAsync(squat, Monday, weight: 100);
        await AddWorkoutAsync(squat, Monday.AddDays(2), weight: 120);
        await AddWorkoutAsync(squat, Monday.AddDays(9), weight: 110);

        var summary = await _progress.GetSummaryAsync(Monday.AddDays(7), Monday.AddDays(13));

        Assert.Equal(0, summary.RecordCount);
    }

    [Fact]
    public async Task ExerciseProgress_EstimatesOneRepMaxFromTheBestSet()
    {
        var squat = await _database.ExercisesWrite.AddAsync(TestData.NewExercise());

        await AddWorkoutAsync(squat, Monday, weight: 100, reps: 5);
        await AddWorkoutAsync(squat, Monday.AddDays(2), weight: 90, reps: 12);

        var progress = await _progress.GetExerciseProgressAsync(squat.Id);

        Assert.Equal(100, progress!.BestWeight);
        Assert.Equal(5, progress.BestWeightReps);
        Assert.Equal(Math.Round(100 * (1 + 5 / 30d)), progress.EstimatedOneRepMax);
        Assert.Equal(2, progress.SessionCount);
    }

    [Fact]
    public async Task ExerciseProgress_HasNoOneRepMaxWithoutCompletedSets()
    {
        var squat = await _database.ExercisesWrite.AddAsync(TestData.NewExercise());
        var workout = TestData.NewWorkout(TestData.NewExerciseSet(squat, TestData.NewWorkingSet(10, 100)));

        workout.SetWorkoutDay(Monday);

        await _database.WorkoutsWrite.AddAsync(workout);

        var progress = await _progress.GetExerciseProgressAsync(squat.Id);

        Assert.Empty(progress!.Sessions);
        Assert.Equal(0, progress.EstimatedOneRepMax);
    }

    [Fact]
    public async Task ExerciseProgress_ListsTrainingPlansTheExerciseBelongsTo()
    {
        var squat = await _database.ExercisesWrite.AddAsync(TestData.NewExercise());

        await _database.TrainingPlansWrite.AddAsync(
            TestData.NewTrainingPlan("Leg day", TestData.NewExerciseSet(squat)));

        var progress = await _progress.GetExerciseProgressAsync(squat.Id);

        Assert.Equal("Leg day", Assert.Single(progress!.TrainingPlanNames));
    }

    private async Task AddWorkoutAsync(Exercise exercise, DateOnly day, double weight, byte reps = 10)
    {
        var workout = TestData.NewWorkout(TestData.NewExerciseSet(
            exercise,
            TestData.NewWorkingSet(reps, weight).Complete(),
            TestData.NewWorkingSet(reps, weight).Complete()));

        workout.SetWorkoutDay(day);

        await _database.WorkoutsWrite.AddAsync(workout);
    }
}
