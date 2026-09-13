using MauiTrainApp.Application.DI;
using MauiTrainApp.Controls;
using MauiTrainApp.Core.ExceptionHandler;
using MauiTrainApp.DI;
using MauiTrainApp.Infrastructure.DI;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Storage;

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
                .AddClient();

#if ANDROID
            EntryHandler.Mapper.AppendToMapping(nameof(NumericEntry), (handler, view) =>
            {
                if (view is NumericEntry)
                {
                    handler.PlatformView.KeyListener = DecimalNumberKeyListener.Instance;
                }
            });
#endif

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            var app = builder.Build();

            app.Services.UseGlobalExceptionHandling();

            return app;
        }
    }
}
