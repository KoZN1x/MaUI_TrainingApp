using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Infrastructure.Interfaces;
using MauiTrainApp.Infrastructure.Records;

namespace MauiTrainApp.Infrastructure.Mappers
{
    internal class ExerciseRecordMapper : IRecordMapper<Exercise, ExerciseRecord>
    {
        public Exercise ToDomain(ExerciseRecord record)
        {
            var exercise = new Exercise(record.Name, record.Description)
            {
                Id = record.Id,
                CreatedAt = record.CreatedAt
            };

            if (record.UpdatedAt is not null)
            {
                exercise.SetUpdatedTime(record.UpdatedAt);
            }

            return exercise;
        }

        public ExerciseRecord ToRecord(Exercise entity)
        {
            return new ExerciseRecord
            {
                Id = entity.Id,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                Name = entity.Name,
                Description = entity.Description,
            };
        }
    }
}
