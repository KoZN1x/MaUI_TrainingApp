using MauiTrainApp.Domain.Entities.Base;

namespace MauiTrainApp.Domain.Entities
{
    public sealed class Workout : BaseEntity
    {
        private readonly List<ExerciseSet> _exerciseSets = [];

        #region Properties

        public DateOnly WorkoutDay { get; private set; } = DateOnly.FromDateTime(DateTime.Now);

        public IReadOnlyCollection<ExerciseSet> ExerciseSets => _exerciseSets.AsReadOnly();

        #endregion

        #region Methods

        public void SetWorkoutDay(DateOnly workoutDay)
        {
            WorkoutDay = workoutDay;
        }

        public void SetExerciseSets(ICollection<ExerciseSet> exerciseSets)
        {
            _exerciseSets.AddRange(exerciseSets);
        }

        public void AddExerciseSet(ExerciseSet exerciseSet)
        {
            _exerciseSets.Add(exerciseSet);

            SetUpdatedTime();
        }

        public void RemoveExerciseSet(ExerciseSet exerciseSet)
        {
            _exerciseSets.Remove(exerciseSet);

            SetUpdatedTime();
        }

        public void UpdateExerciseSet(int index, ExerciseSet exerciseSet)
        {
            _exerciseSets[index] = exerciseSet;

            SetUpdatedTime();
        }

        #endregion
    }
}
