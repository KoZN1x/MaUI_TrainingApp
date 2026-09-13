using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiTrainApp.Application.CQRS.Commands.AddPerformedExerciseSet;
using MauiTrainApp.Application.CQRS.Commands.AddPerformedWorkingSet;
using MauiTrainApp.Application.CQRS.Commands.CompleteExerciseSet;
using MauiTrainApp.Application.CQRS.Commands.CompleteWorkingSet;
using MauiTrainApp.Application.CQRS.Commands.CompleteWorkout;
using MauiTrainApp.Application.CQRS.Commands.DeleteWorkout;
using MauiTrainApp.Application.CQRS.Commands.RemovePerformedWorkingSet;
using MauiTrainApp.Application.CQRS.Commands.ResetWorkingSet;
using MauiTrainApp.Application.CQRS.Commands.UpdatePerformedWorkingSet;
using MauiTrainApp.Application.CQRS.Queries.GetExercises;
using MauiTrainApp.Application.CQRS.Queries.GetWorkoutDetails;
using MauiTrainApp.Converters;
using MauiTrainApp.Formatting;
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
        private const double MaximumWeight = 1000;
        private const int AddedSets = 3;
        private const byte AddedReps = 10;
        private const double AddedWeight = 0;

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

        [ObservableProperty]
        private bool _isPicking;

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

        public event EventHandler<WorkoutExerciseViewModel>? ExerciseExpanded;

        public ObservableCollection<WorkoutExerciseViewModel> Exercises { get; } = [];

        public ObservableCollection<ExerciseRowViewModel> PickerExercises { get; } = [];

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
            return RunAsync(token => LoadCoreAsync(null, token), cancellationToken);
        }

        private async Task LoadCoreAsync(Guid? expandExerciseSetId, CancellationToken cancellationToken)
        {
            var result = await QueryAsync(new GetWorkoutDetailsQuery(_workoutId), cancellationToken);

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

                exercise.IsExpanded = expandExerciseSetId is { } expandId
                    ? exercise.Id == expandId
                    : !exercise.IsCompleted && Exercises.All(x => !x.IsExpanded);

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
        }

        [RelayCommand]
        private void ToggleExercise(WorkoutExerciseViewModel exercise)
        {
            var expand = !exercise.IsExpanded;

            foreach (var other in Exercises)
            {
                other.IsExpanded = expand && ReferenceEquals(other, exercise);
            }

            if (expand)
            {
                ExerciseExpanded?.Invoke(this, exercise);
            }
        }

        [RelayCommand]
        private Task RemoveWorkingSetAsync(WorkingSetViewModel workingSet, CancellationToken cancellationToken)
        {
            return RunAsync(async token =>
            {
                var exercise = Exercises.FirstOrDefault(x => x.Id == workingSet.ExerciseSetId);

                if (exercise is null)
                {
                    return;
                }

                await SendAsync(
                    new RemovePerformedWorkingSetCommand(_workoutId, workingSet.ExerciseSetId, workingSet.Index),
                    token);

                exercise.Remove(workingSet);

                Refresh();
            }, cancellationToken);
        }

        [RelayCommand]
        private Task StartPickingAsync(CancellationToken cancellationToken)
        {
            return RunAsync(async token =>
            {
                var used = Exercises.Select(x => x.ExerciseId).ToHashSet();

                var exercises = (await QueryAsync(new GetExercisesQuery(), token))
                    .Exercises
                    .Where(x => !used.Contains(x.Id))
                    .Select(ExerciseRowViewModel.From);

                PickerExercises.Clear();

                foreach (var exercise in exercises)
                {
                    PickerExercises.Add(exercise);
                }

                IsPicking = true;
            }, cancellationToken);
        }

        [RelayCommand]
        private void CancelPicking()
        {
            IsPicking = false;
        }

        [RelayCommand]
        private Task PickExerciseAsync(ExerciseRowViewModel exercise, CancellationToken cancellationToken)
        {
            return RunAsync(async token =>
            {
                IsPicking = false;

                var added = await SendAsync(
                    new AddPerformedExerciseSetCommand(
                        _workoutId,
                        exercise.Id,
                        AddedSets,
                        AddedReps,
                        AddedWeight),
                    token);

                await LoadCoreAsync(added.ExerciseSetId, token);

                var target = Exercises.FirstOrDefault(x => x.Id == added.ExerciseSetId);

                if (target is not null)
                {
                    ExerciseExpanded?.Invoke(this, target);
                }
            }, cancellationToken);
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
        private async Task EditWorkingSetAsync(WorkingSetViewModel workingSet, CancellationToken cancellationToken)
        {
            var (reps, weight) = Pending(workingSet);

            if (reps == workingSet.Reps && Math.Abs(weight - workingSet.Weight) < double.Epsilon)
            {
                workingSet.SyncInputs();

                return;
            }

            await RunAsync(async token =>
            {
                if (await PersistAsync(workingSet, reps, weight, token))
                {
                    Refresh();
                }
            }, cancellationToken);

            workingSet.SyncInputs();
        }

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
                    var (reps, weight) = Pending(workingSet);

                    await SendAsync(
                        new UpdatePerformedWorkingSetCommand(
                            _workoutId,
                            workingSet.ExerciseSetId,
                            workingSet.Index,
                            reps,
                            weight),
                        token);

                    await SendAsync(
                        new CompleteWorkingSetCommand(_workoutId, workingSet.ExerciseSetId, workingSet.Index),
                        token);

                    workingSet.Apply(reps, weight);
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
                foreach (var workingSet in Exercises.SelectMany(x => x.WorkingSets).ToList())
                {
                    var (reps, weight) = Pending(workingSet);

                    await PersistAsync(workingSet, reps, weight, token);
                }

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
                var (pendingReps, pendingWeight) = Pending(workingSet);

                var weight = Math.Round(Math.Clamp(pendingWeight + weightDelta, 0, MaximumWeight), 2);
                var reps = (byte)Math.Clamp(pendingReps + repsDelta, MinimumReps, MaximumReps);

                if (await PersistAsync(workingSet, reps, weight, token))
                {
                    Refresh();
                }
                else
                {
                    workingSet.SyncInputs();
                }
            }, cancellationToken);
        }

        private (byte Reps, double Weight) Pending(WorkingSetViewModel workingSet)
        {
            return (
                NumberInput.Reps(workingSet.RepsText, workingSet.Reps, MinimumReps, MaximumReps),
                NumberInput.Weight(workingSet.WeightText, workingSet.Weight, MaximumWeight));
        }

        private async Task<bool> PersistAsync(
            WorkingSetViewModel workingSet,
            byte reps,
            double weight,
            CancellationToken cancellationToken)
        {
            if (reps == workingSet.Reps && Math.Abs(weight - workingSet.Weight) < double.Epsilon)
            {
                return false;
            }

            await SendAsync(
                new UpdatePerformedWorkingSetCommand(
                    _workoutId,
                    workingSet.ExerciseSetId,
                    workingSet.Index,
                    reps,
                    weight),
                cancellationToken);

            workingSet.Apply(reps, weight);

            return true;
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
