using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Domain.Interfaces;
using MauiTrainApp.Infrastructure.Database;
using MauiTrainApp.Infrastructure.Database.Seeding;
using MauiTrainApp.Infrastructure.Interfaces;
using MauiTrainApp.Infrastructure.Mappers;
using MauiTrainApp.Infrastructure.Records;
using MauiTrainApp.Infrastructure.Repositories;
using MauiTrainApp.Infrastructure.Repositories.Read;
using MauiTrainApp.Infrastructure.Repositories.Write;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MauiTrainApp.Infrastructure.DI
{
    public static class DependencyInjection
    {
        extension(IServiceCollection services)
        {
            public IServiceCollection AddInfrastructure(string databasePath)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);

                SQLitePCL.Batteries_V2.Init();

                services.AddDbContext<MauiTrainAppDbContext>(options => options.UseSqlite($"Data Source={databasePath}"));

                return services
                    .AddMappers()
                    .AddWriteRepositories()
                    .AddReadRepositories()
                    .AddScoped<DatabaseSeeder>();
            }

            private IServiceCollection AddMappers()
            {
                return services
                    .AddSingleton<IRecordMapper<Exercise, ExerciseRecord>, ExerciseRecordMapper>()
                    .AddSingleton<IChildRecordMapper<ExerciseSet, ExerciseSetRecord, ExerciseSetParent>, ExerciseSetRecordMapper>()
                    .AddSingleton<IRecordMapper<Workout, WorkoutRecord>, WorkoutRecordMapper>()
                    .AddSingleton<IRecordMapper<TrainingPlan, TrainingPlanRecord>, TrainingPlanRecordMapper>();
            }

            private IServiceCollection AddWriteRepositories()
            {
                return services
                    .AddScoped<IWriteRepository<Exercise>, ExerciseWriteRepository>()
                    .AddScoped<IWriteRepository<Workout>, WorkoutWriteRepository>()
                    .AddScoped<IWriteRepository<TrainingPlan>, TrainingPlanWriteRepository>();
            }

            private IServiceCollection AddReadRepositories()
            {
                return services
                    .AddScoped<IExerciseReadRepository, ExerciseReadRepository>()
                    .AddScoped<ITrainingPlanReadRepository, TrainingPlanReadRepository>()
                    .AddScoped<IWorkoutReadRepository, WorkoutReadRepository>()
                    .AddScoped<IProgressReadRepository, ProgressReadRepository>();
            }
        }
    }
}
