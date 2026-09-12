using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Core.CQRS;
using MauiTrainApp.Core.ExceptionHandler.Interfaces;
using MauiTrainApp.Core.ExceptionHandler;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MauiTrainApp.Core.DI
{
    public static class ServiceCollectionExtensions
    {
        extension(IServiceCollection services)
        {
            public IServiceCollection AddCqrs()
            {
                return services
                    .AddLogging()
                    .AddSingleton(new HandlerRegistry())
                    .AddScoped<Sender>()
                    .AddScoped<ISender>(provider => new ExceptionHandlingSender(
                        provider.GetRequiredService<Sender>(),
                        provider.GetServices<IExceptionHandler>(),
                        provider.GetRequiredService<ILogger<ExceptionHandlingSender>>()));
            }

            public IServiceCollection AddCommand<TCommand, TResult, THandler>()
                where TCommand : ICommand<TResult>
                where THandler : class, ICommandHandler<TCommand, TResult>
            {
                services.Registry().RegisterCommand<TCommand, TResult, THandler>();

                return services.AddScoped<THandler>();
            }

            public IServiceCollection AddQuery<TQuery, TResult, THandler>()
                where TQuery : IQuery<TResult>
                where THandler : class, IQueryHandler<TQuery, TResult>
            {
                services.Registry().RegisterQuery<TQuery, TResult, THandler>();

                return services.AddScoped<THandler>();
            }

            public IServiceCollection AddExceptionHandler<THandler>()
                where THandler : class, IExceptionHandler
            {
                return services.AddScoped<IExceptionHandler, THandler>();
            }

            private HandlerRegistry Registry()
            {
                var descriptor = services.FirstOrDefault(x => x.ServiceType == typeof(HandlerRegistry));

                return descriptor?.ImplementationInstance as HandlerRegistry
                    ?? throw new InvalidOperationException($"Call {nameof(AddCqrs)} before registering handlers");
            }
        }
    }
}
