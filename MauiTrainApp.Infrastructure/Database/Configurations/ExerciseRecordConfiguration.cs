using MauiTrainApp.Infrastructure.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MauiTrainApp.Infrastructure.Database.Configurations
{
    internal class ExerciseRecordConfiguration : IEntityTypeConfiguration<ExerciseRecord>
    {
        public void Configure(EntityTypeBuilder<ExerciseRecord> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired();
        }
    }
}
