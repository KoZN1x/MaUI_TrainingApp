using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MauiTrainApp.Application.CQRS.Commands.CreateExercise;
using MauiTrainApp.Application.CQRS.Commands.CreateTrainingPlan;
using MauiTrainApp.Application.CQRS.Commands.DeleteWorkout;
using MauiTrainApp.Application.CQRS.Commands.StartWorkoutFromPlan;
using MauiTrainApp.Application.DI;
using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Core.DI;
using MauiTrainApp.Domain.Exceptions;
using MauiTrainApp.Infrastructure.DI;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MauiTrainApp.Tests.Application;

public class ExceptionHandlingTests : IAsyncLifetime
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"mauitrain-{Guid.NewGuid():N}.db");
    private ServiceProvider _provider = null!;

    public Task InitializeAsync()
    {
        _provider = new ServiceCollection()
            .AddInfrastructure(_databasePath)
            .AddApplication()
            .AddCommand<BoomCommand, BoomResult, BoomCommandHandler>()
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
    public async Task BrokenInvariant_BecomesValidationException()
    {
        var sender = Sender();

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => sender.SendAsync(new CreateExerciseCommand("   ")));

        Assert.Equal("Exercise name couldn't be empty", exception.UserMessage);
    }

    [Fact]
    public async Task DuplicateName_BecomesValidationException()
    {
        var sender = Sender();

        await sender.SendAsync(new CreateExerciseCommand("Squat"));

        await Assert.ThrowsAsync<ValidationException>(
            () => sender.SendAsync(new CreateExerciseCommand("Squat")));
    }

    [Fact]
    public async Task MissingAggregate_BecomesNotFoundException()
    {
        var sender = Sender();
        var missingId = Guid.NewGuid();

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => sender.SendAsync(new StartWorkoutFromPlanCommand(missingId, new DateOnly(2026, 9, 12))));

        Assert.Contains(missingId.ToString(), exception.Message);

        await Assert.ThrowsAsync<NotFoundException>(
            () => sender.SendAsync(new DeleteWorkoutCommand(Guid.NewGuid())));
    }

    [Fact]
    public async Task MissingExerciseInPlan_BecomesNotFoundException()
    {
        var sender = Sender();

        await Assert.ThrowsAsync<NotFoundException>(
            () => sender.SendAsync(new CreateTrainingPlanCommand(
                "Broken",
                [new PlannedExerciseSet(Guid.NewGuid(), [new PlannedWorkingSet(5, 100)])])));
    }

    [Fact]
    public async Task UnknownFailure_BecomesUnexpectedExceptionKeepingCause()
    {
        var sender = Sender();

        var exception = await Assert.ThrowsAsync<UnexpectedException>(
            () => sender.SendAsync(new BoomCommand()));

        Assert.IsType<InvalidTimeZoneException>(exception.InnerException);
        Assert.Equal("Something went wrong. Please try again.", exception.UserMessage);
        Assert.Equal("boom", exception.Message);
    }

    [Fact]
    public async Task Cancellation_IsNotTranslated()
    {
        var sender = Sender();

        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => sender.SendAsync(new BoomCommand(), cancellation.Token));
    }

    private ISender Sender() => _provider.CreateScope().ServiceProvider.GetRequiredService<ISender>();

    private sealed record BoomCommand : ICommand<BoomResult>;

    private sealed record BoomResult;

    private sealed class BoomCommandHandler : ICommandHandler<BoomCommand, BoomResult>
    {
        public Task<BoomResult> HandleAsync(BoomCommand command, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            throw new InvalidTimeZoneException("boom");
        }
    }
}
