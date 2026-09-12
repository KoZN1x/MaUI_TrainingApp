using MauiTrainApp.Domain.Entities.Base;
using MauiTrainApp.Domain.Exceptions;
using MauiTrainApp.Domain.ValueObjects;

namespace MauiTrainApp.Domain.Entities;

public sealed class TrainingPlan : ExerciseSetAggregate
{
    public TrainingPlan(string name, IEnumerable<ExerciseSet>? exerciseSets = null)
        : base(exerciseSets)
    {
        Name = EnsureName(name);
    }

    #region Properties

    public string Name { get; private set; }

    #endregion

    #region Methods

    public void SetName(string name)
    {
        Name = EnsureName(name);

        SetUpdatedTime();
    }

    public Workout StartWorkout(DateOnly workoutDay, Workout? previousWorkout = null)
    {
        if (ExerciseSets.Count == 0)
        {
            throw new InvariantException($"Training plan '{Name}' has no exercise sets to start a workout");
        }

        if (previousWorkout is not null && previousWorkout.TrainingPlanId != Id)
        {
            throw new InvariantException($"Previous workout belongs to another training plan, not to '{Name}'");
        }

        var exerciseSets = ExerciseSets
            .Select(x => new ExerciseSet(x.Exercise, StartingWorkingSets(x, previousWorkout)))
            .ToList();

        return new Workout(workoutDay, exerciseSets, Id);
    }

    private static IEnumerable<WorkingSet> StartingWorkingSets(ExerciseSet planned, Workout? previousWorkout)
    {
        var performed = previousWorkout?.ExerciseSets
            .FirstOrDefault(x => x.Exercise.Id == planned.Exercise.Id);

        var source = performed is null || performed.WorkingSets.Count == 0
            ? planned
            : performed;

        return source.WorkingSets.Select(x => x.Reset());
    }

    private static string EnsureName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvariantException("Training plan name couldn't be empty");
        }

        return name.Trim();
    }

    #endregion
}
