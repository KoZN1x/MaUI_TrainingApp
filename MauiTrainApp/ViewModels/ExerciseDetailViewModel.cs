using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiTrainApp.Application.CQRS.Commands.DeleteExercise;
using MauiTrainApp.Application.CQRS.Commands.UpdateExercise;
using MauiTrainApp.Application.CQRS.Queries.GetExerciseProgress;
using MauiTrainApp.Controls;
using MauiTrainApp.Converters;
using MauiTrainApp.Domain.Enums;
using MauiTrainApp.Domain.ReadModels;
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
    public sealed partial class ExerciseDetailViewModel : ViewModelBase, IQueryAttributable
    {
        private const int ChartSessions = 8;
        private const int TableSessions = 8;
        private const string NoValue = "—";

        private Guid _exerciseId;
        private MuscleGroup _muscleGroup;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _muscleName = string.Empty;

        [ObservableProperty]
        private string _plansText = string.Empty;

        [ObservableProperty]
        private bool _hasPlans;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private string _descriptionDraft = string.Empty;

        [ObservableProperty]
        private bool _hasDescription;

        [ObservableProperty]
        private bool _isEditing;

        [ObservableProperty]
        private string _bestSetText = NoValue;

        [ObservableProperty]
        private string _oneRepMaxText = NoValue;

        [ObservableProperty]
        private string _bestVolumeText = NoValue;

        [ObservableProperty]
        private string _sessionCountText = "0";

        [ObservableProperty]
        private IReadOnlyList<BarChartItem> _weightChart = [];

        [ObservableProperty]
        private bool _hasSessions;

        private readonly INavigator _navigator;
        private readonly IDialogService _dialogs;

        public ExerciseDetailViewModel(
            IServiceScopeFactory scopeFactory,
            IExceptionPresenter exceptionPresenter,
            INavigator navigator,
            IDialogService dialogs)
            : base(scopeFactory, exceptionPresenter)
        {
            _navigator = navigator;
            _dialogs = dialogs;
        }

        public ObservableCollection<ExerciseSessionRowViewModel> Sessions { get; } = [];

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue(AppRoutes.ExerciseIdParameter, out var value) && value is Guid exerciseId)
            {
                _exerciseId = exerciseId;
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
                var progress = (await QueryAsync(new GetExerciseProgressQuery(_exerciseId), token)).Progress;

                _muscleGroup = progress.MuscleGroup;

                Name = progress.Name;
                MuscleName = MuscleGroupConverter.ToName(progress.MuscleGroup);

                HasPlans = progress.TrainingPlanNames.Count > 0;
                PlansText = HasPlans
                    ? $"В планах: {string.Join(", ", progress.TrainingPlanNames)}"
                    : "Не входит ни в один план";

                Description = progress.Description ?? string.Empty;
                DescriptionDraft = Description;
                HasDescription = !string.IsNullOrWhiteSpace(Description);
                IsEditing = false;

                ApplyRecords(progress);
                ApplySessions(progress);
            }, cancellationToken);
        }

        [RelayCommand]
        private void StartEdit()
        {
            DescriptionDraft = Description;
            IsEditing = true;
        }

        [RelayCommand]
        private void CancelEdit()
        {
            DescriptionDraft = Description;
            IsEditing = false;
        }

        [RelayCommand]
        private Task SaveDescriptionAsync(CancellationToken cancellationToken)
        {
            return RunAsync(async token =>
            {
                var description = string.IsNullOrWhiteSpace(DescriptionDraft) ? null : DescriptionDraft.Trim();

                await SendAsync(new UpdateExerciseCommand(_exerciseId, Name, description, _muscleGroup), token);

                Description = description ?? string.Empty;
                DescriptionDraft = Description;
                HasDescription = description is not null;
                IsEditing = false;
            }, cancellationToken);
        }

        [RelayCommand]
        private Task DeleteAsync(CancellationToken cancellationToken)
        {
            return RunAsync(async token =>
            {
                var confirmed = await _dialogs.ConfirmAsync(
                    "Удалить упражнение?",
                    $"«{Name}» исчезнет из каталога. Упражнение, входящее в план или проведённую тренировку, удалить нельзя.",
                    "Удалить");

                if (!confirmed)
                {
                    return;
                }

                await SendAsync(new DeleteExerciseCommand(_exerciseId), token);

                await _navigator.GoBackAsync();
            }, cancellationToken);
        }

        private void ApplyRecords(ExerciseProgressReadModel progress)
        {
            SessionCountText = $"{progress.SessionCount}";

            if (progress.SessionCount == 0 || progress.BestWeight <= 0)
            {
                BestSetText = NoValue;
                OneRepMaxText = NoValue;
                BestVolumeText = NoValue;

                return;
            }

            BestSetText = $"{progress.BestWeightReps}×{Weight(progress.BestWeight)}";
            OneRepMaxText = $"{Weight(progress.EstimatedOneRepMax)} кг";
            BestVolumeText = VolumeConverter.ToText(progress.BestVolume);
        }

        private void ApplySessions(ExerciseProgressReadModel progress)
        {
            var recent = progress.Sessions.Take(TableSessions).ToList();

            Sessions.Clear();

            foreach (var session in recent)
            {
                Sessions.Add(ExerciseSessionRowViewModel.From(session));
            }

            HasSessions = recent.Count > 0;

            var chartSessions = progress.Sessions.Take(ChartSessions).Reverse().ToList();

            WeightChart =
            [
                .. chartSessions.Select((session, index) => new BarChartItem(
                    WorkoutDayConverter.ToShortText(session.WorkoutDay),
                    session.BestWeight,
                    Weight(session.BestWeight),
                    index == chartSessions.Count - 1))
            ];
        }

        private static string Weight(double weight) => weight % 1 == 0 ? $"{weight:0}" : $"{weight:0.#}";
    }
}
