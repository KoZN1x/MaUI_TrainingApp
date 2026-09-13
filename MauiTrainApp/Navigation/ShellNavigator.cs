using MauiTrainApp.Navigation.Interfaces;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace MauiTrainApp.Navigation
{
    internal sealed class ShellNavigator : INavigator
    {
        public Task GoToTodayAsync() => GoToAsync(AppRoutes.Tab(AppRoutes.Today));

        public Task GoToTrainingPlansAsync() => GoToAsync(AppRoutes.Tab(AppRoutes.TrainingPlans));

        public Task GoToHistoryAsync() => GoToAsync(AppRoutes.Tab(AppRoutes.History));

        public Task GoToWorkoutAsync(Guid workoutId) =>
            GoToAsync(AppRoutes.Workout, AppRoutes.WorkoutIdParameter, workoutId);

        public Task GoToWorkoutDetailsAsync(Guid workoutId) =>
            GoToAsync(AppRoutes.WorkoutDetails, AppRoutes.WorkoutIdParameter, workoutId);

        public Task GoToPlanEditorAsync(Guid? trainingPlanId = null) =>
            trainingPlanId is null
                ? GoToAsync(AppRoutes.PlanEditor)
                : GoToAsync(AppRoutes.PlanEditor, AppRoutes.TrainingPlanIdParameter, trainingPlanId.Value);

        public Task GoToExercisesAsync() => GoToAsync(AppRoutes.Exercises);

        public Task GoToExerciseDetailsAsync(Guid exerciseId) =>
            GoToAsync(AppRoutes.ExerciseDetails, AppRoutes.ExerciseIdParameter, exerciseId);

        public Task GoToProgressAsync() => GoToAsync(AppRoutes.Tab(AppRoutes.Progress));

        public Task GoBackAsync() => GoToAsync("..");

        private static Task GoToAsync(string route)
        {
            return MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync(route));
        }

        private static Task GoToAsync(string route, string parameterName, object parameterValue)
        {
            var parameters = new Dictionary<string, object>
            {
                [parameterName] = parameterValue
            };

            return MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync(route, parameters));
        }
    }
}
