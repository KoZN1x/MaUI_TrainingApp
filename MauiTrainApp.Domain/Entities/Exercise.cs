using MauiTrainApp.Domain.Entities.Base;

namespace MauiTrainApp.Domain.Entities
{
    public sealed class Exercise : BaseEntity
    {
        #region Properties

        public string Name { get; private set; }

        public string? Description { get; private set; }

        #endregion

        #region Methods

        public void SetName(string name)
        {
            Name = name;
        }

        public void SetDescription(string? description)
        {
            Description = description;
        }

        #endregion
    }
}
