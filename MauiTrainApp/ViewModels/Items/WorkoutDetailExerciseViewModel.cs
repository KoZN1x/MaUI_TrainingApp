using MauiTrainApp.Converters;
using MauiTrainApp.Domain.ReadModels;

namespace MauiTrainApp.ViewModels.Items
{
    public sealed record WorkoutDetailExerciseViewModel(
        string Name,
        string SummaryText,
        IReadOnlyList<WorkingSetChipViewModel> WorkingSets)
    {
        public static WorkoutDetailExerciseViewModel From(ExerciseSetReadModel exerciseSet)
        {
            var completed = exerciseSet.WorkingSets.Count(x => x.IsCompleted);

            return new WorkoutDetailExerciseViewModel(
                exerciseSet.ExerciseName,
                $"{completed} / {exerciseSet.WorkingSets.Count} · {VolumeConverter.ToText(exerciseSet.CompletedVolume)}",
                [.. exerciseSet.WorkingSets.Select(WorkingSetChipViewModel.From)]);
        }
    }
}
