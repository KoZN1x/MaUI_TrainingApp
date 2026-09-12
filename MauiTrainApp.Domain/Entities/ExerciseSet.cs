using MauiTrainApp.Domain.Entities.Base;
using MauiTrainApp.Domain.Exceptions;
using MauiTrainApp.Domain.ValueObjects;

namespace MauiTrainApp.Domain.Entities
{
    public sealed class ExerciseSet : BaseEntity
    {
        private readonly List<WorkingSet> _workingSets = [];

        public ExerciseSet(Exercise exercise, IEnumerable<WorkingSet>? workingSets = null)
        {
            ArgumentNullException.ThrowIfNull(exercise);

            Exercise = exercise;

            if (workingSets is not null)
            {
                _workingSets.AddRange(workingSets);
            }
        }

        #region Properties

        public Exercise Exercise { get; private set; }

        public IReadOnlyCollection<WorkingSet> WorkingSets => _workingSets.AsReadOnly();

        public bool IsCompleted => _workingSets.Count > 0 && _workingSets.All(x => x.IsCompleted);

        #endregion

        #region Methods

        public void SetExercise(Exercise exercise)
        {
            ArgumentNullException.ThrowIfNull(exercise);

            Exercise = exercise;

            SetUpdatedTime();
        }

        public void ReplaceWorkingSets(IEnumerable<WorkingSet> workingSets)
        {
            ArgumentNullException.ThrowIfNull(workingSets);

            _workingSets.Clear();
            _workingSets.AddRange(workingSets);

            SetUpdatedTime();
        }

        public void AddWorkingSet(WorkingSet workingSet)
        {
            _workingSets.Add(workingSet);

            SetUpdatedTime();
        }

        public void RemoveWorkingSet(WorkingSet workingSet)
        {
            if (!_workingSets.Remove(workingSet))
            {
                throw new InvariantException("Working set was not found in the collection");
            }

            SetUpdatedTime();
        }

        public void UpdateWorkingSet(int index, WorkingSet workingSet)
        {
            _workingSets[EnsureIndex(index)] = workingSet;

            SetUpdatedTime();
        }

        public void CompleteWorkingSet(int index)
        {
            _workingSets[EnsureIndex(index)] = _workingSets[EnsureIndex(index)].Complete();

            SetUpdatedTime();
        }

        public void Complete()
        {
            if (_workingSets.Count == 0)
            {
                throw new InvariantException("Exercise set without working sets couldn't be completed");
            }

            for (var index = 0; index < _workingSets.Count; index++)
            {
                _workingSets[index] = _workingSets[index].Complete();
            }

            SetUpdatedTime();
        }

        private int EnsureIndex(int index)
        {
            if (index < 0 || index >= _workingSets.Count)
            {
                throw new InvariantException($"Working set index {index} is out of range");
            }

            return index;
        }

        #endregion
    }
}
