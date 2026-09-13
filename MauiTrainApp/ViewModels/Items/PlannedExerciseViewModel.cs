using CommunityToolkit.Mvvm.ComponentModel;
using MauiTrainApp.Domain.ReadModels;
using MauiTrainApp.Formatting;

namespace MauiTrainApp.ViewModels.Items
{
    public sealed partial class PlannedExerciseViewModel : ObservableObject
    {
        [ObservableProperty]
        private int _sets;

        [ObservableProperty]
        private byte _reps;

        [ObservableProperty]
        private double _weight;

        [ObservableProperty]
        private string _setsText;

        [ObservableProperty]
        private string _repsText;

        [ObservableProperty]
        private string _weightText;

        [ObservableProperty]
        private int _number;

        [ObservableProperty]
        private bool _canMoveUp;

        [ObservableProperty]
        private bool _canMoveDown;

        public PlannedExerciseViewModel(Guid exerciseId, string name, Guid? exerciseSetId, int sets, byte reps, double weight)
        {
            ExerciseId = exerciseId;
            Name = name;
            ExerciseSetId = exerciseSetId;
            _sets = sets;
            _reps = reps;
            _weight = weight;
            _setsText = NumberInput.Format(_sets);
            _repsText = NumberInput.Format(_reps);
            _weightText = NumberInput.Format(_weight);
        }

        public Guid ExerciseId { get; }

        public string Name { get; }

        public Guid? ExerciseSetId { get; }

        public static PlannedExerciseViewModel From(ExerciseSetReadModel exerciseSet)
        {
            var first = exerciseSet.WorkingSets.FirstOrDefault();

            return new PlannedExerciseViewModel(
                exerciseSet.ExerciseId,
                exerciseSet.ExerciseName,
                exerciseSet.Id,
                exerciseSet.WorkingSets.Count,
                first?.Reps ?? 10,
                first?.Weight ?? 20);
        }

        public void SetOrder(int index, int count)
        {
            Number = index + 1;
            CanMoveUp = index > 0;
            CanMoveDown = index < count - 1;
        }

        public void Apply(int sets, byte reps, double weight)
        {
            Sets = sets;
            Reps = reps;
            Weight = weight;

            SyncInputs();
        }

        public void SyncInputs()
        {
            SetsText = NumberInput.Format(Sets);
            RepsText = NumberInput.Format(Reps);
            WeightText = NumberInput.Format(Weight);
        }
    }
}
