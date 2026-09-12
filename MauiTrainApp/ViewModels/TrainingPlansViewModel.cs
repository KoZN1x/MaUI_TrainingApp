using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiTrainApp.Application.CQRS.Commands.StartWorkoutFromPlan;
using MauiTrainApp.Application.CQRS.Queries.GetTrainingPlans;
using MauiTrainApp.Domain.ReadModels;
using MauiTrainApp.ExceptionHandler.Interfaces;
using MauiTrainApp.Navigation.Interfaces;
using MauiTrainApp.Startup;
using MauiTrainApp.ViewModels.Base;
using Microsoft.Extensions.DependencyInjection;

namespace MauiTrainApp.ViewModels
{
    public sealed partial class TrainingPlansViewModel : ViewModelBase
    {
        private readonly AppStartup _startup;
        private readonly INavigator _navigator;

        public TrainingPlansViewModel(
            IServiceScopeFactory scopeFactory,
            IExceptionPresenter exceptionPresenter,
            AppStartup startup,
            INavigator navigator)
            : base(scopeFactory, exceptionPresenter)
        {
            _startup = startup;
            _navigator = navigator;
        }

        public ObservableCollection<TrainingPlanListItemReadModel> TrainingPlans { get; } = [];

        [ObservableProperty]
        private bool _isEmpty;

        public override Task AppearingAsync(CancellationToken cancellationToken = default)
        {
            return LoadCommand.ExecuteAsync(null);
        }

        [RelayCommand]
        private Task LoadAsync(CancellationToken cancellationToken)
        {
            return RunAsync(async token =>
            {
                await _startup.EnsureReadyAsync();

                var result = await QueryAsync(new GetTrainingPlansQuery(), token);

                TrainingPlans.Clear();

                foreach (var trainingPlan in result.TrainingPlans)
                {
                    TrainingPlans.Add(trainingPlan);
                }

                IsEmpty = TrainingPlans.Count == 0;
            }, cancellationToken);
        }

        [RelayCommand]
        private Task StartWorkoutAsync(TrainingPlanListItemReadModel trainingPlan, CancellationToken cancellationToken)
        {
            return RunAsync(async token =>
            {
                var workoutDay = DateOnly.FromDateTime(DateTime.Now);

                var result = await SendAsync(new StartWorkoutFromPlanCommand(trainingPlan.Id, workoutDay), token);

                await _navigator.GoToWorkoutAsync(result.WorkoutId);
            }, cancellationToken);
        }
    }
}
