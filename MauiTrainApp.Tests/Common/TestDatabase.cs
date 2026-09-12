using System;
using MauiTrainApp.Domain.Interfaces;
using MauiTrainApp.Infrastructure.Database;
using MauiTrainApp.Infrastructure.Mappers;
using MauiTrainApp.Infrastructure.Repositories;
using MauiTrainApp.Infrastructure.Repositories.Read;
using MauiTrainApp.Infrastructure.Repositories.Write;
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

        ExercisesWrite = new ExerciseWriteRepository(exerciseMapper, Context);
        WorkoutsWrite = new WorkoutWriteRepository(new WorkoutRecordMapper(exerciseSetMapper), Context);
        TrainingPlansWrite = new TrainingPlanWriteRepository(new TrainingPlanRecordMapper(exerciseSetMapper), Context);

        ExerciseReader = new ExerciseReadRepository(Context);
        WorkoutReader = new WorkoutReadRepository(Context);
        TrainingPlanReader = new TrainingPlanReadRepository(Context);
    }

    public MauiTrainAppDbContext Context { get; }

    public ExerciseWriteRepository ExercisesWrite { get; }

    public WorkoutWriteRepository WorkoutsWrite { get; }

    public TrainingPlanWriteRepository TrainingPlansWrite { get; }

    public IExerciseReadRepository ExerciseReader { get; }

    public IWorkoutReadRepository WorkoutReader { get; }

    public ITrainingPlanReadRepository TrainingPlanReader { get; }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
