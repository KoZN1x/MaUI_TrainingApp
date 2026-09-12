namespace MauiTrainApp.Navigation
{
    public static class AppRoutes
    {
        public const string Today = "today";

        public const string TrainingPlans = "plans";

        public const string History = "history";

        public const string Workout = "workout";

        public const string WorkoutDetails = "workoutDetail";

        public const string PlanEditor = "planEdit";

        public const string Exercises = "exercises";

        public const string ExerciseDetails = "exercise";

        public const string Progress = "stats";

        public const string WorkoutIdParameter = "workoutId";

        public const string TrainingPlanIdParameter = "trainingPlanId";

        public const string ExerciseIdParameter = "exerciseId";

        public static string Tab(string route) => $"//{route}";
    }
}
