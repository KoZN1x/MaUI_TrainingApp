using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiTrainApp.Application.CQRS.Commands.CreateExercise;
using MauiTrainApp.Application.CQRS.Queries.GetExercises;
using MauiTrainApp.Converters;
using MauiTrainApp.Domain.Enums;
using MauiTrainApp.ExceptionHandler.Interfaces;
using MauiTrainApp.Navigation.Interfaces;
using MauiTrainApp.Startup;
using MauiTrainApp.ViewModels.Base;
using MauiTrainApp.ViewModels.Items;
using Microsoft.Extensions.DependencyInjection;

namespace MauiTrainApp.ViewModels
{
    public sealed partial class ExercisesViewModel : ViewModelBase
    {
        private static readonly MuscleGroup[] FilterGroups =
        [
            MuscleGroup.Chest,
            MuscleGroup.Back,
            MuscleGroup.Legs,
            MuscleGroup.Shoulders,
            MuscleGroup.Arms,
            MuscleGroup.Core,
            MuscleGroup.Other
        ];

        private readonly AppStartup _startup;
        private readonly INavigator _navigator;

        private List<ExerciseRowViewModel> _allExercises = [];

        [ObservableProperty]
        private string _searchTerm = string.Empty;

        [ObservableProperty]
        private bool _isEmpty;

        [ObservableProperty]
        private bool _isCreating;

        [ObservableProperty]
        private string _newName = string.Empty;

        [ObservableProperty]
        private string _newDescription = string.Empty;

        [ObservableProperty]
        private string _newMuscleName = string.Empty;

        public ExercisesViewModel(
            IServiceScopeFactory scopeFactory,
            IExceptionPresenter exceptionPresenter,
            AppStartup startup,
            INavigator navigator)
            : base(scopeFactory, exceptionPresenter)
        {
            _startup = startup;
            _navigator = navigator;

            Filters = [new MuscleFilterViewModel("Все", null)];

            foreach (var muscleGroup in FilterGroups)
            {
                Filters.Add(new MuscleFilterViewModel(MuscleGroupConverter.ToName(muscleGroup), muscleGroup));
            }

            Filters[0].IsSelected = true;

            MuscleNames = [.. FilterGroups.Select(MuscleGroupConverter.ToName)];
            NewMuscleName = MuscleGroupConverter.ToName(MuscleGroup.Other);
        }

        public ObservableCollection<ExerciseRowViewModel> Exercises { get; } = [];

        public ObservableCollection<MuscleFilterViewModel> Filters { get; }

        public IReadOnlyList<string> MuscleNames { get; }

        public override Task AppearingAsync(CancellationToken cancellationToken = default)
        {
            return LoadCommand.ExecuteAsync(null);
        }

        partial void OnSearchTermChanged(string value) => ApplyFilters();

        [RelayCommand]
        private Task LoadAsync(CancellationToken cancellationToken)
        {
            return RunAsync(async token =>
            {
                await _startup.EnsureReadyAsync();

                var result = await QueryAsync(new GetExercisesQuery(), token);

                _allExercises = [.. result.Exercises.Select(ExerciseRowViewModel.From)];

                ApplyFilters();
            }, cancellationToken);
        }

        [RelayCommand]
        private void SelectFilter(MuscleFilterViewModel filter)
        {
            foreach (var item in Filters)
            {
                item.IsSelected = ReferenceEquals(item, filter);
            }

            ApplyFilters();
        }

        [RelayCommand]
        private Task OpenExerciseAsync(ExerciseRowViewModel exercise)
        {
            return _navigator.GoToExerciseDetailsAsync(exercise.Id);
        }

        [RelayCommand]
        private void StartCreate()
        {
            NewName = string.Empty;
            NewDescription = string.Empty;
            NewMuscleName = MuscleGroupConverter.ToName(MuscleGroup.Other);
            IsCreating = true;
        }

        [RelayCommand]
        private void CancelCreate()
        {
            IsCreating = false;
        }

        [RelayCommand]
        private Task CreateExerciseAsync(CancellationToken cancellationToken)
        {
            return RunAsync(async token =>
            {
                var description = string.IsNullOrWhiteSpace(NewDescription) ? null : NewDescription.Trim();

                await SendAsync(
                    new CreateExerciseCommand(NewName.Trim(), description, ToMuscleGroup(NewMuscleName)),
                    token);

                IsCreating = false;

                var result = await QueryAsync(new GetExercisesQuery(), token);

                _allExercises = [.. result.Exercises.Select(ExerciseRowViewModel.From)];

                ApplyFilters();
            }, cancellationToken);
        }

        private void ApplyFilters()
        {
            var muscleGroup = Filters.FirstOrDefault(x => x.IsSelected)?.MuscleGroup;
            var term = SearchTerm.Trim();

            var filtered = _allExercises
                .Where(x => muscleGroup is null || x.MuscleGroup == muscleGroup)
                .Where(x => term.Length == 0 || x.Name.Contains(term, StringComparison.CurrentCultureIgnoreCase));

            Exercises.Clear();

            foreach (var exercise in filtered)
            {
                Exercises.Add(exercise);
            }

            IsEmpty = Exercises.Count == 0;
        }

        private static MuscleGroup ToMuscleGroup(string name) =>
            FilterGroups.FirstOrDefault(x => MuscleGroupConverter.ToName(x) == name, MuscleGroup.Other);
    }
}
