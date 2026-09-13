using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiTrainApp.Application.CQRS.Queries.GetWorkouts;
using MauiTrainApp.Converters;
using MauiTrainApp.Domain.ReadModels;
using MauiTrainApp.ExceptionHandler.Interfaces;
using MauiTrainApp.Formatting;
using MauiTrainApp.Navigation.Interfaces;
using MauiTrainApp.Services.Interfaces;
using MauiTrainApp.Startup;
using MauiTrainApp.ViewModels.Base;
using MauiTrainApp.ViewModels.Items;
using Microsoft.Extensions.DependencyInjection;

namespace MauiTrainApp.ViewModels
{
    public sealed partial class HistoryViewModel : ViewModelBase
    {
        private readonly AppStartup _startup;
        private readonly INavigator _navigator;
        private readonly IBackupService _backups;

        [ObservableProperty]
        private string _workoutCountText = "0";

        [ObservableProperty]
        private string _volumeText = "0 кг";

        [ObservableProperty]
        private string _averageDurationText = "—";

        [ObservableProperty]
        private bool _isEmpty;

        public HistoryViewModel(
            IServiceScopeFactory scopeFactory,
            IExceptionPresenter exceptionPresenter,
            AppStartup startup,
            INavigator navigator,
            IBackupService backups)
            : base(scopeFactory, exceptionPresenter)
        {
            _startup = startup;
            _navigator = navigator;
            _backups = backups;
        }

        public ObservableCollection<WorkoutMonthViewModel> Months { get; } = [];

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

                var workouts = (await QueryAsync(new GetWorkoutsQuery(), token)).Workouts;

                ApplyTiles(workouts);
                ApplyMonths(workouts);

                IsEmpty = workouts.Count == 0;
            }, cancellationToken);
        }

        [RelayCommand]
        private Task SaveBackupAsync(CancellationToken cancellationToken)
        {
            return RunAsync(token => _backups.ShareAsync(token), cancellationToken);
        }

        [RelayCommand]
        private Task OpenWorkoutAsync(WorkoutRowViewModel workout)
        {
            return _navigator.GoToWorkoutDetailsAsync(workout.Id);
        }

        private void ApplyTiles(IReadOnlyCollection<WorkoutListItemReadModel> workouts)
        {
            var durations = workouts.Where(x => x.Duration is { } duration && duration > TimeSpan.Zero).ToList();

            WorkoutCountText = $"{workouts.Count}";
            VolumeText = VolumeConverter.ToText(workouts.Sum(x => x.TotalVolume));
            AverageDurationText = durations.Count == 0
                ? "—"
                : DurationConverter.ToText(TimeSpan.FromSeconds(durations.Average(x => x.Duration!.Value.TotalSeconds)));
        }

        private void ApplyMonths(IReadOnlyCollection<WorkoutListItemReadModel> workouts)
        {
            Months.Clear();

            var months = workouts
                .OrderByDescending(x => x.WorkoutDay)
                .GroupBy(x => new DateOnly(x.WorkoutDay.Year, x.WorkoutDay.Month, 1));

            foreach (var month in months)
            {
                var rows = month.Select(WorkoutRowViewModel.From).ToList();

                Months.Add(new WorkoutMonthViewModel(
                    WorkoutDayConverter.ToMonthText(month.Key),
                    $"{RussianPlural.Workouts(rows.Count)} · {VolumeConverter.ToText(month.Sum(x => x.TotalVolume))}",
                    rows));
            }
        }
    }
}
