using MauiTrainApp.Infrastructure.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MauiTrainApp.Infrastructure.Database.Configurations
{
    internal class WorkoutRecordConfiguration : IEntityTypeConfiguration<WorkoutRecord>
    {
        public void Configure(EntityTypeBuilder<WorkoutRecord> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasMany(x => x.ExerciseSets)
              .WithOne()
              .HasForeignKey(x => x.WorkoutRecordId)
              .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<TrainingPlanRecord>()
              .WithMany()
              .HasForeignKey(x => x.TrainingPlanRecordId)
              .OnDelete(DeleteBehavior.SetNull);

            builder.Property(x => x.WorkoutDay)
                   .IsRequired();
        }
    }
}
