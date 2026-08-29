namespace MauiTrainApp.Domain.ValueObjects
{
    public record struct WorkingSet
    {
        public required RepScheme RepScheme { get; init; }
    }
}