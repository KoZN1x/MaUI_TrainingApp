using System;
using System.Linq;
using System.Threading.Tasks;
using MauiTrainApp.Domain.Enums;
using MauiTrainApp.Infrastructure.Database.Seeding;
using MauiTrainApp.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MauiTrainApp.Tests.Infrastructure;

public class DatabaseSeederTests : IDisposable
{
    private readonly TestDatabase _database = new();

    public void Dispose() => _database.Dispose();

    [Fact]
    public async Task Seed_CreatesCatalogAndPlans()
    {
        await SeedAsync();

        var exercises = await _database.ExerciseReader.GetAllAsync();
        var plans = await _database.TrainingPlanReader.GetAllAsync();

        Assert.Equal(SeedCatalog.Exercises.Count, exercises.Count);
        Assert.Equal(SeedPlans.All.Count, plans.Count);
    }

    [Fact]
    public async Task Seed_FillsThursdayFullBodyPlanAsPlanned()
    {
        await SeedAsync();

        var plans = await _database.TrainingPlanReader.GetAllAsync();
        var thursday = plans.Single(x => x.Name.StartsWith("Фуллбади · Четверг"));
        var details = await _database.TrainingPlanReader.GetByIdAsync(thursday.Id);

        var squat = details!.ExerciseSets.Single(x => x.ExerciseName == SeedCatalog.SmithSquat);

        Assert.Equal(4, details.ExerciseSets.Count);
        Assert.Equal(3, squat.WorkingSets.Count);
        Assert.All(squat.WorkingSets, x => Assert.Equal(60, x.Weight));
        Assert.All(squat.WorkingSets, x => Assert.Equal(10, x.Reps));
        Assert.All(details.ExerciseSets, x => Assert.False(x.IsCompleted));
    }

    [Fact]
    public async Task Seed_SharesOneExerciseAcrossPlans()
    {
        await SeedAsync();

        var plans = await _database.TrainingPlanReader.GetAllAsync();
        var fullBody = await _database.TrainingPlanReader.GetByIdAsync(
            plans.Single(x => x.Name.StartsWith("Фуллбади · Четверг")).Id);
        var split = await _database.TrainingPlanReader.GetByIdAsync(
            plans.Single(x => x.Name.StartsWith("Сплит · День А")).Id);

        var fromFullBody = fullBody!.ExerciseSets.Single(x => x.ExerciseName == SeedCatalog.SmithSquat);
        var fromSplit = split!.ExerciseSets.Single(x => x.ExerciseName == SeedCatalog.SmithSquat);

        Assert.Equal(fromFullBody.ExerciseId, fromSplit.ExerciseId);
        Assert.NotEqual(fromFullBody.Id, fromSplit.Id);
    }

    [Fact]
    public async Task Seed_IsIdempotent()
    {
        await SeedAsync();
        await SeedAsync();

        Assert.Equal(SeedCatalog.Exercises.Count, await _database.Context.Exercises.CountAsync());
        Assert.Equal(SeedPlans.All.Count, await _database.Context.TrainingPlans.CountAsync());
    }

    [Fact]
    public async Task SeededPlan_CanStartWorkout()
    {
        await SeedAsync();

        var plans = await _database.TrainingPlanReader.GetAllAsync();
        var sunday = await _database.TrainingPlansWrite.GetByIdAsync(
            plans.Single(x => x.Name.StartsWith("Фуллбади · Воскресенье")).Id);

        var workout = sunday!.StartWorkout(new DateOnly(2026, 9, 13));
        await _database.WorkoutsWrite.AddAsync(workout);

        var stored = await _database.WorkoutReader.GetByIdAsync(workout.Id);

        Assert.Equal(7, stored!.ExerciseSets.Count);
        Assert.Equal(18, stored.ExerciseSets.Sum(x => x.WorkingSets.Count));
        Assert.StartsWith("Фуллбади · Воскресенье", stored.TrainingPlanName);
    }

    [Fact]
    public async Task Seed_TagsEveryExerciseWithItsMuscleGroup()
    {
        await SeedAsync();

        var exercises = await _database.ExerciseReader.GetAllAsync();

        foreach (var seed in SeedCatalog.Exercises)
        {
            Assert.Equal(seed.MuscleGroup, exercises.Single(x => x.Name == seed.Name).MuscleGroup);
        }
    }

    [Fact]
    public async Task Seed_BackfillsMuscleGroupsOfExercisesStoredBeforeTheyExisted()
    {
        await SeedAsync();

        await _database.Context.Exercises.ExecuteUpdateAsync(
            x => x.SetProperty(exercise => exercise.MuscleGroup, MuscleGroup.Other));

        _database.Context.ChangeTracker.Clear();

        await SeedAsync();

        var exercises = await _database.ExerciseReader.GetAllAsync();
        var squat = exercises.Single(x => x.Name == SeedCatalog.SmithSquat);

        Assert.Equal(MuscleGroup.Legs, squat.MuscleGroup);
        Assert.Equal(SeedCatalog.Exercises.Count, exercises.Count);
    }

    private Task SeedAsync()
    {
        var seeder = new DatabaseSeeder(
            _database.Context,
            _database.ExercisesWrite,
            _database.TrainingPlansWrite);

        return seeder.SeedAsync();
    }
}
