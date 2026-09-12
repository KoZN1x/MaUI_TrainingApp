using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MauiTrainApp.Core.ExceptionHandler
{
    public static class GlobalExceptionHandling
    {
        private const string LoggerName = "MauiTrainApp.GlobalExceptionHandler";

        extension(IServiceProvider serviceProvider)
        {
            public void UseGlobalExceptionHandling()
            {
                var logger = serviceProvider
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger(LoggerName);

                AppDomain.CurrentDomain.UnhandledException += (_, args) =>
                    logger.LogCritical(
                        args.ExceptionObject as Exception,
                        "Unhandled exception, terminating: {IsTerminating}",
                        args.IsTerminating);

                TaskScheduler.UnobservedTaskException += (_, args) =>
                {
                    logger.LogError(args.Exception, "Unobserved task exception");

                    args.SetObserved();
                };
            }
        }
    }
}
