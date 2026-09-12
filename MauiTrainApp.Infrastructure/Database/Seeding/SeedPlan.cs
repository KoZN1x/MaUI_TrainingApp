namespace MauiTrainApp.Infrastructure.Database.Seeding
{
    internal sealed record SeedPlan(string Name, IReadOnlyCollection<SeedWorkingSet> ExerciseSets);
}
