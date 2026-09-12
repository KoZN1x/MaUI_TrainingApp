using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MauiTrainApp.Application.CQRS.Commands.AddPlannedExerciseSet;
using MauiTrainApp.Application.CQRS.Commands.CreateExercise;
using MauiTrainApp.Application.CQRS.Commands.CreateTrainingPlan;
using MauiTrainApp.Application.CQRS.Commands.DeleteExercise;
using MauiTrainApp.Application.CQRS.Commands.RemovePlannedExerciseSet;
using MauiTrainApp.Application.CQRS.Commands.RenameTrainingPlan;
using MauiTrainApp.Application.CQRS.Commands.UpdatePlannedExerciseSet;
using MauiTrainApp.Application.CQRS.Queries.GetExercises;
using MauiTrainApp.Application.CQRS.Commands.StartWorkoutFromPlan;
using MauiTrainApp.Application.CQRS.Queries.GetTrainingPlanDetails;
using MauiTrainApp.Application.CQRS.Queries.GetWorkoutDetails;
using MauiTrainApp.Application.DI;
using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Domain.Enums;
using MauiTrainApp.Domain.ReadModels;
using MauiTrainApp.Domain.Exceptions;
using MauiTrainApp.Infrastructure.DI;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MauiTrainApp.Tests.Application;

