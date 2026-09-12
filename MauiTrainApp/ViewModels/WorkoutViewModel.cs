using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiTrainApp.Application.CQRS.Commands.AddPerformedWorkingSet;
using MauiTrainApp.Application.CQRS.Commands.CompleteExerciseSet;
using MauiTrainApp.Application.CQRS.Commands.CompleteWorkingSet;
using MauiTrainApp.Application.CQRS.Commands.CompleteWorkout;
using MauiTrainApp.Application.CQRS.Commands.DeleteWorkout;
using MauiTrainApp.Application.CQRS.Commands.ResetWorkingSet;
using MauiTrainApp.Application.CQRS.Commands.UpdatePerformedWorkingSet;
using MauiTrainApp.Application.CQRS.Queries.GetWorkoutDetails;
using MauiTrainApp.Converters;
using MauiTrainApp.ExceptionHandler.Interfaces;
using MauiTrainApp.Navigation;
using MauiTrainApp.Navigation.Interfaces;
using MauiTrainApp.Services.Interfaces;
using MauiTrainApp.ViewModels.Base;
using MauiTrainApp.ViewModels.Items;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace MauiTrainApp.ViewModels
{
    public sealed partial class WorkoutViewModel : ViewModelBase, IQueryAttributable
    {
        private const byte MinimumReps = 1;
        private const byte MaximumReps = 100;

        private readonly INavigator _navigator;
        private readonly IDialogService _dialogs;
        private readonly IAppSettings _settings;
        private readonly IWorkoutClock _clock;

        private Guid _workoutId;

        [ObservableProperty]
        private string _title = "Тренировка";

        [ObservableProperty]
        private string _dayText = string.Empty;

        [ObservableProperty]
        private string _elapsedText = "0:00";

        [ObservableProperty]
        private string _progressText = string.Empty;

        [ObservableProperty]
        private string _volumeText = "0 кг";

        [ObservableProperty]
        private string _completeText = "Завершить";

        [ObservableProperty]
        private double _progress;

        [ObservableProperty]
        private bool _isCompleted;

        [ObservableProperty]
        private bool _isLoaded;

        public WorkoutViewModel(
            IServiceScopeFactory scopeFactory,
            IExceptionPresenter exceptionPresenter,
            INavigator navigator,
            IDialogService dialogs,
            IAppSettings settings,
            IWorkoutClock clock)
            : base(scopeFactory, exceptionPresenter)
        {
            _navigator = navigator;
            _dialogs = dialogs;
            _settings = settings;
            _clock = clock;
            _clock.Ticked += OnClockTicked;
        }

        public ObservableCollection<WorkoutExerciseViewModel> Exercises { get; } = [];

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue(AppRoutes.WorkoutIdParameter, out var value) && value is Guid workoutId)
            {
                _workoutId = workoutId;
            }
        }

        public override Task AppearingAsync(CancellationToken cancellationToken = default)
        {
            return LoadCommand.ExecuteAsync(null);
        }

        public override Task DisappearingAsync()
        {
            _clock.Stop();

            return Task.CompletedTask;
        }

        [RelayCommand]
        private Task LoadAsync(CancellationToken cancellationToken)
        {
            return RunAsync(async token =>
            {
                var result = await QueryAsync(new GetWorkoutDetailsQuery(_workoutId), token);

                Exercises.Clear();

                if (result.Workout is null)
                {
                    IsLoaded = false;

                    return;
                }

                var number = 1;

                foreach (var exerciseSet in result.Workout.ExerciseSets)
                {
                    var exercise = new WorkoutExerciseViewModel(exerciseSet, number++);

                    exercise.IsExpanded = !exercise.IsCompleted && Exercises.All(x => !x.IsExpanded);

                    Exercises.Add(exercise);
                }

                Title = result.Workout.TrainingPlanName ?? "Тренировка";
                DayText = WorkoutDayConverter.ToText(result.Workout.WorkoutDay);
                IsLoaded = true;

                Refresh();

                var elapsed = result.Workout.Duration ?? Since(result.Workout.StartedAt);

                OnClockTicked(this, elapsed);

                if (result.Workout.Duration is null)
                {
                    await MainThread.InvokeOnMainThreadAsync(() => _clock.Start(elapsed));
                }
            }, cancellationToken);
        }

        [RelayCommand]
        private void ToggleExercise(WorkoutExerciseViewModel exercise)
        {
            exercise.IsExpanded = !exercise.IsExpanded;
        }

        [RelayCommand]
        private Task IncreaseWeightAsync(WorkingSetViewModel workingSet, CancellationToken cancellationToken) =>
            ChangeAsync(workingSet, weightDelta: _settings.WeightStep, repsDelta: 0, cancellationToken);

        [RelayCommand]
        private Task DecreaseWeightAsync(WorkingSetViewModel workingSet, CancellationToken cancellationToken) =>
            ChangeAsync(workingSet, weightDelta: -_settings.WeightStep, repsDelta: 0, cancellationToken);

        [RelayCommand]
        private Task IncreaseRepsAsync(WorkingSetViewModel workingSet, CancellationToken cancellationToken) =>
            ChangeAsync(workingSet, weightDelta: 0, repsDelta: 1, cancellationToken);

        [RelayCommand]
        private Task DecreaseRepsAsync(WorkingSetViewModel workingSet, CancellationToken cancellationToken) =>
            ChangeAsync(workingSet, weightDelta: 0, repsDelta: -1, cancellationToken);

        [RelayCommand]
        private Task ToggleWorkingSetAsync(WorkingSetViewModel workingSet, CancellationToken cancellationToken)
        {
            return RunAsync(async token =>
            {
                if (workingSet.IsCompleted)
                {
                    await SendAsync(
                        new ResetWorkingSetCommand(_workoutId, workingSet.ExerciseSetId, workingSet.Index),
                        token);

                    workingSet.IsCompleted = false;
                }
                else
                {
                    await SendAsync(
                        new UpdatePerformedWorkingSetCommand(
                            _workoutId,
                            workingSet.ExerciseSetId,
                            workingSet.Index,
                            workingSet.Reps,
                            workingSet.Weight),
                        token);

                    await SendAsync(
                        new CompleteWorkingSetCommand(_workoutId, workingSet.ExerciseSetId, workingSet.Index),
                        token);

                    workingSet.IsCompleted = true;
                }

                Refresh();
            }, cancellationToken);
        }

        [RelayCommand]
        private Task AddWorkingSetAsync(WorkoutExerciseViewModel exercise, CancellationToken cancellationToken)
        {
            return RunAsync(async token =>
            {
                await SendAsync(new AddPerformedWorkingSetCommand(_workoutId, exercise.Id), token);

                var last = exercise.WorkingSets.LastOrDefault();

                exercise.Add(new WorkingSetViewModel(
                    exercise.Id,
                    exercise.WorkingSets.Count,
                    new(last?.Reps ?? 10, last?.Weight ?? 0, false)));

                Refresh();
            }, cancellationToken);
        }

        [RelayCommand]
        private Task CompleteExerciseAsync(WorkoutExerciseViewModel exercise, CancellationToken cancellationToken)
        {
            return RunAsync(async token =>
            {
                await SendAsync(new CompleteExerciseSetCommand(_workoutId, exercise.Id), token);

                foreach (var workingSet in exercise.WorkingSets)
                {
                    workingSet.IsCompleted = true;
                }

                Refresh();
            }, cancellationToken);
        }

        [RelayCommand]
        private Task CompleteWorkoutAsync(CancellationToken cancellationToken)
        {
            return RunAsync(async token =>
            {
                _clock.Stop();

                await SendAsync(new CompleteWorkoutCommand(_workoutId, DateTimeOffset.UtcNow), token);

                await _navigator.GoToTodayAsync();
            }, cancellationToken);
        }

        [RelayCommand]
        private Task CancelWorkoutAsync(CancellationToken cancellationToken)
        {
            return RunAsync(async token =>
            {
                var confirmed = await _dialogs.ConfirmAsync(
                    "Отменить тренировку?",
                    "Тренировка и все отмеченные подходы будут удалены.",
                    "Удалить");

                if (!confirmed)
                {
                    return;
                }

                _clock.Stop();

                await SendAsync(new DeleteWorkoutCommand(_workoutId), token);

                await _navigator.GoToTodayAsync();
            }, cancellationToken);
        }

        private Task ChangeAsync(
            WorkingSetViewModel workingSet,
            double weightDelta,
            int repsDelta,
            CancellationToken cancellationToken)
        {
            return RunAsync(async token =>
            {
                var weight = Math.Max(0, workingSet.Weight + weightDelta);
                var reps = (byte)Math.Clamp(workingSet.Reps + repsDelta, MinimumReps, MaximumReps);

                if (Math.Abs(weight - workingSet.Weight) < double.Epsilon && reps == workingSet.Reps)
                {
                    return;
                }

                await SendAsync(
                    new UpdatePerformedWorkingSetCommand(
                        _workoutId,
                        workingSet.ExerciseSetId,
                        workingSet.Index,
                        reps,
                        weight),
                    token);

                workingSet.Weight = weight;
                workingSet.Reps = reps;

                Refresh();
            }, cancellationToken);
        }

        private void Refresh()
        {
            var workingSets = Exercises.SelectMany(x => x.WorkingSets).ToList();
            var completed = workingSets.Count(x => x.IsCompleted);

            Progress = workingSets.Count == 0 ? 0 : (double)completed / workingSets.Count;
            IsCompleted = workingSets.Count > 0 && completed == workingSets.Count;
            ProgressText = $"{completed} / {workingSets.Count} подходов";
            VolumeText = VolumeConverter.ToText(workingSets.Sum(x => x.Volume));
            CompleteText = $"Завершить ({completed}/{workingSets.Count})";
        }

        private static TimeSpan Since(DateTimeOffset startedAt)
        {
            var elapsed = DateTimeOffset.UtcNow - startedAt;

            return elapsed < TimeSpan.Zero ? TimeSpan.Zero : elapsed;
        }

        private void OnClockTicked(object? sender, TimeSpan elapsed)
        {
            ElapsedText = elapsed.TotalHours >= 1
                ? $"{(int)elapsed.TotalHours}:{elapsed.Minutes:00}:{elapsed.Seconds:00}"
                : $"{elapsed.Minutes}:{elapsed.Seconds:00}";
        }
    }
}
