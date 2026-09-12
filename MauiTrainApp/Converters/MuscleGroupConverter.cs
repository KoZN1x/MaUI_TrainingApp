using System.Globalization;
using MauiTrainApp.Domain.Enums;
using Microsoft.Maui.Controls;

namespace MauiTrainApp.Converters
{
    public sealed class MuscleGroupConverter : IValueConverter
    {
        private const string Unknown = "Другое";

        private static readonly Dictionary<MuscleGroup, string> Names = new()
        {
            [MuscleGroup.Chest] = "Грудь",
            [MuscleGroup.Back] = "Спина",
            [MuscleGroup.Legs] = "Ноги",
            [MuscleGroup.Shoulders] = "Плечи",
            [MuscleGroup.Arms] = "Руки",
            [MuscleGroup.Core] = "Корпус",
            [MuscleGroup.Other] = Unknown
        };

        public static string ToName(MuscleGroup muscleGroup) =>
            Names.TryGetValue(muscleGroup, out var name) ? name : Unknown;

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is MuscleGroup muscleGroup ? ToName(muscleGroup) : Unknown;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
