using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Infrastructure.Interfaces;
using MauiTrainApp.Infrastructure.Records;

namespace MauiTrainApp.Infrastructure.Mappers
{
    internal class ExerciseSetRecordMapper : IChildRecordMapper<ExerciseSet, ExerciseSetRecord, Guid>
    {
        private readonly IRecordMapper<Exercise, ExerciseRecord> _exerciseRecordMapper;

        public ExerciseSetRecordMapper(IRecordMapper<Exercise, ExerciseRecord> exerciseRecordMapper)
        {
            _exerciseRecordMapper = exerciseRecordMapper;
        }

        public ExerciseSet ToDomain(ExerciseSetRecord record)
        {
            var exerciseSet = new ExerciseSet
            {
                Id = record.Id,
                CreatedAt = record.CreatedAt
            };

            exerciseSet.SetExercise(_exerciseRecordMapper.ToDomain(record.Exercise));
            exerciseSet.SetWorkingSets(record.WorkingSets);
            exerciseSet.SetUpdatedTime(record.UpdatedAt);

            return exerciseSet;
        }

        public ExerciseSetRecord ToRecord(ExerciseSet entity, Guid workoutId)
        {
            return new ExerciseSetRecord
            {
                Id = entity.Id,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                ExerciseRecordId = entity.Exercise.Id,
                WorkingSets = entity.WorkingSets.ToList(),
                WorkoutRecordId = workoutId
            };
        }
    }
}
