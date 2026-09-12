using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Domain.Exceptions;
using MauiTrainApp.Tests.Common;

namespace MauiTrainApp.Tests.Domain;

public class TrainingPlanTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ThrowsOnEmptyName(string name)
    {
        Assert.Throws<InvariantException>(() => new TrainingPlan(name));
    }

    [Fact]
    public void StartWorkout_LinksWorkoutToPlan()
    {
        var plan = TestData.NewTrainingPlan();

        var workout = plan.StartWorkout(TestData.WorkoutDay);

        Assert.Equal(plan.Id, workout.TrainingPlanId);
        Assert.Equal(TestData.WorkoutDay, workout.WorkoutDay);
    }

    [Fact]
    public void StartWorkout_CopiesSetsAsNotCompleted()
    {
        var completedSet = TestData.NewExerciseSet();
        completedSet.Complete();
        var plan = TestData.NewTrainingPlan("Leg day", completedSet);

        var workout = plan.StartWorkout(TestData.WorkoutDay);
        var workoutSet = workout.ExerciseSets.Single();

        Assert.False(workoutSet.IsCompleted);
        Assert.NotEqual(completedSet.Id, workoutSet.Id);
        Assert.Equal(completedSet.Exercise, workoutSet.Exercise);
        Assert.Equal(
            completedSet.WorkingSets.Select(x => x.RepScheme),
            workoutSet.WorkingSets.Select(x => x.RepScheme));
    }

    [Fact]
    public void StartWorkout_DoesNotAffectPlanWhenWorkoutChanges()
    {
        var plan = TestData.NewTrainingPlan();
        var workout = plan.StartWorkout(TestData.WorkoutDay);

        workout.ExerciseSets.Single().Complete();
        workout.AddExerciseSet(TestData.NewExerciseSet());

        Assert.Single(plan.ExerciseSets);
        Assert.False(plan.ExerciseSets.Single().IsCompleted);
    }

    [Fact]
    public void StartWorkout_ThrowsOnEmptyPlan()
    {
        var plan = new TrainingPlan("Empty");

        Assert.Throws<InvariantException>(() => plan.StartWorkout(TestData.WorkoutDay));
    }

    [Fact]
    public void Workout_IsCompletedWhenEveryWorkingSetIsDone()
    {
        var workout = TestData.NewTrainingPlan().StartWorkout(TestData.WorkoutDay);
        var exerciseSet = workout.ExerciseSets.Single();

        exerciseSet.AddWorkingSet(TestData.NewWorkingSet(3, 120));
        Assert.False(workout.IsCompleted);

        exerciseSet.CompleteWorkingSet(0);
        Assert.False(workout.IsCompleted);

        exerciseSet.CompleteWorkingSet(1);
        Assert.True(workout.IsCompleted);
    }

    [Fact]
    public void CompleteWorkingSet_ThrowsWhenIndexIsOutOfRange()
    {
        var exerciseSet = TestData.NewExerciseSet();

        Assert.Throws<InvariantException>(() => exerciseSet.CompleteWorkingSet(5));
    }
}
