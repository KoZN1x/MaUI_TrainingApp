using CommunityToolkit.Mvvm.ComponentModel;
using MauiTrainApp.Controls;
using MauiTrainApp.Domain.Enums;

namespace MauiTrainApp.ViewModels.Items
{
    public sealed partial class MuscleFilterViewModel : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Kind))]
        private bool _isSelected;

        public MuscleFilterViewModel(string name, MuscleGroup? muscleGroup)
        {
            Name = name;
            MuscleGroup = muscleGroup;
        }

        public string Name { get; }

        public MuscleGroup? MuscleGroup { get; }

        public TagChipKind Kind => IsSelected ? TagChipKind.Accent : TagChipKind.Outline;
    }
}
