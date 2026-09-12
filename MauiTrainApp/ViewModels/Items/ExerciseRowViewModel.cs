using MauiTrainApp.Converters;
using MauiTrainApp.Domain.ReadModels;

namespace MauiTrainApp.ViewModels.Items
{
    public sealed record ExerciseRowViewModel(Guid Id, string Name, string MuscleName)
    {
        public static ExerciseRowViewModel From(ExerciseReadModel exercise) =>
            new(exercise.Id, exercise.Name, MuscleGroupConverter.ToName(exercise.MuscleGroup));
    }
}
