using MauiTrainApp.ExceptionHandler;
using MauiTrainApp.ExceptionHandler.Interfaces;
using MauiTrainApp.Navigation;
using MauiTrainApp.Navigation.Interfaces;
using MauiTrainApp.Services;
using MauiTrainApp.Services.Interfaces;
using MauiTrainApp.Startup;
using MauiTrainApp.ViewModels;
using MauiTrainApp.Views;
using Microsoft.Extensions.DependencyInjection;

namespace MauiTrainApp.DI
{
    public static class DependencyInjection
    {
        extension(IServiceCollection services)
        {
            public IServiceCollection AddClient()
            {
                return services
                    .AddSingleton<IExceptionPresenter, DialogExceptionPresenter>()
                    .AddSingleton<INavigator, ShellNavigator>()
                    .AddSingleton<AppStartup>()
                    .AddSingleton<IDialogService, DialogService>()
                    .AddSingleton<IAppSettings, AppSettings>()
                    .AddTransient<IWorkoutClock, WorkoutClock>()
                    .AddViewModels()
                    .AddPages();
            }

            private IServiceCollection AddViewModels()
            {
                return services
                    .AddTransient<TodayViewModel>()
                    .AddTransient<TrainingPlansViewModel>()
                    .AddTransient<WorkoutViewModel>()
                    .AddTransient<ProgressViewModel>()
                    .AddTransient<ExerciseDetailViewModel>()
                    .AddTransient<HistoryViewModel>()
                    .AddTransient<WorkoutDetailViewModel>();
            }

            private IServiceCollection AddPages()
            {
                return services
                    .AddTransient<TodayPage>()
                    .AddTransient<TrainingPlansPage>()
                    .AddTransient<HistoryPage>()
                    .AddTransient<WorkoutPage>()
                    .AddTransient<WorkoutDetailPage>()
                    .AddTransient<ProgressPage>()
                    .AddTransient<PlanEditorPage>()
                    .AddTransient<ExercisesPage>()
                    .AddTransient<ExerciseDetailPage>();
            }
        }
    }
}
