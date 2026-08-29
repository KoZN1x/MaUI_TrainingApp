using MauiTrainApp.Infrastructure.Records.Base;

namespace MauiTrainApp.Infrastructure.Records
{
    internal class ExerciseRecord : BaseRecord
    {
        public string Name { get; init; }
        public string? Description { get; init; }
    }
}
