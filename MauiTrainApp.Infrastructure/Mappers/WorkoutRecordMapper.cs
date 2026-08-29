using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Infrastructure.Interfaces;
using MauiTrainApp.Infrastructure.Records;

namespace MauiTrainApp.Infrastructure.Mappers
{
    internal class WorkoutRecordMapper : IRecordMapper<Workout, WorkoutRecord>
    {
        private readonly IChildRecordMapper<ExerciseSet, ExerciseSetRecord, Guid> _exerciseSetRecordMapper;

        public WorkoutRecordMapper(IChildRecordMapper<ExerciseSet, ExerciseSetRecord, Guid> exerciseSetRecordMapper)
        {
            _exerciseSetRecordMapper = exerciseSetRecordMapper;
        }

        public Workout ToDomain(WorkoutRecord record)
        {
            var workout = new Workout
            {
                Id = record.Id,
                CreatedAt = record.CreatedAt
            };

            workout.SetWorkoutDay(record.WorkoutDay);
            workout.SetExerciseSets(record.ExerciseSets
                .Select(_exerciseSetRecordMapper.ToDomain)
                .ToList() ?? []);
            workout.SetUpdatedTime(record.UpdatedAt);

            return workout;
        }

        public WorkoutRecord ToRecord(Workout entity)
        {
            return new WorkoutRecord
            {
                Id = entity.Id,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                WorkoutDay = entity.WorkoutDay,
                ExerciseSets = entity.ExerciseSets
                .Select(x => _exerciseSetRecordMapper.ToRecord(x, entity.Id))
                .ToList() ?? []
            };
        }
    }
}
