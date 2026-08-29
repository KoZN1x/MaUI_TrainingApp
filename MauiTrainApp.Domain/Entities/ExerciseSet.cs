using MauiTrainApp.Domain.Entities.Base;
using MauiTrainApp.Domain.ValueObjects;

namespace MauiTrainApp.Domain.Entities
{
    public sealed class ExerciseSet : BaseEntity
    {
        private readonly List<WorkingSet> _workingSets = [];

        #region Properties

        public Exercise Exercise { get; private set; } = null!;

        public IReadOnlyCollection<WorkingSet> WorkingSets => _workingSets.AsReadOnly();

        #endregion

        #region Methods

        public void SetExercise(Exercise exercise)
        {
            Exercise = exercise;
        }

        public void AddWorkingSet(WorkingSet workingSet)
        {
            _workingSets.Add(workingSet);

            SetUpdatedTime();
        }

        public void RemoveWorkingSet(WorkingSet workingSet)
        {
            _workingSets.Remove(workingSet);

            SetUpdatedTime();
        }

        public void UpdateWorkingSet(int index, WorkingSet workingSet)
        {
            _workingSets[index] = workingSet;

            SetUpdatedTime();
        }

        #endregion
    }
}