public class PlanEditingTests : IAsyncLifetime
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"mauitrain-{Guid.NewGuid():N}.db");
    private ServiceProvider _provider = null!;

    public Task InitializeAsync()
    {
        _provider = new ServiceCollection()
            .AddInfrastructure(_databasePath)
            .AddApplication()
            .BuildServiceProvider();

        return _provider.MigrateDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        await _provider.DisposeAsync();

        SqliteConnection.ClearAllPools();

        foreach (var file in Directory.GetFiles(Path.GetTempPath(), $"{Path.GetFileName(_databasePath)}*"))
        {
            File.Delete(file);
        }
    }

    [Fact]
    public async Task AddPlannedExerciseSet_CreatesTheRequestedNumberOfWorkingSets()
    {
        var sender = Sender();
        var planId = await CreatePlanAsync(sender);
        var pullUp = await sender.SendAsync(new CreateExerciseCommand("Pull up"));

        await sender.SendAsync(new AddPlannedExerciseSetCommand(planId, pullUp.ExerciseId, 4, 8, 35));

        var added = (await DetailsAsync(sender, planId)).ExerciseSets.Single(x => x.ExerciseName == "Pull up");

        Assert.Equal(4, added.WorkingSets.Count);
        Assert.All(added.WorkingSets, x => Assert.Equal(8, x.Reps));
        Assert.All(added.WorkingSets, x => Assert.Equal(35, x.Weight));
    }

    [Fact]
    public async Task AddPlannedExerciseSet_RejectsAnExerciseThatIsAlreadyInThePlan()
    {
        var sender = Sender();
        var planId = await CreatePlanAsync(sender);

        var squatId = (await DetailsAsync(sender, planId)).ExerciseSets.Single().ExerciseId;

        await Assert.ThrowsAsync<ValidationException>(
            () => sender.SendAsync(new AddPlannedExerciseSetCommand(planId, squatId)));
    }

    [Fact]
    public async Task UpdatePlannedExerciseSet_ReplacesEveryWorkingSet()
    {
        var sender = Sender();
        var planId = await CreatePlanAsync(sender);
        var exerciseSetId = (await DetailsAsync(sender, planId)).ExerciseSets.Single().Id;

        await sender.SendAsync(new UpdatePlannedExerciseSetCommand(planId, exerciseSetId, 5, 12, 72.5));

        var updated = (await DetailsAsync(sender, planId)).ExerciseSets.Single();

        Assert.Equal(5, updated.WorkingSets.Count);
        Assert.All(updated.WorkingSets, x => Assert.Equal(12, x.Reps));
        Assert.All(updated.WorkingSets, x => Assert.Equal(72.5, x.Weight));
    }

    [Fact]
    public async Task UpdatePlannedExerciseSet_RejectsAPlanWithoutWorkingSets()
    {
        var sender = Sender();
        var planId = await CreatePlanAsync(sender);
        var exerciseSetId = (await DetailsAsync(sender, planId)).ExerciseSets.Single().Id;

        await Assert.ThrowsAsync<ValidationException>(
            () => sender.SendAsync(new UpdatePlannedExerciseSetCommand(planId, exerciseSetId, 0, 10, 60)));
    }

    [Fact]
    public async Task RemovePlannedExerciseSet_LeavesTheOtherExercises()
    {
        var sender = Sender();
        var planId = await CreatePlanAsync(sender);
        var pullUp = await sender.SendAsync(new CreateExerciseCommand("Pull up"));

        await sender.SendAsync(new AddPlannedExerciseSetCommand(planId, pullUp.ExerciseId));

        var squatSetId = (await DetailsAsync(sender, planId)).ExerciseSets.Single(x => x.ExerciseName == "Squat").Id;

        var removed = await sender.SendAsync(new RemovePlannedExerciseSetCommand(planId, squatSetId));

        Assert.Equal(1, removed.RemainingExerciseSetCount);
        Assert.Equal("Pull up", (await DetailsAsync(sender, planId)).ExerciseSets.Single().ExerciseName);
    }

    [Fact]
    public async Task RenameTrainingPlan_RejectsAnEmptyName()
    {
        var sender = Sender();
        var planId = await CreatePlanAsync(sender);

        await Assert.ThrowsAsync<ValidationException>(
            () => sender.SendAsync(new RenameTrainingPlanCommand(planId, "   ")));
    }

    [Fact]
    public async Task CreateExercise_RejectsADuplicateName()
    {
        var sender = Sender();

        await sender.SendAsync(new CreateExerciseCommand("Squat", null, MuscleGroup.Legs));

        await Assert.ThrowsAsync<ValidationException>(
            () => sender.SendAsync(new CreateExerciseCommand("Squat")));
    }

    [Fact]
    public async Task CreateExercise_ShowsUpInTheExerciseListWithItsMuscleGroup()
    {
        var sender = Sender();

        await sender.SendAsync(new CreateExerciseCommand("Face pull", "На заднюю дельту", MuscleGroup.Shoulders));

        var exercise = (await sender.QueryAsync(new GetExercisesQuery())).Exercises
            .Single(x => x.Name == "Face pull");

        Assert.Equal(MuscleGroup.Shoulders, exercise.MuscleGroup);
        Assert.Equal("На заднюю дельту", exercise.Description);
        Assert.False(exercise.HasResult);
    }

    [Fact]
    public async Task ExerciseOrder_SurvivesEditingAndStartingAWorkout()
    {
        var sender = Sender();
        var planId = await CreatePlanAsync(sender);

        foreach (var name in new[] { "Pull up", "Press", "Curl" })
        {
            var exercise = await sender.SendAsync(new CreateExerciseCommand(name));

            await sender.SendAsync(new AddPlannedExerciseSetCommand(planId, exercise.ExerciseId));
        }

        string[] expected = ["Squat", "Pull up", "Press", "Curl"];

        Assert.Equal(expected, (await DetailsAsync(sender, planId)).ExerciseSets.Select(x => x.ExerciseName));
        Assert.Equal(expected, (await DetailsAsync(sender, planId)).ExerciseSets.Select(x => x.ExerciseName));

        var started = await sender.SendAsync(new StartWorkoutFromPlanCommand(planId, new DateOnly(2026, 9, 12)));
        var workout = (await sender.QueryAsync(new GetWorkoutDetailsQuery(started.WorkoutId))).Workout!;

        Assert.Equal(expected, workout.ExerciseSets.Select(x => x.ExerciseName));
    }

    [Fact]
    public async Task DeleteExercise_RemovesAnExerciseNobodyUses()
    {
        var sender = Sender();
        var exercise = await sender.SendAsync(new CreateExerciseCommand("Face pull"));

        await sender.SendAsync(new DeleteExerciseCommand(exercise.ExerciseId));

        Assert.DoesNotContain(
            (await sender.QueryAsync(new GetExercisesQuery())).Exercises,
            x => x.Name == "Face pull");
    }

    [Fact]
    public async Task DeleteExercise_IsRejectedWhileItIsPartOfAPlan()
    {
        var sender = Sender();
        var planId = await CreatePlanAsync(sender);
        var squatId = (await DetailsAsync(sender, planId)).ExerciseSets.Single().ExerciseId;

        await Assert.ThrowsAsync<ValidationException>(
            () => sender.SendAsync(new DeleteExerciseCommand(squatId)));

        Assert.Single((await DetailsAsync(sender, planId)).ExerciseSets);
    }

    [Fact]
    public async Task DeleteExercise_IsRejectedWhileItIsPartOfAPastWorkout()
    {
        var sender = Sender();
        var planId = await CreatePlanAsync(sender);
        var squatId = (await DetailsAsync(sender, planId)).ExerciseSets.Single().ExerciseId;

        await sender.SendAsync(new StartWorkoutFromPlanCommand(planId, new DateOnly(2026, 9, 12)));
        await sender.SendAsync(new RemovePlannedExerciseSetCommand(
            planId,
            (await DetailsAsync(sender, planId)).ExerciseSets.Single().Id));

        await Assert.ThrowsAsync<ValidationException>(
            () => sender.SendAsync(new DeleteExerciseCommand(squatId)));
    }

    private ISender Sender() => _provider.CreateScope().ServiceProvider.GetRequiredService<ISender>();

    private static async Task<TrainingPlanDetailsReadModel> DetailsAsync(
        ISender sender,
        Guid planId)
    {
        return (await sender.QueryAsync(new GetTrainingPlanDetailsQuery(planId))).TrainingPlan!;
    }

    private static async Task<Guid> CreatePlanAsync(ISender sender)
    {
        var exercise = await sender.SendAsync(new CreateExerciseCommand("Squat", null, MuscleGroup.Legs));

        var plan = await sender.SendAsync(new CreateTrainingPlanCommand(
            "Leg day",
            [new PlannedExerciseSet(exercise.ExerciseId, [new PlannedWorkingSet(10, 60)])]));

        return plan.TrainingPlanId;
    }
}
