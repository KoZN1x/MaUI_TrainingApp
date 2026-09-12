using MauiTrainApp.Domain.Enums;
using MauiTrainApp.Infrastructure.Records.Base;

namespace MauiTrainApp.Infrastructure.Records
{
    internal record ExerciseRecord : BaseRecord
    {
        public required string Name { get; init; }
        public string? Description { get; init; }
        public MuscleGroup MuscleGroup { get; init; }
    }
}
