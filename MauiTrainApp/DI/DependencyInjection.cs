using MauiTrainApp.ExceptionHandler;
using MauiTrainApp.ExceptionHandler.Interfaces;
using MauiTrainApp.Navigation;
using MauiTrainApp.Navigation.Interfaces;
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
                    .AddViewModels()
                    .AddPages();
            }

            private IServiceCollection AddViewModels()
            {
                return services
                    .AddTransient<TrainingPlansViewModel>()
                    .AddTransient<WorkoutViewModel>();
            }

            private IServiceCollection AddPages()
            {
                return services
                    .AddTransient<TrainingPlansPage>()
                    .AddTransient<WorkoutPage>();
            }
        }
    }
}
