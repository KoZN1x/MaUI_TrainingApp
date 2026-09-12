using System.Globalization;
using Microsoft.Maui.Controls;

namespace MauiTrainApp.Converters
{
    public sealed class DurationConverter : IValueConverter
    {
        private const string Empty = "—";

        public static string ToText(TimeSpan? duration)
        {
            if (duration is not { } value || value <= TimeSpan.Zero)
            {
                return Empty;
            }

            var hours = (int)value.TotalHours;
            var minutes = value.Minutes;

            if (hours > 0)
            {
                return minutes > 0 ? $"{hours} ч {minutes} мин" : $"{hours} ч";
            }

            return minutes > 0 ? $"{minutes} мин" : $"{value.Seconds} с";
        }

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value switch
            {
                TimeSpan duration => ToText(duration),
                null => Empty,
                _ => Empty
            };
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
