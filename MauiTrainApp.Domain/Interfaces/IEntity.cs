namespace MauiTrainApp.Domain.Interfaces
{
    public interface IEntity
    {
        Guid Id { get; }
        DateTimeOffset CreatedAt { get; }
        DateTimeOffset? UpdatedAt { get; }

        public void SetUpdatedTime(DateTimeOffset? updatedAt = null);
    }
}
