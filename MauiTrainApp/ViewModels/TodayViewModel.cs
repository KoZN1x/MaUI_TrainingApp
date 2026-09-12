using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiTrainApp.Application.CQRS.Commands.StartWorkoutFromPlan;
using MauiTrainApp.Application.CQRS.Queries.GetActiveWorkout;
using MauiTrainApp.Application.CQRS.Queries.GetNextTrainingPlan;
using MauiTrainApp.Application.CQRS.Queries.GetProgressSummary;
using MauiTrainApp.Application.CQRS.Queries.GetWorkouts;
using MauiTrainApp.Converters;
using MauiTrainApp.Domain.ReadModels;
using MauiTrainApp.ExceptionHandler.Interfaces;
using MauiTrainApp.Formatting;
using MauiTrainApp.Navigation.Interfaces;
using MauiTrainApp.Startup;
using MauiTrainApp.ViewModels.Base;
using MauiTrainApp.ViewModels.Items;
using Microsoft.Extensions.DependencyInjection;

namespace MauiTrainApp.ViewModels
{
    public sealed partial class TodayViewModel : ViewModelBase
    {
        private const int SummaryDays = 30;
        private const int HistoryDays = 180;
        private const int RecentWorkoutCount = 3;

        private static readonly string[] WeekDayLabels = ["пн", "вт", "ср", "чт", "пт", "сб", "вс"];

        private readonly AppStartup _startup;
        private readonly INavigator _navigator;

        private Guid? _activeWorkoutId;
        private Guid? _nextTrainingPlanId;

        [ObservableProperty]
        private string _todayText = string.Empty;

        [ObservableProperty]
        private string _heroKicker = string.Empty;

        [ObservableProperty]
        private string _heroTitle = string.Empty;

        [ObservableProperty]
        private string _heroSubtitle = string.Empty;

        [ObservableProperty]
        private string _heroActionText = string.Empty;

        [ObservableProperty]
        private string _workoutCountText = "0";

        [ObservableProperty]
        private string _volumeText = "0 кг";

        [ObservableProperty]
        private string _volumeDeltaText = string.Empty;

        [ObservableProperty]
        private string _streakText = "0";

        [ObservableProperty]
        private bool _hasRecentWorkouts;

        public TodayViewModel(
            IServiceScopeFactory scopeFactory,
            IExceptionPresenter exceptionPresenter,
            AppStartup startup,
            INavigator navigator)
            : base(scopeFactory, exceptionPresenter)
        {
            _startup = startup;
            _navigator = navigator;
        }

        public ObservableCollection<WeekDayViewModel> Week { get; } = [];

        public ObservableCollection<WorkoutRowViewModel> RecentWorkouts { get; } = [];

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

                TodayText = $"{WorkoutDayConverter.ToText(today)}, {WorkoutDayConverter.ToWeekday(today)}";

                var active = await QueryAsync(new GetActiveWorkoutQuery(today), token);
                var next = await QueryAsync(new GetNextTrainingPlanQuery(today), token);
                var history = await QueryAsync(
                    new GetWorkoutsQuery(today.AddDays(-(HistoryDays - 1)), today), token);
                var summary = await QueryAsync(
                    new GetProgressSummaryQuery(today.AddDays(-(SummaryDays - 1)), today), token);

