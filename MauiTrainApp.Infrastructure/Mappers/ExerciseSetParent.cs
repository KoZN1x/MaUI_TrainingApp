namespace MauiTrainApp.Infrastructure.Mappers
{
    internal readonly record struct ExerciseSetParent
    {
        private ExerciseSetParent(Guid? workoutId, Guid? trainingPlanId)
        {
            WorkoutId = workoutId;
            TrainingPlanId = trainingPlanId;
        }

        public Guid? WorkoutId { get; }

        public Guid? TrainingPlanId { get; }

        public static ExerciseSetParent ForWorkout(Guid workoutId) => new(workoutId, null);

        public static ExerciseSetParent ForTrainingPlan(Guid trainingPlanId) => new(null, trainingPlanId);
    }
}
