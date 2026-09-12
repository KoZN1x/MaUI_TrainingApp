using MauiTrainApp.Infrastructure.Database;
using MauiTrainApp.Infrastructure.Mappers;
using MauiTrainApp.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MauiTrainApp.Tests.Common;

internal sealed class TestDatabase : IDisposable
{
    private readonly SqliteConnection _connection;

    public TestDatabase()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<MauiTrainAppDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new MauiTrainAppDbContext(options);
        Context.Database.EnsureCreated();

        var exerciseMapper = new ExerciseRecordMapper();
        var exerciseSetMapper = new ExerciseSetRecordMapper(exerciseMapper);

        Exercises = new ExerciseRepository(exerciseMapper, Context);
        Workouts = new WorkoutRepository(new WorkoutRecordMapper(exerciseSetMapper), Context);
        TrainingPlans = new TrainingPlanRepository(new TrainingPlanRecordMapper(exerciseSetMapper), Context);
    }

    public MauiTrainAppDbContext Context { get; }

    public ExerciseRepository Exercises { get; }

    public WorkoutRepository Workouts { get; }

    public TrainingPlanRepository TrainingPlans { get; }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
