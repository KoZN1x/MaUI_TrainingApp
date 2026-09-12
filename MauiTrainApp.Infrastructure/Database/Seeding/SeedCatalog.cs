using MauiTrainApp.Domain.Enums;

namespace MauiTrainApp.Infrastructure.Database.Seeding
{
    internal static class SeedCatalog
    {
        public const string SmithSquat = "Присед в Смите";
        public const string DumbbellBenchPress = "Жим гантелей лёжа";
        public const string LatPulldown = "Тяга верхнего блока к груди";
        public const string ReverseGripPulldown = "Тяга верхнего блока обратным хватом";
        public const string LateralRaise = "Махи гантелями в стороны";
        public const string RomanianDeadlift = "Румынская тяга";
        public const string LegPress = "Жим ногами";
        public const string InclineDumbbellPress = "Жим гантелей на наклонной";
        public const string SeatedRow = "Тяга нижнего блока к поясу";
        public const string SeatedShoulderPress = "Жим гантелей сидя вверх";
        public const string BarbellCurl = "Штанга на бицепс";
        public const string RearDeltFly = "Разводки в наклоне на заднюю дельту";
        public const string LyingLegCurl = "Сгибание ног лёжа";
        public const string TricepsRopePushdown = "Канат на трицепс";
        public const string StandingCalfRaise = "Подъёмы на носки стоя";
        public const string Crunches = "Скручивания";
        public const string ChestFly = "Разводки на грудь";

        private const string Base = "База. Отдых 2-3 мин. Прибавка +5 кг, когда все подходы на верхней границе с запасом 1-2.";
        private const string BaseDumbbell = "База. Отдых 2-3 мин. Прибавка +2,5 кг на руку, когда все подходы на верхней границе с запасом 1-2.";
        private const string Isolation = "Изоляция. Отдых 60-90 сек. Диапазон 10-15 повторов, прибавка +2,5 кг.";

        public static IReadOnlyCollection<SeedExercise> Exercises { get; } =
        [
            new(SmithSquat, Base, MuscleGroup.Legs),
            new(DumbbellBenchPress, BaseDumbbell, MuscleGroup.Chest),
            new(LatPulldown, Base, MuscleGroup.Back),
            new(ReverseGripPulldown, Base, MuscleGroup.Back),
            new(LateralRaise, Isolation, MuscleGroup.Shoulders),
            new(RomanianDeadlift, "База. Отдых 2-3 мин. Техника важнее веса, вес не гнать.", MuscleGroup.Legs),
            new(LegPress, Base, MuscleGroup.Legs),
            new(InclineDumbbellPress, "База. Отдых 2-3 мин. Диапазон 6-8 повторов, прибавка +2,5 кг на руку.", MuscleGroup.Chest),
            new(SeatedRow, Base, MuscleGroup.Back),
            new(SeatedShoulderPress, BaseDumbbell, MuscleGroup.Shoulders),
            new(BarbellCurl, Isolation, MuscleGroup.Arms),
            new(RearDeltFly, "Изоляция. Отдых 60-90 сек. Лёгкий вес, чистая техника.", MuscleGroup.Shoulders),
            new(LyingLegCurl, Isolation, MuscleGroup.Legs),
            new(TricepsRopePushdown, Isolation, MuscleGroup.Arms),
            new(StandingCalfRaise, Isolation, MuscleGroup.Legs),
            new(Crunches, Isolation, MuscleGroup.Core),
            new(ChestFly, Isolation, MuscleGroup.Chest)
        ];
    }
}
