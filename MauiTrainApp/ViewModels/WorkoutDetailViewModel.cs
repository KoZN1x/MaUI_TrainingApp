using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiTrainApp.Application.CQRS.Commands.StartWorkoutFromPlan;
using MauiTrainApp.Application.CQRS.Queries.GetWorkoutDetails;
using MauiTrainApp.Controls;
using MauiTrainApp.Converters;
using MauiTrainApp.Domain.ReadModels;
using MauiTrainApp.ExceptionHandler.Interfaces;
using MauiTrainApp.Formatting;
using MauiTrainApp.Navigation;
using MauiTrainApp.Navigation.Interfaces;
using MauiTrainApp.ViewModels.Base;
using MauiTrainApp.ViewModels.Items;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;

namespace MauiTrainApp.ViewModels
{
    public sealed partial class WorkoutDetailViewModel : ViewModelBase, IQueryAttributable
    {
        private readonly INavigator _navigator;

        private Guid _workoutId;
        private Guid? _trainingPlanId;

        [ObservableProperty]
        private string _title = "Тренировка";

        [ObservableProperty]
        private string _dayText = string.Empty;

        [ObservableProperty]
        private string _statusText = string.Empty;

        [ObservableProperty]
        private TagChipKind _statusKind = TagChipKind.Outline;

        [ObservableProperty]
        private string _volumeText = "0 кг";

        [ObservableProperty]
        private string _durationText = "—";

        [ObservableProperty]
        private string _workingSetText = string.Empty;

        [ObservableProperty]
        private bool _canRepeat;

        [ObservableProperty]
        private bool _isLoaded;

        public WorkoutDetailViewModel(
            IServiceScopeFactory scopeFactory,
            IExceptionPresenter exceptionPresenter,
            INavigator navigator)
            : base(scopeFactory, exceptionPresenter)
        {
            _navigator = navigator;
        }

        public ObservableCollection<WorkoutDetailExerciseViewModel> Exercises { get; } = [];

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

        [RelayCommand]
        private Task LoadAsync(CancellationToken cancellationToken)
        {
            return RunAsync(async token =>
            {
                var workout = (await QueryAsync(new GetWorkoutDetailsQuery(_workoutId), token)).Workout;

                Exercises.Clear();

                if (workout is null)
                {
                    IsLoaded = false;
                    CanRepeat = false;

                    return;
                }

                foreach (var exerciseSet in workout.ExerciseSets)
                {
                    Exercises.Add(WorkoutDetailExerciseViewModel.From(exerciseSet));
                }

                Apply(workout);

                IsLoaded = true;
            }, cancellationToken);
        }

        [RelayCommand]
        private Task RepeatAsync(CancellationToken cancellationToken)
        {
            return RunAsync(async token =>
            {
                if (_trainingPlanId is not { } trainingPlanId)
                {
                    return;
                }

                var started = await SendAsync(
                    new StartWorkoutFromPlanCommand(trainingPlanId, DateOnly.FromDateTime(DateTime.Now)),
                    token);

                await _navigator.GoToWorkoutAsync(started.WorkoutId);
            }, cancellationToken);
        }

        private void Apply(WorkoutDetailsReadModel workout)
        {
            _trainingPlanId = workout.TrainingPlanId;

            var workingSets = workout.ExerciseSets.SelectMany(x => x.WorkingSets).ToList();
            var completed = workingSets.Count(x => x.IsCompleted);

            Title = workout.TrainingPlanName ?? "Тренировка";
            DayText = $"{WorkoutDayConverter.ToText(workout.WorkoutDay)}, {WorkoutDayConverter.ToWeekday(workout.WorkoutDay)}";

            StatusText = workout.IsCompleted ? "Выполнена" : "Частично";
            StatusKind = workout.IsCompleted ? TagChipKind.Accent : TagChipKind.Outline;

            VolumeText = VolumeConverter.ToText(workout.TotalVolume);
            DurationText = DurationConverter.ToText(workout.Duration);
            WorkingSetText = $"{completed} из {RussianPlural.WorkingSets(workingSets.Count)}";

            CanRepeat = workout.TrainingPlanId is not null;
        }
    }
}
