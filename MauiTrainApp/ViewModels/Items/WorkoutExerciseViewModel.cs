using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MauiTrainApp.Converters;
using MauiTrainApp.Domain.ReadModels;

namespace MauiTrainApp.ViewModels.Items
{
    public sealed partial class WorkoutExerciseViewModel : ObservableObject
    {
        private const string CompletedBadge = "✓";

        [ObservableProperty]
        private bool _isCompleted;

        [ObservableProperty]
        private bool _isExpanded;

        [ObservableProperty]
        private string _badgeText = string.Empty;

        [ObservableProperty]
        private string _summaryText = string.Empty;

        public WorkoutExerciseViewModel(ExerciseSetReadModel exerciseSet, int number)
        {
            Id = exerciseSet.Id;
            Number = number;
            ExerciseId = exerciseSet.ExerciseId;
            ExerciseName = exerciseSet.ExerciseName;

            WorkingSets = [.. exerciseSet.WorkingSets.Select((x, index) => new WorkingSetViewModel(Id, index, x))];

            foreach (var workingSet in WorkingSets)
            {
                workingSet.PropertyChanged += (_, _) => Refresh();
            }

            Refresh();
        }

        public Guid Id { get; }

        public int Number { get; }

        public Guid ExerciseId { get; }

        public string ExerciseName { get; }

        public ObservableCollection<WorkingSetViewModel> WorkingSets { get; }

        public double Volume => WorkingSets.Sum(x => x.Volume);

        public void Add(WorkingSetViewModel workingSet)
        {
            workingSet.PropertyChanged += (_, _) => Refresh();

            WorkingSets.Add(workingSet);

            Refresh();
        }

        public void Remove(WorkingSetViewModel workingSet)
        {
            WorkingSets.Remove(workingSet);

            for (var index = 0; index < WorkingSets.Count; index++)
            {
                WorkingSets[index].Index = index;
            }

            Refresh();
        }

        public void Refresh()
        {
            var completed = WorkingSets.Count(x => x.IsCompleted);

            IsCompleted = WorkingSets.Count > 0 && completed == WorkingSets.Count;
            BadgeText = IsCompleted ? CompletedBadge : $"{Number}";
            SummaryText = $"{completed} / {WorkingSets.Count} · {VolumeConverter.ToText(Volume)}";
        }
    }
}