                ApplyHero(active.Workout, next.TrainingPlan, next.DaysUntil, today);
                ApplyWeek(today, history.Workouts);
                ApplySummary(summary.Summary, history.Workouts, today);
                ApplyRecentWorkouts(history.Workouts);
            }, cancellationToken);
        }

        [RelayCommand]
        private Task StartOrContinueAsync(CancellationToken cancellationToken)
        {
            return RunAsync(async token =>
            {
                if (_activeWorkoutId is { } activeWorkoutId)
                {
                    await _navigator.GoToWorkoutAsync(activeWorkoutId);

                    return;
                }

                if (_nextTrainingPlanId is not { } trainingPlanId)
                {
                    await _navigator.GoToPlanEditorAsync();

                    return;
                }

                var started = await SendAsync(
                    new StartWorkoutFromPlanCommand(trainingPlanId, DateOnly.FromDateTime(DateTime.Now)),
                    token);

                await _navigator.GoToWorkoutAsync(started.WorkoutId);
            }, cancellationToken);
        }

        [RelayCommand]
        private Task OpenWorkoutAsync(WorkoutRowViewModel workout)
        {
            return _navigator.GoToWorkoutDetailsAsync(workout.Id);
        }

        [RelayCommand]
        private Task OpenProgressAsync()
        {
            return _navigator.GoToProgressAsync();
        }

        [RelayCommand]
        private Task OpenHistoryAsync()
        {
            return _navigator.GoToHistoryAsync();
        }

        private void ApplyHero(
            WorkoutListItemReadModel? active,
            TrainingPlanListItemReadModel? next,
            int? daysUntil,
            DateOnly today)
        {
            _activeWorkoutId = active?.Id;
            _nextTrainingPlanId = next?.Id;

            if (active is not null)
            {
                HeroKicker = "Тренировка идёт";
                HeroTitle = active.TrainingPlanName ?? "Тренировка";
                HeroSubtitle = $"Подходов: {active.CompletedWorkingSetCount} из {active.TotalWorkingSetCount}";
                HeroActionText = "Продолжить тренировку";

                return;
            }

            if (next is not null)
            {
                HeroKicker = daysUntil switch
                {
                    0 => "План на сегодня",
                    1 => "Завтра по плану",
                    null => "Следующий по очереди",
                    _ => $"Ближайшая тренировка {WeekDays.ToAccusative(today.AddDays(daysUntil.Value).DayOfWeek)}"
                };

                HeroTitle = next.Name;
                HeroSubtitle = RussianPlural.Exercises(next.ExerciseSetCount);
                HeroActionText = "Начать тренировку";

                return;
            }

            HeroKicker = "Начало";
            HeroTitle = "Планов пока нет";
            HeroSubtitle = "Создайте план, чтобы начать тренироваться";
            HeroActionText = "Создать план";
        }

        private void ApplyWeek(DateOnly today, IReadOnlyCollection<WorkoutListItemReadModel> workouts)
        {
            var trainedDays = workouts.Select(x => x.WorkoutDay).ToHashSet();
            var monday = StartOfWeek(today);

            Week.Clear();

            for (var offset = 0; offset < WeekDayLabels.Length; offset++)
            {
                var day = monday.AddDays(offset);

                Week.Add(new WeekDayViewModel(
                    WeekDayLabels[offset],
                    $"{day.Day}",
                    trainedDays.Contains(day),
                    day == today));
            }
        }

        private void ApplySummary(
            ProgressSummaryReadModel summary,
            IReadOnlyCollection<WorkoutListItemReadModel> history,
            DateOnly today)
        {
            WorkoutCountText = $"{summary.WorkoutCount}";
            VolumeText = VolumeConverter.ToText(summary.TotalVolume);
            VolumeDeltaText = summary.PreviousPeriodVolume <= 0
                ? string.Empty
                : PercentConverter.ToText(summary.VolumeChangeRatio);
            StreakText = $"{CountWeekStreak(history, today)}";
        }

        private void ApplyRecentWorkouts(IReadOnlyCollection<WorkoutListItemReadModel> workouts)
        {
            RecentWorkouts.Clear();

            foreach (var workout in workouts.Take(RecentWorkoutCount))
            {
                RecentWorkouts.Add(WorkoutRowViewModel.From(workout));
            }

            HasRecentWorkouts = RecentWorkouts.Count > 0;
        }

        private static int CountWeekStreak(IReadOnlyCollection<WorkoutListItemReadModel> workouts, DateOnly today)
        {
            var trainedWeeks = workouts.Select(x => StartOfWeek(x.WorkoutDay)).ToHashSet();

            if (trainedWeeks.Count == 0)
            {
                return 0;
            }

            var week = StartOfWeek(today);

            if (!trainedWeeks.Contains(week))
            {
                week = week.AddDays(-7);
            }

            var streak = 0;

            while (trainedWeeks.Contains(week))
            {
                streak++;
                week = week.AddDays(-7);
            }

            return streak;
        }

        private static DateOnly StartOfWeek(DateOnly date) => date.AddDays(-(((int)date.DayOfWeek + 6) % 7));
    }
}
