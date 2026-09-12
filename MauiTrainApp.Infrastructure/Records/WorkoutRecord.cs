using MauiTrainApp.Infrastructure.Records.Base;

namespace MauiTrainApp.Infrastructure.Records
{
    internal record WorkoutRecord : BaseRecord
    {
        public DateOnly WorkoutDay { get; init; }

        public Guid? TrainingPlanRecordId { get; init; }

        public TrainingPlanRecord? TrainingPlan { get; init; }

        public int? DurationSeconds { get; init; }

        public DateTimeOffset StartedAt { get; init; }

        public ICollection<ExerciseSetRecord> ExerciseSets { get; init; } = [];
    }
}
