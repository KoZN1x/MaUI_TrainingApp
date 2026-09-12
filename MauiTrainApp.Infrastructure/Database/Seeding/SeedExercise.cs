using MauiTrainApp.Domain.Enums;

namespace MauiTrainApp.Infrastructure.Database.Seeding
{
    internal sealed record SeedExercise(string Name, string? Description, MuscleGroup MuscleGroup);
}
