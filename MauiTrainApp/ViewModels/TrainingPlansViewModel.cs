using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiTrainApp.Application.CQRS.Commands.StartWorkoutFromPlan;
using MauiTrainApp.Application.CQRS.Queries.GetNextTrainingPlan;
using MauiTrainApp.Application.CQRS.Queries.GetTrainingPlanDetails;
using MauiTrainApp.Application.CQRS.Queries.GetTrainingPlans;
using MauiTrainApp.ExceptionHandler.Interfaces;
using MauiTrainApp.Navigation.Interfaces;
using MauiTrainApp.Startup;
using MauiTrainApp.ViewModels.Base;
using MauiTrainApp.ViewModels.Items;
using Microsoft.Extensions.DependencyInjection;

namespace MauiTrainApp.ViewModels
{
    public sealed partial class TrainingPlansViewModel : ViewModelBase
    {
        private readonly AppStartup _startup;
        private readonly INavigator _navigator;

        [ObservableProperty]
        private bool _isEmpty;

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

        public ObservableCollection<TrainingPlanRowViewModel> TrainingPlans { get; } = [];

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

                var plans = await QueryAsync(new GetTrainingPlansQuery(), token);
                var next = await QueryAsync(new GetNextTrainingPlanQuery(DateOnly.FromDateTime(DateTime.Now)), token);

                TrainingPlans.Clear();

                foreach (var plan in plans.TrainingPlans)
                {
                    var details = await QueryAsync(new GetTrainingPlanDetailsQuery(plan.Id), token);

                    if (details.TrainingPlan is null)
                    {
                        continue;
                    }

                    TrainingPlans.Add(TrainingPlanRowViewModel.From(
                        details.TrainingPlan,
                        details.TrainingPlan.Id == next.TrainingPlan?.Id));
                }

                IsEmpty = TrainingPlans.Count == 0;
            }, cancellationToken);
        }

        [RelayCommand]
        private Task StartWorkoutAsync(TrainingPlanRowViewModel trainingPlan, CancellationToken cancellationToken)
        {
            return RunAsync(async token =>
            {
                var started = await SendAsync(
                    new StartWorkoutFromPlanCommand(trainingPlan.Id, DateOnly.FromDateTime(DateTime.Now)),
                    token);

                await _navigator.GoToWorkoutAsync(started.WorkoutId);
            }, cancellationToken);
        }

        [RelayCommand]
        private Task EditPlanAsync(TrainingPlanRowViewModel trainingPlan)
        {
            return _navigator.GoToPlanEditorAsync(trainingPlan.Id);
        }

        [RelayCommand]
        private Task CreatePlanAsync()
        {
            return _navigator.GoToPlanEditorAsync();
        }
    }
}
