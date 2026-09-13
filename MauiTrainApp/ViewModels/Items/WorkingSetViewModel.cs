using CommunityToolkit.Mvvm.ComponentModel;
using MauiTrainApp.Domain.ReadModels;
using MauiTrainApp.Formatting;

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

        [ObservableProperty]
        private string _repsText;

        [ObservableProperty]
        private string _weightText;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Number))]
        private int _index;

        public WorkingSetViewModel(Guid exerciseSetId, int index, WorkingSetReadModel workingSet)
        {
            ExerciseSetId = exerciseSetId;
            _index = index;
            _reps = workingSet.Reps;
            _weight = workingSet.Weight;
            _isCompleted = workingSet.IsCompleted;
            _repsText = NumberInput.Format(_reps);
            _weightText = NumberInput.Format(_weight);
        }

        public Guid ExerciseSetId { get; }

        public int Number => Index + 1;

        public double Volume => IsCompleted ? Reps * Weight : 0;

        public void Apply(byte reps, double weight)
        {
            Reps = reps;
            Weight = weight;

            SyncInputs();
        }

        public void SyncInputs()
        {
            RepsText = NumberInput.Format(Reps);
            WeightText = NumberInput.Format(Weight);
        }
    }
}
