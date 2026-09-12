using MauiTrainApp.Infrastructure.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MauiTrainApp.Infrastructure.Database.Configurations
{
    internal class TrainingPlanRecordConfiguration : IEntityTypeConfiguration<TrainingPlanRecord>
    {
        public void Configure(EntityTypeBuilder<TrainingPlanRecord> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired();

            builder.HasMany(x => x.ExerciseSets)
                .WithOne()
                .HasForeignKey(x => x.TrainingPlanRecordId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
