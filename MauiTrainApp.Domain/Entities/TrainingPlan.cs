using MauiTrainApp.Domain.Entities.Base;
using MauiTrainApp.Domain.Exceptions;

namespace MauiTrainApp.Domain.Entities;

/// <summary>
/// Шаблон тренировки: заранее составленный набор упражнений, по которому потом проводится <see cref="Workout"/>.
/// </summary>
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

    /// <summary>
    /// Создаёт тренировку по плану: подходы копируются как невыполненные, тренировка помнит, из какого плана взята.
    /// </summary>
    public Workout StartWorkout(DateOnly workoutDay)
    {
        if (ExerciseSets.Count == 0)
        {
            throw new InvariantException($"Training plan '{Name}' has no exercise sets to start a workout");
        }

        var exerciseSets = ExerciseSets
            .Select(x => new ExerciseSet(x.Exercise, x.WorkingSets.Select(workingSet => workingSet.Reset())))
            .ToList();

        return new Workout(workoutDay, exerciseSets, Id);
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
