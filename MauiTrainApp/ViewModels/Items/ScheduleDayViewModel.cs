using CommunityToolkit.Mvvm.ComponentModel;
using MauiTrainApp.Controls;
using MauiTrainApp.Formatting;

namespace MauiTrainApp.ViewModels.Items
{
    public sealed partial class ScheduleDayViewModel : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ChipKind))]
        private bool _isSelected;

        public ScheduleDayViewModel(DayOfWeek day, bool isSelected)
        {
            Day = day;
            Label = WeekDays.ToShort(day);
            _isSelected = isSelected;
        }

        public DayOfWeek Day { get; }

        public string Label { get; }

        public TagChipKind ChipKind => IsSelected ? TagChipKind.Accent : TagChipKind.Outline;
    }
}
