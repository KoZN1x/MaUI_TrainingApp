using System.Globalization;
using Microsoft.Maui.Controls;

namespace MauiTrainApp.Converters
{
    public sealed class VolumeConverter : IValueConverter
    {
        private const double TonThreshold = 1000;

        public static string ToText(double volume)
        {
            return volume >= TonThreshold
                ? $"{(volume / TonThreshold).ToString("0.0", CultureInfo.CurrentCulture)} т"
                : $"{Math.Round(volume).ToString("0", CultureInfo.CurrentCulture)} кг";
        }

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is double volume ? ToText(volume) : "0 кг";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
