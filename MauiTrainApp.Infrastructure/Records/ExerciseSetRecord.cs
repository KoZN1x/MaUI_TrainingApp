using MauiTrainApp.Domain.ValueObjects;
using MauiTrainApp.Infrastructure.Records.Base;

namespace MauiTrainApp.Infrastructure.Records
{
    internal class ExerciseSetRecord : BaseRecord
    {
        public Guid WorkoutRecordId { get; init; }
        public Guid ExerciseRecordId { get; init; }

        public ExerciseRecord Exercise { get; init; } = null!;

        public ICollection<WorkingSet> WorkingSets { get; init; } = [];
    }
}
