using Microsoft.Maui.Graphics;

namespace MauiTrainApp.ViewModels.Items
{
    public sealed record WeekDayViewModel(string Label, string DayNumber, bool HasWorkout, bool IsToday)
    {
        private static readonly Color Accent = Color.FromArgb("#9184D9");
        private static readonly Color AccentSurface = Color.FromArgb("#2B2741");
        private static readonly Color Muted = Color.FromArgb("#9397AB");
        private static readonly Color Text = Color.FromArgb("#E9E9ED");

        public Color Stroke => IsToday ? Accent : Colors.Transparent;

        public Color Background => IsToday ? AccentSurface : Colors.Transparent;

        public Color LabelColor => IsToday ? Accent : Muted;

        public Color DayColor => IsToday ? Accent : Text;

        public Color MarkColor => IsToday ? Accent : Muted;
    }
}
