using MauiTrainApp.Domain.Entities.Base;

namespace MauiTrainApp.Domain.Entities
{
    public sealed class Workout : ExerciseSetAggregate
    {
        public Workout(
            DateOnly workoutDay,
            IEnumerable<ExerciseSet>? exerciseSets = null,
            Guid? trainingPlanId = null)
            : base(exerciseSets)
        {
            WorkoutDay = workoutDay;
            TrainingPlanId = trainingPlanId;
        }

        #region Properties

        public DateOnly WorkoutDay { get; private set; }

        /// <summary>
        /// План, по которому проводилась тренировка. Null, если тренировка велась без плана.
        /// </summary>
        public Guid? TrainingPlanId { get; private set; }

        public bool IsCompleted => ExerciseSets.Count > 0 && ExerciseSets.All(x => x.IsCompleted);

        #endregion

        #region Methods

        public void SetWorkoutDay(DateOnly workoutDay)
        {
            WorkoutDay = workoutDay;

            SetUpdatedTime();
        }

        public void SetTrainingPlan(TrainingPlan? trainingPlan)
        {
            TrainingPlanId = trainingPlan?.Id;

            SetUpdatedTime();
        }

        #endregion
    }
}
