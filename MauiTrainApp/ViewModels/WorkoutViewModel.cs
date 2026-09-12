using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiTrainApp.Application.CQRS.Commands.CompleteExerciseSet;
using MauiTrainApp.Application.CQRS.Commands.CompleteWorkingSet;
using MauiTrainApp.Application.CQRS.Commands.ResetWorkingSet;
using MauiTrainApp.Application.CQRS.Commands.UpdatePerformedWorkingSet;
using MauiTrainApp.Application.CQRS.Queries.GetWorkoutDetails;
using MauiTrainApp.ExceptionHandler.Interfaces;
using MauiTrainApp.Navigation;
using MauiTrainApp.ViewModels.Base;
using MauiTrainApp.ViewModels.Items;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;

namespace MauiTrainApp.ViewModels
{
    public sealed partial class WorkoutViewModel : ViewModelBase, IQueryAttributable
    {
        private Guid _workoutId;

        public WorkoutViewModel(IServiceScopeFactory scopeFactory, IExceptionPresenter exceptionPresenter)
            : base(scopeFactory, exceptionPresenter)
        {
        }

        public ObservableCollection<WorkoutExerciseViewModel> Exercises { get; } = [];

        [ObservableProperty]
        private string _title = string.Empty;

        [ObservableProperty]
        private string _subtitle = string.Empty;

        [ObservableProperty]
        private double _progress;

        [ObservableProperty]
        private bool _isCompleted;

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
                var result = await QueryAsync(new GetWorkoutDetailsQuery(_workoutId), token);

                Exercises.Clear();

                if (result.Workout is null)
                {
                    Title = string.Empty;
                    Subtitle = string.Empty;

                    return;
                }

                foreach (var exerciseSet in result.Workout.ExerciseSets)
                {
                    Exercises.Add(new WorkoutExerciseViewModel(exerciseSet));
                }

                Title = result.Workout.TrainingPlanName ?? "Тренировка";
                Subtitle = result.Workout.WorkoutDay.ToString("dd.MM.yyyy");

                RefreshProgress();
            }, cancellationToken);
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

                RefreshProgress();
            }, cancellationToken);
        }

        [RelayCommand]
        private Task SaveWorkingSetAsync(WorkingSetViewModel workingSet, CancellationToken cancellationToken)
        {
            return RunAsync(token => SendAsync(
                new UpdatePerformedWorkingSetCommand(
                    _workoutId,
                    workingSet.ExerciseSetId,
                    workingSet.Index,
                    workingSet.Reps,
                    workingSet.Weight),
                token), cancellationToken);
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

                RefreshProgress();
            }, cancellationToken);
        }

        private void RefreshProgress()
        {
            var workingSets = Exercises.SelectMany(x => x.WorkingSets).ToList();

            var completed = workingSets.Count(x => x.IsCompleted);

            Progress = workingSets.Count == 0 ? 0 : (double)completed / workingSets.Count;
            IsCompleted = workingSets.Count > 0 && completed == workingSets.Count;
        }
    }
}
