namespace MauiTrainApp.Infrastructure.Interfaces
{
    internal interface IDbRecord
    {
        Guid Id { get; }
        DateTimeOffset CreatedAt { get; }
        DateTimeOffset? UpdatedAt { get; }
    }
}
