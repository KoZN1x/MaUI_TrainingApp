using System.Text.Json;
using MauiTrainApp.Domain.ValueObjects;
using MauiTrainApp.Infrastructure.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MauiTrainApp.Infrastructure.Database.Configurations
{
    internal class ExerciseSetRecordConfiguration : IEntityTypeConfiguration<ExerciseSetRecord>
    {
        private static readonly JsonSerializerOptions SerializerOptions = JsonSerializerOptions.Default;

        public void Configure(EntityTypeBuilder<ExerciseSetRecord> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.WorkingSets)
                .HasColumnType("TEXT")
                .HasConversion(
                    x => JsonSerializer.Serialize(x, SerializerOptions),
                    x => JsonSerializer.Deserialize<List<WorkingSet>>(x, SerializerOptions) ?? new List<WorkingSet>(),
                    new ValueComparer<ICollection<WorkingSet>>(
                        (left, right) => left!.SequenceEqual(right!),
                        x => x.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode())),
                        x => x.ToList()));

            builder.HasOne(x => x.Exercise)
                .WithMany()
                .HasForeignKey(x => x.ExerciseRecordId)
                .IsRequired();
        }
    }
}
