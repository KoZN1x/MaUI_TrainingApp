using MauiTrainApp.Domain.ValueObjects;

namespace MauiTrainApp.Formatting
{
    public static class WeekDays
    {
        public static readonly IReadOnlyList<DayOfWeek> Ordered =
        [
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday,
            DayOfWeek.Saturday,
            DayOfWeek.Sunday
        ];

        private static readonly string[] Short = ["Вс", "Пн", "Вт", "Ср", "Чт", "Пт", "Сб"];

        private static readonly string[] Accusative =
        [
            "в воскресенье", "в понедельник", "во вторник", "в среду",
            "в четверг", "в пятницу", "в субботу"
        ];

        public static string ToShort(DayOfWeek day) => Short[(int)day];

        public static string ToAccusative(DayOfWeek day) => Accusative[(int)day];

        public static string Describe(WeekSchedule schedule)
        {
            return schedule.IsEmpty
                ? "Без расписания"
                : string.Join(" · ", schedule.Days.Select(ToShort));
        }
    }
}
