using MauiTrainApp.Domain.Interfaces;

namespace MauiTrainApp.Domain.Entities.Base
{
    public abstract class BaseEntity : IEntity
    {
        #region Properties

        public Guid Id { get; init; } = Guid.NewGuid();

        public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.Now;

        public DateTimeOffset? UpdatedAt { get; private set; }

        #endregion

        #region Methods

        public virtual void SetUpdatedTime(DateTimeOffset? updatedAt = null)
        {
            if (updatedAt is null)
            {
                UpdatedAt = DateTimeOffset.Now;
            }
            else
            {
                UpdatedAt = updatedAt.Value;
            }
        }

        #endregion
    }
}
