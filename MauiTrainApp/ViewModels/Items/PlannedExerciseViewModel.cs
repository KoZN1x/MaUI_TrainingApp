using CommunityToolkit.Mvvm.ComponentModel;
using MauiTrainApp.Domain.ReadModels;

namespace MauiTrainApp.ViewModels.Items
{
    public sealed partial class PlannedExerciseViewModel : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SetsText))]
        private int _sets;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RepsText))]
        private byte _reps;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(WeightText))]
        private double _weight;

        public PlannedExerciseViewModel(Guid exerciseId, string name, Guid? exerciseSetId, int sets, byte reps, double weight)
        {
            ExerciseId = exerciseId;
            Name = name;
            ExerciseSetId = exerciseSetId;
            _sets = sets;
            _reps = reps;
            _weight = weight;
        }

        public Guid ExerciseId { get; }

        public string Name { get; }

        public Guid? ExerciseSetId { get; }

        public string SetsText => $"{Sets}";

        public string RepsText => $"{Reps}";

        public string WeightText => Weight % 1 == 0 ? $"{Weight:0}" : $"{Weight:0.#}";

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
    }
}
