using System.Globalization;

namespace MauiTrainApp.Formatting
{
    public static class NumberInput
    {
        public static string Format(double value)
        {
            return value % 1 == 0
                ? value.ToString("0", CultureInfo.InvariantCulture)
                : value.ToString("0.##", CultureInfo.InvariantCulture);
        }

        public static double Weight(string? text, double fallback, double maximum)
        {
            return TryParse(text, out var value)
                ? Math.Round(Math.Clamp(value, 0, maximum), 2)
                : fallback;
        }

        public static byte Reps(string? text, byte fallback, byte minimum, byte maximum)
        {
            return TryParse(text, out var value)
                ? (byte)Math.Clamp(Math.Round(value), minimum, maximum)
                : fallback;
        }

        public static int Sets(string? text, int fallback, int minimum, int maximum)
        {
            return TryParse(text, out var value)
                ? (int)Math.Clamp(Math.Round(value), minimum, maximum)
                : fallback;
        }

        private static bool TryParse(string? text, out double value)
        {
            value = 0;

            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            var normalized = text.Trim().Replace(',', '.');

            return double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
                && double.IsFinite(value);
        }
    }
}
