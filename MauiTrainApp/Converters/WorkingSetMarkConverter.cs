using System.Globalization;
using Microsoft.Maui.Controls;

namespace MauiTrainApp.Converters
{
    public sealed class WorkingSetMarkConverter : IValueConverter
    {
        private const string Completed = "✓";
        private const string Pending = "○";

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is true ? Completed : Pending;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
