using MauiTrainApp.Domain.Interfaces;

namespace MauiTrainApp.Domain.Entities.Base
{
    public abstract class BaseEntity : IEntity
    {
        #region Properties

        public Guid Id { get; init; } = Guid.NewGuid();

        public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

        public DateTimeOffset? UpdatedAt { get; private set; }

        #endregion

        #region Methods

        public virtual void SetUpdatedTime(DateTimeOffset? updatedAt = null)
        {
            UpdatedAt = updatedAt ?? DateTimeOffset.UtcNow;
        }

        public override bool Equals(object? obj)
        {
            return obj is BaseEntity other
                && GetType() == other.GetType()
                && Id == other.Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(GetType(), Id);
        }

        public static bool operator ==(BaseEntity? left, BaseEntity? right) => Equals(left, right);

        public static bool operator !=(BaseEntity? left, BaseEntity? right) => !Equals(left, right);

        #endregion
    }
}
