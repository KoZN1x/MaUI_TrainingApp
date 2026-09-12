using MauiTrainApp.Domain.Entities.Base;
using MauiTrainApp.Domain.Exceptions;

namespace MauiTrainApp.Domain.Entities
{
    public sealed class Workout : ExerciseSetAggregate
    {
        public Workout(
            DateOnly workoutDay,
            IEnumerable<ExerciseSet>? exerciseSets = null,
            Guid? trainingPlanId = null,
            TimeSpan? duration = null)
            : base(exerciseSets)
        {
            WorkoutDay = workoutDay;
            TrainingPlanId = trainingPlanId;
            Duration = EnsureDuration(duration);
        }

        #region Properties

        public DateOnly WorkoutDay { get; private set; }

        public Guid? TrainingPlanId { get; private set; }

        public TimeSpan? Duration { get; private set; }

        public bool IsCompleted => ExerciseSets.Count > 0 && ExerciseSets.All(x => x.IsCompleted);

        public double TotalVolume => ExerciseSets.Sum(x => x.CompletedVolume);

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

        public void SetDuration(TimeSpan? duration)
        {
            Duration = EnsureDuration(duration);

            SetUpdatedTime();
        }

        private static TimeSpan? EnsureDuration(TimeSpan? duration)
        {
            if (duration is not null && duration.Value < TimeSpan.Zero)
            {
                throw new InvariantException("Workout duration couldn't be negative");
            }

            return duration;
        }

        #endregion
    }
}
