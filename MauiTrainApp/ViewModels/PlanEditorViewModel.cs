using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiTrainApp.Application.CQRS.Commands.AddPlannedExerciseSet;
using MauiTrainApp.Application.CQRS.Commands.CreateTrainingPlan;
using MauiTrainApp.Application.CQRS.Commands.DeleteTrainingPlan;
using MauiTrainApp.Application.CQRS.Commands.MovePlannedExerciseSet;
using MauiTrainApp.Application.CQRS.Commands.RemovePlannedExerciseSet;
using MauiTrainApp.Application.CQRS.Commands.RenameTrainingPlan;
using MauiTrainApp.Application.CQRS.Commands.SetTrainingPlanSchedule;
using MauiTrainApp.Application.CQRS.Commands.UpdatePlannedExerciseSet;
using MauiTrainApp.Application.CQRS.Queries.GetExercises;
using MauiTrainApp.Application.CQRS.Queries.GetTrainingPlanDetails;
using MauiTrainApp.Domain.ValueObjects;
using MauiTrainApp.Formatting;
using MauiTrainApp.ExceptionHandler.Interfaces;
using MauiTrainApp.Navigation;
using MauiTrainApp.Navigation.Interfaces;
using MauiTrainApp.Services.Interfaces;
using MauiTrainApp.ViewModels.Base;
using MauiTrainApp.ViewModels.Items;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;

namespace MauiTrainApp.ViewModels
{
    public sealed partial class PlanEditorViewModel : ViewModelBase, IQueryAttributable
    {
        private const int DefaultSets = 3;
        private const byte DefaultReps = 10;
        private const double DefaultWeight = 20;
        private const byte MinimumReps = 1;
        private const byte MaximumReps = 100;
        private const int MinimumSets = 1;
        private const int MaximumSets = 20;
        private const double MaximumWeight = 1000;

        private readonly INavigator _navigator;
        private readonly IDialogService _dialogs;
        private readonly IAppSettings _settings;

        private Guid? _trainingPlanId;
        private string _savedName = string.Empty;
        private List<ExerciseRowViewModel> _allExercises = [];

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _summaryText = string.Empty;

        [ObservableProperty]
        private string _scheduleText = string.Empty;

        [ObservableProperty]
        private string _saveText = "Создать план";

        [ObservableProperty]
        private bool _isExisting;

        [ObservableProperty]
        private bool _isPicking;

        [ObservableProperty]
        private bool _isEmpty = true;

        public PlanEditorViewModel(
            IServiceScopeFactory scopeFactory,
            IExceptionPresenter exceptionPresenter,
            INavigator navigator,
            IDialogService dialogs,
            IAppSettings settings)
            : base(scopeFactory, exceptionPresenter)
        {
            _navigator = navigator;
            _dialogs = dialogs;
            _settings = settings;
        }

        public ObservableCollection<PlannedExerciseViewModel> Exercises { get; } = [];

        public ObservableCollection<ScheduleDayViewModel> Schedule { get; } = [];

