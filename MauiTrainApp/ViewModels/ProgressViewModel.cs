using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiTrainApp.Application.CQRS.Queries.GetProgressSummary;
using MauiTrainApp.Controls;
using MauiTrainApp.Converters;
using MauiTrainApp.Domain.ReadModels;
using MauiTrainApp.ExceptionHandler.Interfaces;
using MauiTrainApp.Navigation.Interfaces;
using MauiTrainApp.Startup;
using MauiTrainApp.ViewModels.Base;
using MauiTrainApp.ViewModels.Items;
using Microsoft.Extensions.DependencyInjection;

namespace MauiTrainApp.ViewModels
{
    public sealed partial class ProgressViewModel : ViewModelBase
    {
        private const int SummaryDays = 30;
        private const int ChartWeeks = 7;

        private readonly AppStartup _startup;
        private readonly INavigator _navigator;

        [ObservableProperty]
        private string _periodText = string.Empty;

        [ObservableProperty]
        private string _volumeText = "0 кг";

        [ObservableProperty]
        private string _volumeDeltaText = string.Empty;

        [ObservableProperty]
        private string _workoutsPerWeekText = "0";

        [ObservableProperty]
        private string _workingSetCountText = "0";

        [ObservableProperty]
        private string _recordCountText = "0";

        [ObservableProperty]
        private IReadOnlyList<BarChartItem> _weeklyVolume = [];

        [ObservableProperty]
        private bool _hasData;

        public ProgressViewModel(
            IServiceScopeFactory scopeFactory,
            IExceptionPresenter exceptionPresenter,
            AppStartup startup,
            INavigator navigator)
            : base(scopeFactory, exceptionPresenter)
        {
            _startup = startup;
            _navigator = navigator;
        }

        public ObservableCollection<MuscleShareViewModel> MuscleShares { get; } = [];

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

                var today = DateOnly.FromDateTime(DateTime.Now);
                var periodStart = today.AddDays(-(SummaryDays - 1));
                var chartStart = StartOfWeek(today).AddDays(-7 * (ChartWeeks - 1));

                PeriodText = $"{WorkoutDayConverter.ToShortText(periodStart)} — {WorkoutDayConverter.ToShortText(today)}";

                var summary = (await QueryAsync(new GetProgressSummaryQuery(periodStart, today), token)).Summary;
                var chart = (await QueryAsync(new GetProgressSummaryQuery(chartStart, today), token)).Summary;

                ApplyTiles(summary);
                ApplyMuscleShares(summary);

                WeeklyVolume =
                [
                    .. chart.WeeklyVolume.Select((week, index) =>
                        ToBar(week, index == chart.WeeklyVolume.Count - 1))
                ];

                HasData = summary.WorkoutCount > 0;
            }, cancellationToken);
        }

        [RelayCommand]
        private Task OpenExercisesAsync()
        {
            return _navigator.GoToExercisesAsync();
        }

        private void ApplyTiles(ProgressSummaryReadModel summary)
        {
            VolumeText = VolumeConverter.ToText(summary.TotalVolume);
            VolumeDeltaText = summary.PreviousPeriodVolume <= 0
                ? string.Empty
                : PercentConverter.ToText(summary.VolumeChangeRatio);
            WorkoutsPerWeekText = summary.WorkoutsPerWeek.ToString("0.#");
            WorkingSetCountText = $"{summary.CompletedWorkingSetCount}";
            RecordCountText = $"{summary.RecordCount}";
        }

        private void ApplyMuscleShares(ProgressSummaryReadModel summary)
        {
            MuscleShares.Clear();

            foreach (var muscle in summary.MuscleVolume)
            {
                MuscleShares.Add(MuscleShareViewModel.From(muscle, summary.TotalVolume));
            }
        }

        private static BarChartItem ToBar(WeeklyVolumeReadModel week, bool isCurrent) =>
            new(
                WorkoutDayConverter.ToShortText(week.WeekStart),
                week.Volume,
                week.Volume > 0 ? VolumeConverter.ToText(week.Volume) : string.Empty,
                isCurrent);

        private static DateOnly StartOfWeek(DateOnly date) => date.AddDays(-(((int)date.DayOfWeek + 6) % 7));
    }
}
