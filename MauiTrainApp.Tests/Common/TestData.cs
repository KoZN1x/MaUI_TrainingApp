using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Domain.ValueObjects;

namespace MauiTrainApp.Tests.Common;

internal static class TestData
{
    public static readonly DateOnly WorkoutDay = new(2026, 9, 12);

    public static Exercise NewExercise(string name = "Squat") => new(name);

    public static WorkingSet NewWorkingSet(byte reps = 5, double weight = 100) =>
        new() { RepScheme = new RepScheme { Reps = reps, Weight = weight } };

    public static ExerciseSet NewExerciseSet(Exercise? exercise = null, params WorkingSet[] workingSets) =>
        new(exercise ?? NewExercise(), workingSets.Length == 0 ? [NewWorkingSet()] : workingSets);

    public static Workout NewWorkout(params ExerciseSet[] exerciseSets) =>
        new(WorkoutDay, exerciseSets);

    public static TrainingPlan NewTrainingPlan(string name = "Push day", params ExerciseSet[] exerciseSets) =>
        new(name, exerciseSets.Length == 0 ? [NewExerciseSet()] : exerciseSets);

    public static ExerciseSet CopyOf(ExerciseSet exerciseSet) =>
        new(exerciseSet.Exercise, exerciseSet.WorkingSets)
        {
            Id = exerciseSet.Id,
            CreatedAt = exerciseSet.CreatedAt
        };
}
