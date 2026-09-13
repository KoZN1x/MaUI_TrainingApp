using MauiTrainApp.Domain.ValueObjects;
using MauiTrainApp.Infrastructure.Records.Base;

namespace MauiTrainApp.Infrastructure.Records
{
    internal record ExerciseSetRecord : BaseRecord
    {
        public Guid? WorkoutRecordId { get; init; }
        public Guid? TrainingPlanRecordId { get; init; }
        public Guid ExerciseRecordId { get; init; }

        public int Position { get; init; }

        public ExerciseRecord Exercise { get; init; } = null!;

        public ICollection<WorkingSet> WorkingSets { get; init; } = [];
    }
}
