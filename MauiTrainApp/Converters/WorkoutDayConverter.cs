using System.Globalization;
using Microsoft.Maui.Controls;

namespace MauiTrainApp.Converters
{
    public sealed class WorkoutDayConverter : IValueConverter
    {
        public const string ShortFormat = "short";
        public const string WeekdayFormat = "weekday";
        public const string MonthFormat = "month";

        private static readonly string[] Months =
        [
            "января", "февраля", "марта", "апреля", "мая", "июня",
            "июля", "августа", "сентября", "октября", "ноября", "декабря"
        ];

        private static readonly string[] NominativeMonths =
        [
            "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь",
            "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь"
        ];

        private static readonly string[] Weekdays = ["вс", "пн", "вт", "ср", "чт", "пт", "сб"];

        public static string ToText(DateOnly day) => $"{day.Day} {Months[day.Month - 1]}";

        public static string ToShortText(DateOnly day) => $"{day.Day} {Months[day.Month - 1][..3]}";

        public static string ToWeekday(DateOnly day) => Weekdays[(int)day.DayOfWeek];

        public static string ToMonthText(DateOnly day) => $"{NominativeMonths[day.Month - 1]} {day.Year}";

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not DateOnly day)
            {
                return string.Empty;
            }

            return (parameter as string) switch
            {
                ShortFormat => ToShortText(day),
                WeekdayFormat => ToWeekday(day),
                MonthFormat => ToMonthText(day),
                _ => ToText(day)
            };
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
