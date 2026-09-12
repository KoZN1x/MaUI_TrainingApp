using MauiTrainApp.Converters;
using MauiTrainApp.Domain.ReadModels;

namespace MauiTrainApp.ViewModels.Items
{
    public sealed record ExerciseSessionRowViewModel(string DayText, string SetsText, string VolumeText)
    {
        public static ExerciseSessionRowViewModel From(ExerciseSessionReadModel session) =>
            new(
                $"{WorkoutDayConverter.ToText(session.WorkoutDay)}, {WorkoutDayConverter.ToWeekday(session.WorkoutDay)}",
                string.Join(" · ", session.WorkingSets.Select(ToSetText)),
                VolumeConverter.ToText(session.Volume));

        private static string ToSetText(WorkingSetReadModel workingSet)
        {
            var weight = workingSet.Weight % 1 == 0
                ? $"{workingSet.Weight:0}"
                : $"{workingSet.Weight:0.#}";

            return $"{workingSet.Reps}×{weight}";
        }
    }
}
