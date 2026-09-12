namespace MauiTrainApp.Domain.ValueObjects
{
    public record struct WorkingSet
    {
        public required RepScheme RepScheme { get; init; }

        public bool IsCompleted { get; init; }

        public WorkingSet Complete() => this with { IsCompleted = true };

        public WorkingSet Reset() => this with { IsCompleted = false };
    }
}
