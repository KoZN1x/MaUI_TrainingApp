using CommunityToolkit.Mvvm.ComponentModel;
using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Domain.Exceptions;
using MauiTrainApp.ExceptionHandler.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace MauiTrainApp.ViewModels.Base
{
    public abstract partial class ViewModelBase : ObservableObject
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IExceptionPresenter _exceptionPresenter;

        [ObservableProperty]
        private bool _isBusy;

        protected ViewModelBase(IServiceScopeFactory scopeFactory, IExceptionPresenter exceptionPresenter)
        {
            _scopeFactory = scopeFactory;
            _exceptionPresenter = exceptionPresenter;
        }

        public virtual Task AppearingAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public virtual Task DisappearingAsync()
        {
            return Task.CompletedTask;
        }

        protected async Task RunAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default)
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            try
            {
                await operation(cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
            catch (AppException exception)
            {
                await _exceptionPresenter.PresentAsync(exception);
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected async Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            return await scope.ServiceProvider
                .GetRequiredService<ISender>()
                .SendAsync(command, cancellationToken);
        }

        protected async Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            return await scope.ServiceProvider
                .GetRequiredService<ISender>()
                .QueryAsync(query, cancellationToken);
        }
    }
}
