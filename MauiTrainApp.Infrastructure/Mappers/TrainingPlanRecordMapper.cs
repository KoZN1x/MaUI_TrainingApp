using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Infrastructure.Interfaces;
using MauiTrainApp.Infrastructure.Records;

namespace MauiTrainApp.Infrastructure.Mappers
{
    internal class TrainingPlanRecordMapper : IRecordMapper<TrainingPlan, TrainingPlanRecord>
    {
        private readonly IChildRecordMapper<ExerciseSet, ExerciseSetRecord, ExerciseSetParent> _exerciseSetRecordMapper;

        public TrainingPlanRecordMapper(
            IChildRecordMapper<ExerciseSet, ExerciseSetRecord, ExerciseSetParent> exerciseSetRecordMapper)
        {
            _exerciseSetRecordMapper = exerciseSetRecordMapper;
        }

        public TrainingPlan ToDomain(TrainingPlanRecord record)
        {
            var trainingPlan = new TrainingPlan(
                record.Name,
                record.ExerciseSets.Select(_exerciseSetRecordMapper.ToDomain))
            {
                Id = record.Id,
                CreatedAt = record.CreatedAt
            };

            if (record.UpdatedAt is not null)
            {
                trainingPlan.SetUpdatedTime(record.UpdatedAt);
            }

            return trainingPlan;
        }

        public TrainingPlanRecord ToRecord(TrainingPlan entity)
        {
            var parent = ExerciseSetParent.ForTrainingPlan(entity.Id);

            return new TrainingPlanRecord
            {
                Id = entity.Id,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                Name = entity.Name,
                ExerciseSets = entity.ExerciseSets
                    .Select(x => _exerciseSetRecordMapper.ToRecord(x, parent))
                    .ToList()
            };
        }
    }
}
