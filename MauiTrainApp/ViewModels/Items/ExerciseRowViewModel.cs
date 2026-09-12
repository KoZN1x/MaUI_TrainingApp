using MauiTrainApp.Converters;
using MauiTrainApp.Domain.Enums;
using MauiTrainApp.Domain.ReadModels;

namespace MauiTrainApp.ViewModels.Items
{
    public sealed record ExerciseRowViewModel(
        Guid Id,
        string Name,
        string MuscleName,
        MuscleGroup MuscleGroup,
        string BestResultText)
    {
        public static ExerciseRowViewModel From(ExerciseReadModel exercise)
        {
            var weight = exercise.BestWeight % 1 == 0
                ? $"{exercise.BestWeight:0}"
                : $"{exercise.BestWeight:0.#}";

            return new ExerciseRowViewModel(
                exercise.Id,
                exercise.Name,
                MuscleGroupConverter.ToName(exercise.MuscleGroup),
                exercise.MuscleGroup,
                exercise.HasResult ? $"Лучший: {exercise.BestWeightReps}×{weight}" : "Ещё не выполнялось");
        }
    }
}
