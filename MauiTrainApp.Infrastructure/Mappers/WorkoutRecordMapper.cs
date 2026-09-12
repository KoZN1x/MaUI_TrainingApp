using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Infrastructure.Interfaces;
using MauiTrainApp.Infrastructure.Records;

namespace MauiTrainApp.Infrastructure.Mappers
{
    internal class WorkoutRecordMapper : IRecordMapper<Workout, WorkoutRecord>
    {
        private readonly IChildRecordMapper<ExerciseSet, ExerciseSetRecord, ExerciseSetParent> _exerciseSetRecordMapper;

        public WorkoutRecordMapper(
            IChildRecordMapper<ExerciseSet, ExerciseSetRecord, ExerciseSetParent> exerciseSetRecordMapper)
        {
            _exerciseSetRecordMapper = exerciseSetRecordMapper;
        }

        public Workout ToDomain(WorkoutRecord record)
        {
            var workout = new Workout(
                record.WorkoutDay,
                record.ExerciseSets.Select(_exerciseSetRecordMapper.ToDomain),
                record.TrainingPlanRecordId,
                record.DurationSeconds is null ? null : TimeSpan.FromSeconds(record.DurationSeconds.Value),
                record.StartedAt)
            {
                Id = record.Id,
                CreatedAt = record.CreatedAt
            };

            if (record.UpdatedAt is not null)
            {
                workout.SetUpdatedTime(record.UpdatedAt);
            }

            return workout;
        }

        public WorkoutRecord ToRecord(Workout entity)
        {
            var parent = ExerciseSetParent.ForWorkout(entity.Id);

            return new WorkoutRecord
            {
                Id = entity.Id,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                WorkoutDay = entity.WorkoutDay,
                TrainingPlanRecordId = entity.TrainingPlanId,
                DurationSeconds = entity.Duration is null ? null : (int)entity.Duration.Value.TotalSeconds,
                StartedAt = entity.StartedAt,
                ExerciseSets = entity.ExerciseSets
                    .Select(x => _exerciseSetRecordMapper.ToRecord(x, parent))
                    .ToList()
            };
        }
    }
}
