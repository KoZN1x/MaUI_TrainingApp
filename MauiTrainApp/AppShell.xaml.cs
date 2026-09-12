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
        }
    }
}