        public ObservableCollection<ExerciseRowViewModel> PickerExercises { get; } = [];

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue(AppRoutes.TrainingPlanIdParameter, out var value) && value is Guid trainingPlanId)
            {
                _trainingPlanId = trainingPlanId;
            }
        }

        public override Task AppearingAsync(CancellationToken cancellationToken = default)
        {
            return LoadCommand.ExecuteAsync(null);
        }

        [RelayCommand]
        private Task LoadAsync(CancellationToken cancellationToken)
        {
            return RunAsync(async token =>
            {
                _allExercises = [.. (await QueryAsync(new GetExercisesQuery(), token))
                    .Exercises
                    .Select(ExerciseRowViewModel.From)];

                IsExisting = _trainingPlanId is not null;
                SaveText = IsExisting ? "Сохранить" : "Создать план";

                Exercises.Clear();

                var schedule = WeekSchedule.Empty;

                if (_trainingPlanId is { } trainingPlanId)
                {
                    var details = (await QueryAsync(new GetTrainingPlanDetailsQuery(trainingPlanId), token))
                        .TrainingPlan;

                    if (details is not null)
                    {
                        Name = details.Name;
                        _savedName = details.Name;
                        schedule = details.Schedule;

                        foreach (var exerciseSet in details.ExerciseSets)
                        {
                            Exercises.Add(PlannedExerciseViewModel.From(exerciseSet));
                        }
                    }
                }

                ApplySchedule(schedule);

                Refresh();
            }, cancellationToken);
        }

        [RelayCommand]
        private void StartPicking()
        {
            var picked = Exercises.Select(x => x.ExerciseId).ToHashSet();

            PickerExercises.Clear();

            foreach (var exercise in _allExercises.Where(x => !picked.Contains(x.Id)))
            {
                PickerExercises.Add(exercise);
            }

            IsPicking = true;
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

                if (_trainingPlanId is { } trainingPlanId)
                {
                    var added = await SendAsync(
                        new AddPlannedExerciseSetCommand(
                            trainingPlanId,
                            exercise.Id,
                            DefaultSets,
                            DefaultReps,
                            DefaultWeight),
                        token);

                    Exercises.Add(new PlannedExerciseViewModel(
                        exercise.Id,
                        exercise.Name,
                        added.ExerciseSetId,
                        DefaultSets,
                        DefaultReps,
                        DefaultWeight));
                }
                else
                {
                    Exercises.Add(new PlannedExerciseViewModel(
                        exercise.Id,
                        exercise.Name,
                        null,
                        DefaultSets,
                        DefaultReps,
                        DefaultWeight));
                }

                Refresh();
            }, cancellationToken);
        }

        [RelayCommand]
        private Task RemoveExerciseAsync(PlannedExerciseViewModel exercise, CancellationToken cancellationToken)
        {
            return RunAsync(async token =>
            {
                if (_trainingPlanId is { } trainingPlanId && exercise.ExerciseSetId is { } exerciseSetId)
                {
                    await SendAsync(new RemovePlannedExerciseSetCommand(trainingPlanId, exerciseSetId), token);
                }

                Exercises.Remove(exercise);

                Refresh();
            }, cancellationToken);
        }

        [RelayCommand]
        private Task IncreaseSetsAsync(PlannedExerciseViewModel exercise, CancellationToken cancellationToken) =>
            ChangeAsync(exercise, setsDelta: 1, repsDelta: 0, weightDelta: 0, cancellationToken);

        [RelayCommand]
        private Task DecreaseSetsAsync(PlannedExerciseViewModel exercise, CancellationToken cancellationToken) =>
            ChangeAsync(exercise, setsDelta: -1, repsDelta: 0, weightDelta: 0, cancellationToken);

        [RelayCommand]
        private Task IncreaseRepsAsync(PlannedExerciseViewModel exercise, CancellationToken cancellationToken) =>
            ChangeAsync(exercise, setsDelta: 0, repsDelta: 1, weightDelta: 0, cancellationToken);

        [RelayCommand]
        private Task DecreaseRepsAsync(PlannedExerciseViewModel exercise, CancellationToken cancellationToken) =>
            ChangeAsync(exercise, setsDelta: 0, repsDelta: -1, weightDelta: 0, cancellationToken);

        [RelayCommand]
        private Task IncreaseWeightAsync(PlannedExerciseViewModel exercise, CancellationToken cancellationToken) =>
            ChangeAsync(exercise, setsDelta: 0, repsDelta: 0, _settings.WeightStep, cancellationToken);

        [RelayCommand]
        private Task DecreaseWeightAsync(PlannedExerciseViewModel exercise, CancellationToken cancellationToken) =>
            ChangeAsync(exercise, setsDelta: 0, repsDelta: 0, -_settings.WeightStep, cancellationToken);

        [RelayCommand]
        private Task MoveExerciseUpAsync(PlannedExerciseViewModel exercise, CancellationToken cancellationToken) =>
            MoveAsync(exercise, -1, cancellationToken);

        [RelayCommand]
        private Task MoveExerciseDownAsync(PlannedExerciseViewModel exercise, CancellationToken cancellationToken) =>
            MoveAsync(exercise, 1, cancellationToken);

        [RelayCommand]
        private Task ToggleScheduleDayAsync(ScheduleDayViewModel day, CancellationToken cancellationToken)
        {
            return RunAsync(async token =>
            {
                IReadOnlyCollection<DayOfWeek> days =
                    [.. Schedule.Where(x => x.IsSelected ^ ReferenceEquals(x, day)).Select(x => x.Day)];

                if (_trainingPlanId is { } trainingPlanId)
                {
                    await SendAsync(new SetTrainingPlanScheduleCommand(trainingPlanId, days), token);
                }

                day.IsSelected = !day.IsSelected;

                RefreshScheduleText();
            }, cancellationToken);
        }

        [RelayCommand]
        private async Task EditPlannedExerciseAsync(
            PlannedExerciseViewModel exercise,
            CancellationToken cancellationToken)
        {
            var (sets, reps, weight) = Pending(exercise);

            if (sets == exercise.Sets
                && reps == exercise.Reps
                && Math.Abs(weight - exercise.Weight) < double.Epsilon)
            {
                exercise.SyncInputs();

                return;
            }

            await RunAsync(async token =>
            {
                if (await PersistAsync(exercise, sets, reps, weight, token))
                {
                    Refresh();
                }
            }, cancellationToken);

            exercise.SyncInputs();
        }

        [RelayCommand]
        private Task SaveAsync(CancellationToken cancellationToken)
        {
            return RunAsync(async token =>
            {
                foreach (var exercise in Exercises.ToList())
                {
                    var (sets, reps, weight) = Pending(exercise);

                    await PersistAsync(exercise, sets, reps, weight, token);
                }

                if (_trainingPlanId is { } trainingPlanId)
                {
                    if (Name.Trim() != _savedName)
                    {
                        await SendAsync(new RenameTrainingPlanCommand(trainingPlanId, Name.Trim()), token);

                        _savedName = Name.Trim();
                    }
                }
                else
                {
                    await SendAsync(
                        new CreateTrainingPlanCommand(
                            Name.Trim(),
                            [.. Exercises.Select(ToPlanned)],
                            SelectedDays()),
                        token);
                }

                await _navigator.GoToTrainingPlansAsync();
            }, cancellationToken);
        }

        [RelayCommand]
        private Task DeleteAsync(CancellationToken cancellationToken)
        {
            return RunAsync(async token =>
            {
                if (_trainingPlanId is not { } trainingPlanId)
                {
                    return;
                }

                var confirmed = await _dialogs.ConfirmAsync(
                    "Удалить план?",
                    "Проведённые по нему тренировки останутся в истории.",
                    "Удалить");

                if (!confirmed)
                {
                    return;
                }

                await SendAsync(new DeleteTrainingPlanCommand(trainingPlanId), token);

                await _navigator.GoToTrainingPlansAsync();
            }, cancellationToken);
        }

        private Task ChangeAsync(
            PlannedExerciseViewModel exercise,
            int setsDelta,
            int repsDelta,
            double weightDelta,
            CancellationToken cancellationToken)
        {
            return RunAsync(async token =>
            {
                var (pendingSets, pendingReps, pendingWeight) = Pending(exercise);

                var sets = Math.Clamp(pendingSets + setsDelta, MinimumSets, MaximumSets);
                var reps = (byte)Math.Clamp(pendingReps + repsDelta, MinimumReps, MaximumReps);
                var weight = Math.Round(Math.Clamp(pendingWeight + weightDelta, 0, MaximumWeight), 2);

                if (await PersistAsync(exercise, sets, reps, weight, token))
                {
                    Refresh();
                }
                else
                {
                    exercise.SyncInputs();
                }
            }, cancellationToken);
        }

        private (int Sets, byte Reps, double Weight) Pending(PlannedExerciseViewModel exercise)
        {
            return (
                NumberInput.Sets(exercise.SetsText, exercise.Sets, MinimumSets, MaximumSets),
                NumberInput.Reps(exercise.RepsText, exercise.Reps, MinimumReps, MaximumReps),
                NumberInput.Weight(exercise.WeightText, exercise.Weight, MaximumWeight));
        }

        private async Task<bool> PersistAsync(
            PlannedExerciseViewModel exercise,
            int sets,
            byte reps,
            double weight,
            CancellationToken cancellationToken)
        {
            if (sets == exercise.Sets
                && reps == exercise.Reps
                && Math.Abs(weight - exercise.Weight) < double.Epsilon)
            {
                return false;
            }

            if (_trainingPlanId is { } trainingPlanId && exercise.ExerciseSetId is { } exerciseSetId)
            {
                await SendAsync(
                    new UpdatePlannedExerciseSetCommand(trainingPlanId, exerciseSetId, sets, reps, weight),
                    cancellationToken);
            }

            exercise.Apply(sets, reps, weight);

            return true;
        }

        private Task MoveAsync(
            PlannedExerciseViewModel exercise,
            int offset,
            CancellationToken cancellationToken)
        {
            return RunAsync(async token =>
            {
                var index = Exercises.IndexOf(exercise);
                var newIndex = index + offset;

                if (index < 0 || newIndex < 0 || newIndex >= Exercises.Count)
                {
                    return;
                }

                if (_trainingPlanId is { } trainingPlanId && exercise.ExerciseSetId is { } exerciseSetId)
                {
                    await SendAsync(
                        new MovePlannedExerciseSetCommand(trainingPlanId, exerciseSetId, newIndex),
                        token);
                }

                Exercises.Move(index, newIndex);

                RefreshOrder();
            }, cancellationToken);
        }

        private void RefreshOrder()
        {
            for (var index = 0; index < Exercises.Count; index++)
            {
                Exercises[index].SetOrder(index, Exercises.Count);
            }
        }

        private void ApplySchedule(WeekSchedule schedule)
        {
            Schedule.Clear();

            foreach (var day in WeekDays.Ordered)
            {
                Schedule.Add(new ScheduleDayViewModel(day, schedule.Contains(day)));
            }

            RefreshScheduleText();
        }

        private void RefreshScheduleText()
        {
            ScheduleText = Schedule.Any(x => x.IsSelected)
                ? WeekDays.Describe(WeekSchedule.From(SelectedDays()))
                : "Без расписания — план предлагается по очереди";
        }

        private IReadOnlyCollection<DayOfWeek> SelectedDays() =>
            [.. Schedule.Where(x => x.IsSelected).Select(x => x.Day)];

        private void Refresh()
        {
            RefreshOrder();

            IsEmpty = Exercises.Count == 0;

            SummaryText = IsEmpty
                ? "Добавьте упражнения, чтобы по плану можно было тренироваться"
                : $"{RussianPlural.Exercises(Exercises.Count)} · {RussianPlural.WorkingSets(Exercises.Sum(x => x.Sets))}";
        }

        private static PlannedExerciseSet ToPlanned(PlannedExerciseViewModel exercise) =>
            new(
                exercise.ExerciseId,
                [.. Enumerable.Repeat(new PlannedWorkingSet(exercise.Reps, exercise.Weight), exercise.Sets)]);
    }
}
