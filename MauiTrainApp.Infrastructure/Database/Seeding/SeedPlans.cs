namespace MauiTrainApp.Infrastructure.Database.Seeding
{
    internal static class SeedPlans
    {
        public static IReadOnlyCollection<SeedPlan> All { get; } =
        [
            new("Фуллбади · Четверг (40 мин)",
            [
                new(SeedCatalog.SmithSquat, Sets: 3, Reps: 10, Weight: 60),
                new(SeedCatalog.DumbbellBenchPress, Sets: 3, Reps: 10, Weight: 16),
                new(SeedCatalog.LatPulldown, Sets: 3, Reps: 10, Weight: 50),
                new(SeedCatalog.LateralRaise, Sets: 2, Reps: 15, Weight: 8)
            ]),
            new("Фуллбади · Воскресенье (70 мин)",
            [
                new(SeedCatalog.RomanianDeadlift, Sets: 3, Reps: 10, Weight: 50),
                new(SeedCatalog.LegPress, Sets: 3, Reps: 10, Weight: 80),
                new(SeedCatalog.InclineDumbbellPress, Sets: 3, Reps: 8, Weight: 20),
                new(SeedCatalog.SeatedRow, Sets: 3, Reps: 10, Weight: 45),
                new(SeedCatalog.SeatedShoulderPress, Sets: 2, Reps: 10, Weight: 15),
                new(SeedCatalog.BarbellCurl, Sets: 2, Reps: 12, Weight: 25),
                new(SeedCatalog.RearDeltFly, Sets: 2, Reps: 15, Weight: 6)
            ]),
            new("Сплит · День А · Вторник",
            [
                new(SeedCatalog.SmithSquat, Sets: 3, Reps: 10, Weight: 60),
                new(SeedCatalog.DumbbellBenchPress, Sets: 3, Reps: 10, Weight: 16),
                new(SeedCatalog.SeatedShoulderPress, Sets: 3, Reps: 10, Weight: 15),
                new(SeedCatalog.LatPulldown, Sets: 3, Reps: 10, Weight: 50),
                new(SeedCatalog.LateralRaise, Sets: 2, Reps: 15, Weight: 8)
            ]),
            new("Сплит · День Б · Четверг",
            [
                new(SeedCatalog.RomanianDeadlift, Sets: 3, Reps: 10, Weight: 50),
                new(SeedCatalog.SeatedRow, Sets: 3, Reps: 10, Weight: 45),
                new(SeedCatalog.InclineDumbbellPress, Sets: 3, Reps: 8, Weight: 20),
                new(SeedCatalog.BarbellCurl, Sets: 2, Reps: 12, Weight: 25),
                new(SeedCatalog.RearDeltFly, Sets: 2, Reps: 15, Weight: 6)
            ]),
            new("Сплит · День В · Суббота",
            [
                new(SeedCatalog.LegPress, Sets: 3, Reps: 10, Weight: 80),
                new(SeedCatalog.ReverseGripPulldown, Sets: 3, Reps: 10, Weight: 45),
                new(SeedCatalog.SeatedShoulderPress, Sets: 3, Reps: 10, Weight: 10),
                new(SeedCatalog.LyingLegCurl, Sets: 3, Reps: 15, Weight: 50),
                new(SeedCatalog.TricepsRopePushdown, Sets: 2, Reps: 15, Weight: 28)
            ])
        ];
    }
}
