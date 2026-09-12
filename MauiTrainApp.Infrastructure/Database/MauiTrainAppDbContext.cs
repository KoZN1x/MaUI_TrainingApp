using MauiTrainApp.Infrastructure.Records;
using Microsoft.EntityFrameworkCore;

namespace MauiTrainApp.Infrastructure.Database
{
    internal class MauiTrainAppDbContext : DbContext
    {
        public MauiTrainAppDbContext(DbContextOptions<MauiTrainAppDbContext> options)
            : base(options)
        {
        }

        public DbSet<ExerciseRecord> Exercises { get; set; }
        public DbSet<WorkoutRecord> Workouts { get; set; }
        public DbSet<TrainingPlanRecord> TrainingPlans { get; set; }
        public DbSet<ExerciseSetRecord> ExerciseSets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MauiTrainAppDbContext).Assembly);
        }
    }
}
