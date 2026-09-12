using MauiTrainApp.Infrastructure.Records.Base;

namespace MauiTrainApp.Infrastructure.Records
{
    internal record TrainingPlanRecord : BaseRecord
    {
        public required string Name { get; init; }

        public int ScheduleMask { get; init; }

        public ICollection<ExerciseSetRecord> ExerciseSets { get; init; } = [];
    }
}
