namespace MauiTrainApp.Formatting
{
    public static class RussianPlural
    {
        public static string Of(int count, string one, string few, string many)
        {
            var tail = Math.Abs(count) % 100;

            if (tail is >= 11 and <= 14)
            {
                return many;
            }

            return (tail % 10) switch
            {
                1 => one,
                2 or 3 or 4 => few,
                _ => many
            };
        }

        public static string Format(int count, string one, string few, string many) =>
            $"{count} {Of(count, one, few, many)}";

        public static string Exercises(int count) =>
            Format(count, "упражнение", "упражнения", "упражнений");

        public static string WorkingSets(int count) =>
            Format(count, "подход", "подхода", "подходов");

        public static string Workouts(int count) =>
            Format(count, "тренировка", "тренировки", "тренировок");

        public static string Weeks(int count) =>
            Format(count, "неделя", "недели", "недель");
    }
}
