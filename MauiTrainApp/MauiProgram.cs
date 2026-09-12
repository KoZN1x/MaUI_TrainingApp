using MauiTrainApp.Application.DI;
using MauiTrainApp.Core.ExceptionHandler;
using MauiTrainApp.ExceptionHandler;
using MauiTrainApp.ExceptionHandler.Interfaces;
using MauiTrainApp.Infrastructure.DI;
using Microsoft.Extensions.Logging;

namespace MauiTrainApp
{
    public static class MauiProgram
    {
        private const string DatabaseFileName = "mauitrain.db";

        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services
                .AddInfrastructure(Path.Combine(FileSystem.AppDataDirectory, DatabaseFileName))
                .AddApplication()
                .AddSingleton<IExceptionPresenter, DialogExceptionPresenter>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            var app = builder.Build();

            app.Services.UseGlobalExceptionHandling();

            Task.Run(() => app.Services.MigrateDatabaseAsync()).GetAwaiter().GetResult();

            return app;
        }
    }
}
