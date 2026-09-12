using MauiTrainApp.Controls;
using MauiTrainApp.Domain.ReadModels;

namespace MauiTrainApp.ViewModels.Items
{
    public sealed record WorkingSetChipViewModel(string Text, TagChipKind Kind)
    {
        public static WorkingSetChipViewModel From(WorkingSetReadModel workingSet)
        {
            var weight = workingSet.Weight % 1 == 0 ? $"{workingSet.Weight:0}" : $"{workingSet.Weight:0.#}";

            return new WorkingSetChipViewModel(
                $"{workingSet.Reps}×{weight}",
                workingSet.IsCompleted ? TagChipKind.Accent : TagChipKind.Outline);
        }
    }
}
