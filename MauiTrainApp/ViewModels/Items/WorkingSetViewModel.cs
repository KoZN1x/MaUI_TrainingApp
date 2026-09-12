using CommunityToolkit.Mvvm.ComponentModel;
using MauiTrainApp.Domain.ReadModels;

namespace MauiTrainApp.ViewModels.Items
{
    public sealed partial class WorkingSetViewModel : ObservableObject
    {
        [ObservableProperty]
        private byte _reps;

        [ObservableProperty]
        private double _weight;

        [ObservableProperty]
        private bool _isCompleted;

        public WorkingSetViewModel(Guid exerciseSetId, int index, WorkingSetReadModel workingSet)
        {
            ExerciseSetId = exerciseSetId;
            Index = index;
            _reps = workingSet.Reps;
            _weight = workingSet.Weight;
            _isCompleted = workingSet.IsCompleted;
        }

        public Guid ExerciseSetId { get; }

        public int Index { get; }

        public int Number => Index + 1;
    }
}
