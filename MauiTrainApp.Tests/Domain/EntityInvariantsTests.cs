using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Domain.Exceptions;
using MauiTrainApp.Domain.ValueObjects;
using MauiTrainApp.Tests.Common;

namespace MauiTrainApp.Tests.Domain;

public class EntityInvariantsTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Exercise_ThrowsOnEmptyName(string name)
    {
        Assert.Throws<InvariantException>(() => new Exercise(name));
    }

    [Fact]
    public void Exercise_TrimsName()
    {
        Assert.Equal("Squat", new Exercise("  Squat  ").Name);
    }

    [Fact]
    public void ExerciseSet_ThrowsWithoutExercise()
    {
        Assert.Throws<ArgumentNullException>(() => new ExerciseSet(null!));
    }

    [Fact]
    public void RepScheme_ThrowsOnZeroReps()
    {
        Assert.Throws<InvariantException>(() => new RepScheme { Reps = 0, Weight = 50 });
    }

    [Fact]
    public void RepScheme_ThrowsOnNegativeWeight()
    {
        Assert.Throws<InvariantException>(() => new RepScheme { Reps = 5, Weight = -1 });
    }

    [Fact]
    public void Entities_AreEqualById()
    {
        var exerciseSet = TestData.NewExerciseSet();

        Assert.Equal(exerciseSet, TestData.CopyOf(exerciseSet));
        Assert.NotEqual(exerciseSet, TestData.NewExerciseSet());
    }

    [Fact]
    public void Entities_OfDifferentTypesAreNotEqual()
    {
        var id = Guid.NewGuid();
        var workout = new Workout(new DateOnly(2026, 9, 12)) { Id = id };
        var plan = new TrainingPlan("Push day") { Id = id };

        Assert.NotEqual<object>(workout, plan);
    }

    [Fact]
    public void Entities_AreCreatedInUtc()
    {
        Assert.Equal(TimeSpan.Zero, TestData.NewWorkout().CreatedAt.Offset);
    }
}
