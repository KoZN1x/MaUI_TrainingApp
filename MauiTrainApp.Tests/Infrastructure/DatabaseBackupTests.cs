using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MauiTrainApp.Application.CQRS.Commands.CreateExercise;
using MauiTrainApp.Application.CQRS.Queries.GetExercises;
using MauiTrainApp.Application.DI;
using MauiTrainApp.Core.CQRS.Interfaces;
using MauiTrainApp.Infrastructure.DI;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MauiTrainApp.Tests.Infrastructure;

public class DatabaseBackupTests : IAsyncLifetime
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"mauitrain-backup-{Guid.NewGuid():N}");

    private string _databasePath = null!;
    private ServiceProvider _provider = null!;

    public Task InitializeAsync()
    {
        Directory.CreateDirectory(_root);

        _databasePath = Path.Combine(_root, "mauitrain.db");

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

        Directory.Delete(_root, recursive: true);
    }

    [Fact]
    public async Task Backup_CopiesTheDataIntoAReadableDatabase()
    {
        await Sender().SendAsync(new CreateExerciseCommand("Присед"));

        var copy = await _provider.BackupDatabaseAsync(Path.Combine(_root, "out"));

        Assert.True(File.Exists(copy));

        await using var restored = new ServiceCollection()
            .AddInfrastructure(copy)
            .AddApplication()
            .BuildServiceProvider();

        var exercises = (await restored.CreateScope().ServiceProvider
            .GetRequiredService<ISender>()
            .QueryAsync(new GetExercisesQuery()))
            .Exercises;

        Assert.Contains(exercises, x => x.Name == "Присед");
    }

    [Fact]
    public async Task Backup_LeavesTheOriginalUsable()
    {
        await Sender().SendAsync(new CreateExerciseCommand("Присед"));

        await _provider.BackupDatabaseAsync(Path.Combine(_root, "out"));

        await Sender().SendAsync(new CreateExerciseCommand("Жим"));

        var exercises = (await Sender().QueryAsync(new GetExercisesQuery())).Exercises;

        Assert.Contains(exercises, x => x.Name == "Жим");
    }

    [Fact]
    public async Task Backup_CreatesTheTargetDirectory()
    {
        var target = Path.Combine(_root, "deep", "nested");

        var copy = await _provider.BackupDatabaseAsync(target);

        Assert.Equal(target, Path.GetDirectoryName(copy));
    }

    [Fact]
    public async Task Backup_RejectsAnEmptyTarget()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _provider.BackupDatabaseAsync("  "));
    }

    private ISender Sender() => _provider.CreateScope().ServiceProvider.GetRequiredService<ISender>();
}
