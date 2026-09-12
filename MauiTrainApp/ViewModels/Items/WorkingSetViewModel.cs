using CommunityToolkit.Mvvm.ComponentModel;
using MauiTrainApp.Domain.ReadModels;

namespace MauiTrainApp.ViewModels.Items
{
    public sealed partial class WorkingSetViewModel : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RepsText))]
        private byte _reps;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(WeightText))]
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

        public string RepsText => $"{Reps}";

        public string WeightText => Weight % 1 == 0 ? $"{Weight:0}" : $"{Weight:0.#}";

        public double Volume => IsCompleted ? Reps * Weight : 0;
    }
}
