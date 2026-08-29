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

        public virtual void SetUpdatedTime()
        {
            UpdatedAt = DateTimeOffset.Now;
        }

        #endregion
    }
}
