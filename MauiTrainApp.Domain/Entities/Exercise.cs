using MauiTrainApp.Domain.Entities.Base;
using MauiTrainApp.Domain.Enums;
using MauiTrainApp.Domain.Exceptions;

namespace MauiTrainApp.Domain.Entities
{
    public sealed class Exercise : BaseEntity
    {
        public Exercise(string name, string? description = null, MuscleGroup muscleGroup = MuscleGroup.Other)
        {
            Name = EnsureName(name);
            Description = description;
            MuscleGroup = muscleGroup;
        }

        #region Properties

        public string Name { get; private set; }

        public string? Description { get; private set; }

        public MuscleGroup MuscleGroup { get; private set; }

        #endregion

        #region Methods

        public void SetName(string name)
        {
            Name = EnsureName(name);

            SetUpdatedTime();
        }

        public void SetDescription(string? description)
        {
            Description = description;

            SetUpdatedTime();
        }

        public void SetMuscleGroup(MuscleGroup muscleGroup)
        {
            MuscleGroup = muscleGroup;

            SetUpdatedTime();
        }

        private static string EnsureName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new InvariantException("Exercise name couldn't be empty");
            }

            return name.Trim();
        }

        #endregion
    }
}
