using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MauiTrainApp.Application.CQRS.Commands.AddPlannedExerciseSet;
using MauiTrainApp.Application.CQRS.Commands.CreateExercise;
using MauiTrainApp.Application.CQRS.Commands.CreateTrainingPlan;
using MauiTrainApp.Application.CQRS.Commands.MovePlannedExerciseSet;
using MauiTrainApp.Application.CQRS.Commands.RemovePlannedExerciseSet;
using MauiTrainApp.Application.CQRS.Commands.StartWorkoutFromPlan;
using MauiTrainApp.Application.CQRS.Queries.GetTrainingPlanDetails;
using MauiTrainApp.Application.CQRS.Queries.GetWorkoutDetails;
using MauiTrainApp.Application.DI;
using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Domain.Exceptions;
using MauiTrainApp.Infrastructure.DI;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MauiTrainApp.Tests.Application;

public class ExerciseOrderTests : IAsyncLifetime
{
    private static readonly DateOnly Day = new(2026, 9, 12);
    private static readonly string[] Names = ["Присед", "Жим", "Тяга", "Махи"];

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
    public async Task Plan_KeepsTheOrderExercisesWereAddedIn()
    {
        var sender = Sender();
        var planId = await CreatePlanAsync(sender);

        Assert.Equal(Names, await OrderAsync(sender, planId));
    }

    [Fact]
    public async Task Move_PutsAnExerciseEarlier()
    {
        var sender = Sender();
        var planId = await CreatePlanAsync(sender);
        var ids = await IdsAsync(sender, planId);

        await sender.SendAsync(new MovePlannedExerciseSetCommand(planId, ids[2], 0));

        Assert.Equal(["Тяга", "Присед", "Жим", "Махи"], await OrderAsync(sender, planId));
    }

    [Fact]
    public async Task Move_PutsAnExerciseLater()
    {
        var sender = Sender();
        var planId = await CreatePlanAsync(sender);
        var ids = await IdsAsync(sender, planId);

        await sender.SendAsync(new MovePlannedExerciseSetCommand(planId, ids[0], 3));

        Assert.Equal(["Жим", "Тяга", "Махи", "Присед"], await OrderAsync(sender, planId));
    }

    [Fact]
    public async Task Move_SurvivesAReload()
    {
        var sender = Sender();
        var planId = await CreatePlanAsync(sender);
        var ids = await IdsAsync(sender, planId);

        await sender.SendAsync(new MovePlannedExerciseSetCommand(planId, ids[3], 0));

        Assert.Equal(["Махи", "Присед", "Жим", "Тяга"], await OrderAsync(Sender(), planId));
    }

    [Fact]
    public async Task Move_RejectsAnIndexOutOfRange()
    {
        var sender = Sender();
        var planId = await CreatePlanAsync(sender);
        var ids = await IdsAsync(sender, planId);

        await Assert.ThrowsAsync<ValidationException>(
            () => sender.SendAsync(new MovePlannedExerciseSetCommand(planId, ids[0], 4)));
    }

    [Fact]
    public async Task Move_RejectsAnExerciseFromAnotherPlan()
    {
        var sender = Sender();
        var planId = await CreatePlanAsync(sender);
        var otherId = await CreatePlanAsync(sender, "Другой");
        var ids = await IdsAsync(sender, planId);

        await Assert.ThrowsAsync<ValidationException>(
            () => sender.SendAsync(new MovePlannedExerciseSetCommand(otherId, ids[0], 0)));
    }

    [Fact]
    public async Task AddedExercise_GoesToTheEnd()
    {
        var sender = Sender();
        var planId = await CreatePlanAsync(sender);

        var extra = await sender.SendAsync(new CreateExerciseCommand("Планка"));

        await sender.SendAsync(new AddPlannedExerciseSetCommand(planId, extra.ExerciseId, 3, 10, 0));

        Assert.Equal(["Присед", "Жим", "Тяга", "Махи", "Планка"], await OrderAsync(sender, planId));
    }

    [Fact]
    public async Task RemovedExercise_LeavesTheRestInOrder()
    {
        var sender = Sender();
        var planId = await CreatePlanAsync(sender);
        var ids = await IdsAsync(sender, planId);

        await sender.SendAsync(new RemovePlannedExerciseSetCommand(planId, ids[1]));

        Assert.Equal(["Присед", "Тяга", "Махи"], await OrderAsync(sender, planId));
    }

    [Fact]
    public async Task Workout_InheritsThePlanOrder()
    {
        var sender = Sender();
        var planId = await CreatePlanAsync(sender);
        var ids = await IdsAsync(sender, planId);

        await sender.SendAsync(new MovePlannedExerciseSetCommand(planId, ids[3], 0));

        var started = await sender.SendAsync(new StartWorkoutFromPlanCommand(planId, Day));

        var workout = (await sender.QueryAsync(new GetWorkoutDetailsQuery(started.WorkoutId))).Workout!;

        Assert.Equal(
            ["Махи", "Присед", "Жим", "Тяга"],
            workout.ExerciseSets.Select(x => x.ExerciseName.Split(' ').Last()));
    }

    private ISender Sender() => _provider.CreateScope().ServiceProvider.GetRequiredService<ISender>();

    private static async Task<Guid> CreatePlanAsync(ISender sender, string name = "A day")
    {
        var planned = new PlannedExerciseSet[Names.Length];

        for (var index = 0; index < Names.Length; index++)
        {
            var exercise = await sender.SendAsync(new CreateExerciseCommand($"{name} {Names[index]}"));

            planned[index] = new PlannedExerciseSet(exercise.ExerciseId, [new PlannedWorkingSet(10, 60)]);
        }

        var plan = await sender.SendAsync(new CreateTrainingPlanCommand(name, planned));

        return plan.TrainingPlanId;
    }

    private static async Task<string[]> OrderAsync(ISender sender, Guid planId)
    {
        var details = (await sender.QueryAsync(new GetTrainingPlanDetailsQuery(planId))).TrainingPlan!;

        return [.. details.ExerciseSets.Select(x => x.ExerciseName.Split(' ').Last())];
    }

    private static async Task<Guid[]> IdsAsync(ISender sender, Guid planId)
    {
        var details = (await sender.QueryAsync(new GetTrainingPlanDetailsQuery(planId))).TrainingPlan!;

        return [.. details.ExerciseSets.Select(x => x.Id)];
    }
}
