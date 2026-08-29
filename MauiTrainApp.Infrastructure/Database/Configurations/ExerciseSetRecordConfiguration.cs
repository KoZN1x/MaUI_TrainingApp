using MauiTrainApp.Infrastructure.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MauiTrainApp.Infrastructure.Database.Configurations
{
    internal class ExerciseSetRecordConfiguration : IEntityTypeConfiguration<ExerciseSetRecord>
    {
        public void Configure(EntityTypeBuilder<ExerciseSetRecord> builder)
        {
            builder.HasKey(x => x.Id);

            builder.ComplexCollection(x => x.WorkingSets, b =>
            {
                b.ToJson();
            });

            builder.HasOne(x => x.Exercise)
                .WithMany()
                .HasForeignKey(x => x.ExerciseRecordId)
                .IsRequired();
        }
    }
}
