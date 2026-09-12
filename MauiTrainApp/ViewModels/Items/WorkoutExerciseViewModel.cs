using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MauiTrainApp.Domain.ReadModels;

namespace MauiTrainApp.ViewModels.Items
{
    public sealed partial class WorkoutExerciseViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isCompleted;

        [ObservableProperty]
        private string _progress = string.Empty;

        public WorkoutExerciseViewModel(ExerciseSetReadModel exerciseSet)
        {
            Id = exerciseSet.Id;
            ExerciseName = exerciseSet.ExerciseName;

            WorkingSets = [.. exerciseSet.WorkingSets.Select((x, index) => new WorkingSetViewModel(Id, index, x))];

            foreach (var workingSet in WorkingSets)
            {
                workingSet.PropertyChanged += (_, _) => Refresh();
            }

            Refresh();
        }

        public Guid Id { get; }

        public string ExerciseName { get; }

        public ObservableCollection<WorkingSetViewModel> WorkingSets { get; }

        private void Refresh()
        {
            var completed = WorkingSets.Count(x => x.IsCompleted);

            IsCompleted = WorkingSets.Count > 0 && completed == WorkingSets.Count;
            Progress = $"{completed} / {WorkingSets.Count}";
        }
    }
}
