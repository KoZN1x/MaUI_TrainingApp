using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Domain.Interfaces;
using MauiTrainApp.Infrastructure.Database;
using MauiTrainApp.Infrastructure.Mappers;
using MauiTrainApp.Infrastructure.Interfaces;
using MauiTrainApp.Infrastructure.Records;
using MauiTrainApp.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MauiTrainApp.Infrastructure.DI
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string databasePath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);
            
            SQLitePCL.Batteries_V2.Init();

            services.AddDbContext<MauiTrainAppDbContext>(options => options.UseSqlite($"Data Source={databasePath}"));

            return services
                .AddMappers()
                .AddRepositories();
        }

        private static IServiceCollection AddMappers(this IServiceCollection services)
        {
            return services
                .AddSingleton<IRecordMapper<Exercise, ExerciseRecord>, ExerciseRecordMapper>()
                .AddSingleton<IChildRecordMapper<ExerciseSet, ExerciseSetRecord, ExerciseSetParent>, ExerciseSetRecordMapper>()
                .AddSingleton<IRecordMapper<Workout, WorkoutRecord>, WorkoutRecordMapper>()
                .AddSingleton<IRecordMapper<TrainingPlan, TrainingPlanRecord>, TrainingPlanRecordMapper>();
        }

        private static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IExerciseRepository, ExerciseRepository>();
            services.AddScoped<IWorkoutRepository, WorkoutRepository>();
            services.AddScoped<ITrainingPlanRepository, TrainingPlanRepository>();

            services.AddScoped<IRepository<Exercise>>(x => x.GetRequiredService<IExerciseRepository>());
            services.AddScoped<IRepository<Workout>>(x => x.GetRequiredService<IWorkoutRepository>());
            services.AddScoped<IRepository<TrainingPlan>>(x => x.GetRequiredService<ITrainingPlanRepository>());

            return services;
        }
    }
}
