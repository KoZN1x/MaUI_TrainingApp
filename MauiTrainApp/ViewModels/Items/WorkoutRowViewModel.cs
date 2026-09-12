using MauiTrainApp.Controls;
using MauiTrainApp.Converters;
using MauiTrainApp.Domain.ReadModels;

namespace MauiTrainApp.ViewModels.Items
{
    public sealed record WorkoutRowViewModel(
        Guid Id,
        string Title,
        string DayText,
        string MetaText,
        string TagText,
        TagChipKind TagKind)
    {
        public static WorkoutRowViewModel From(WorkoutListItemReadModel workout)
        {
            var meta = workout.Duration is null
                ? VolumeConverter.ToText(workout.TotalVolume)
                : $"{VolumeConverter.ToText(workout.TotalVolume)} · {DurationConverter.ToText(workout.Duration)}";

            return new WorkoutRowViewModel(
                workout.Id,
                workout.TrainingPlanName ?? "Тренировка",
                $"{WorkoutDayConverter.ToText(workout.WorkoutDay)}, {WorkoutDayConverter.ToWeekday(workout.WorkoutDay)}",
                meta,
                workout.IsCompleted ? "Выполнена" : "Частично",
                workout.IsCompleted ? TagChipKind.Accent : TagChipKind.Outline);
        }
    }
}
