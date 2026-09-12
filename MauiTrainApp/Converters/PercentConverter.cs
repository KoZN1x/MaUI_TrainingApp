using System.Globalization;
using Microsoft.Maui.Controls;

namespace MauiTrainApp.Converters
{
    public sealed class PercentConverter : IValueConverter
    {
        public static string ToText(double ratio)
        {
            var percent = Math.Round(ratio * 100);

            return percent switch
            {
                > 0 => $"+{percent.ToString("0", CultureInfo.CurrentCulture)} %",
                < 0 => $"−{Math.Abs(percent).ToString("0", CultureInfo.CurrentCulture)} %",
                _ => "0 %"
            };
        }

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is double ratio ? ToText(ratio) : "0 %";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
