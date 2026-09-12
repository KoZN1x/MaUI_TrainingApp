using MauiTrainApp.Navigation;
using MauiTrainApp.Views;
using Microsoft.Maui.Controls;

namespace MauiTrainApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(AppRoutes.Workout, typeof(WorkoutPage));
            Routing.RegisterRoute(AppRoutes.WorkoutDetails, typeof(WorkoutDetailPage));
            Routing.RegisterRoute(AppRoutes.PlanEditor, typeof(PlanEditorPage));
            Routing.RegisterRoute(AppRoutes.Exercises, typeof(ExercisesPage));
            Routing.RegisterRoute(AppRoutes.ExerciseDetails, typeof(ExerciseDetailPage));
            Routing.RegisterRoute(AppRoutes.Progress, typeof(ProgressPage));
        }
    }
}
