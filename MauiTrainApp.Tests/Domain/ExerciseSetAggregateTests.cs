using System;
using System.Linq;
using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Domain.Exceptions;
using MauiTrainApp.Tests.Common;
using Xunit;

namespace MauiTrainApp.Tests.Domain;

public class ExerciseSetAggregateTests
{
    [Fact]
    public void RemoveExerciseSet_RemovesSetPassedAsAnotherInstanceWithSameId()
    {
        var exerciseSet = TestData.NewExerciseSet();
        var workout = TestData.NewWorkout(exerciseSet);
        var sameSet = TestData.CopyOf(exerciseSet);

        workout.RemoveExerciseSet(sameSet);

        Assert.Empty(workout.ExerciseSets);
    }

    [Fact]
    public void RemoveExerciseSet_ThrowsWhenSetIsMissing()
    {
        var workout = TestData.NewWorkout();

        Assert.Throws<InvariantException>(() => workout.RemoveExerciseSet(Guid.NewGuid()));
    }

    [Fact]
    public void AddExerciseSet_ThrowsOnDuplicate()
    {
        var exerciseSet = TestData.NewExerciseSet();
        var workout = TestData.NewWorkout(exerciseSet);

        Assert.Throws<InvariantException>(() => workout.AddExerciseSet(TestData.CopyOf(exerciseSet)));
    }

    [Fact]
    public void ReplaceExerciseSets_ReplacesInsteadOfAppending()
    {
        var workout = TestData.NewWorkout(TestData.NewExerciseSet());
        var replacement = TestData.NewExerciseSet();

        workout.ReplaceExerciseSets([replacement]);

        Assert.Equal([replacement], workout.ExerciseSets);
    }

    [Fact]
    public void UpdateExerciseSet_ReplacesSetWithSameId()
    {
        var exerciseSet = TestData.NewExerciseSet();
        var workout = TestData.NewWorkout(exerciseSet);
        var updated = TestData.CopyOf(exerciseSet);
        updated.AddWorkingSet(TestData.NewWorkingSet(3, 120));

        workout.UpdateExerciseSet(updated);

        Assert.Single(workout.ExerciseSets);
        Assert.Equal(2, workout.ExerciseSets.First().WorkingSets.Count);
    }

    [Fact]
    public void UpdateExerciseSet_ThrowsWhenSetIsMissing()
    {
        var workout = TestData.NewWorkout();

        Assert.Throws<InvariantException>(() => workout.UpdateExerciseSet(TestData.NewExerciseSet()));
    }

    [Fact]
    public void Mutation_SetsUpdatedTime()
    {
        var workout = TestData.NewWorkout();

        Assert.Null(workout.UpdatedAt);

        workout.AddExerciseSet(TestData.NewExerciseSet());

        Assert.NotNull(workout.UpdatedAt);
    }

    [Fact]
    public void Constructor_ThrowsOnDuplicatedSets()
    {
        var exerciseSet = TestData.NewExerciseSet();

        Assert.Throws<InvariantException>(() => new TrainingPlan("Push day", [exerciseSet, TestData.CopyOf(exerciseSet)]));
    }
}
