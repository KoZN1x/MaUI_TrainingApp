using MauiTrainApp.Navigation.Interfaces;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace MauiTrainApp.Navigation
{
    internal sealed class ShellNavigator : INavigator
    {
        public Task GoToWorkoutAsync(Guid workoutId)
        {
            var parameters = new Dictionary<string, object>
            {
                [AppRoutes.WorkoutIdParameter] = workoutId
            };

            return GoToAsync(() => Shell.Current.GoToAsync(AppRoutes.Workout, parameters));
        }

        public Task GoBackAsync()
        {
            return GoToAsync(() => Shell.Current.GoToAsync(".."));
        }

        private static Task GoToAsync(Func<Task> navigation)
        {
            return MainThread.InvokeOnMainThreadAsync(navigation);
        }
    }
}
