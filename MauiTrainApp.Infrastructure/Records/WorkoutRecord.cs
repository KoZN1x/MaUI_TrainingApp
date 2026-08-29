using MauiTrainApp.Infrastructure.Records.Base;

namespace MauiTrainApp.Infrastructure.Records
{
    internal class WorkoutRecord : BaseRecord
    {
        public DateOnly WorkoutDay { get; init; }

        public ICollection<ExerciseSetRecord> ExerciseSets { get; init; } = [];
    }
}
