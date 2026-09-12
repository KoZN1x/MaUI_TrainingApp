using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Infrastructure.Interfaces;
using MauiTrainApp.Infrastructure.Records;

namespace MauiTrainApp.Infrastructure.Mappers
{
    internal class ExerciseSetRecordMapper : IChildRecordMapper<ExerciseSet, ExerciseSetRecord, ExerciseSetParent>
    {
        private readonly IRecordMapper<Exercise, ExerciseRecord> _exerciseRecordMapper;

        public ExerciseSetRecordMapper(IRecordMapper<Exercise, ExerciseRecord> exerciseRecordMapper)
        {
            _exerciseRecordMapper = exerciseRecordMapper;
        }

        public ExerciseSet ToDomain(ExerciseSetRecord record)
        {
            var exerciseSet = new ExerciseSet(
                _exerciseRecordMapper.ToDomain(record.Exercise),
                record.WorkingSets)
            {
                Id = record.Id,
                CreatedAt = record.CreatedAt
            };

            if (record.UpdatedAt is not null)
            {
                exerciseSet.SetUpdatedTime(record.UpdatedAt);
            }

            return exerciseSet;
        }

        public ExerciseSetRecord ToRecord(ExerciseSet entity, ExerciseSetParent parent)
        {
            return new ExerciseSetRecord
            {
                Id = entity.Id,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                ExerciseRecordId = entity.Exercise.Id,
                WorkingSets = entity.WorkingSets.ToList(),
                WorkoutRecordId = parent.WorkoutId,
                TrainingPlanRecordId = parent.TrainingPlanId
            };
        }
    }
}
