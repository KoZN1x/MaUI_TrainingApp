using MauiTrainApp.Domain.Exceptions;

namespace MauiTrainApp.Domain.Entities.Base
{
    public abstract class ExerciseSetAggregate : BaseEntity
    {
        private readonly List<ExerciseSet> _exerciseSets = [];

        protected ExerciseSetAggregate(IEnumerable<ExerciseSet>? exerciseSets = null)
        {
            if (exerciseSets is not null)
            {
                Fill(exerciseSets);
            }
        }

        #region Properties

        public IReadOnlyCollection<ExerciseSet> ExerciseSets => _exerciseSets.AsReadOnly();

        #endregion

        #region Methods

        public void AddExerciseSet(ExerciseSet exerciseSet)
        {
            ArgumentNullException.ThrowIfNull(exerciseSet);

            if (_exerciseSets.Contains(exerciseSet))
            {
                throw new InvariantException($"Exercise set '{exerciseSet.Id}' is already in the collection");
            }

            _exerciseSets.Add(exerciseSet);

            SetUpdatedTime();
        }

        public void RemoveExerciseSet(ExerciseSet exerciseSet)
        {
            ArgumentNullException.ThrowIfNull(exerciseSet);

            RemoveExerciseSet(exerciseSet.Id);
        }

        public void RemoveExerciseSet(Guid exerciseSetId)
        {
            var index = IndexOf(exerciseSetId);

            _exerciseSets.RemoveAt(index);

            SetUpdatedTime();
        }

        public void UpdateExerciseSet(ExerciseSet exerciseSet)
        {
            ArgumentNullException.ThrowIfNull(exerciseSet);

            var index = IndexOf(exerciseSet.Id);

            _exerciseSets[index] = exerciseSet;

            SetUpdatedTime();
        }

        public void ReplaceExerciseSets(IEnumerable<ExerciseSet> exerciseSets)
        {
            ArgumentNullException.ThrowIfNull(exerciseSets);

            _exerciseSets.Clear();
            Fill(exerciseSets);

            SetUpdatedTime();
        }

        private void Fill(IEnumerable<ExerciseSet> exerciseSets)
        {
            foreach (var exerciseSet in exerciseSets)
            {
                ArgumentNullException.ThrowIfNull(exerciseSet);

                if (_exerciseSets.Contains(exerciseSet))
                {
                    throw new InvariantException($"Exercise set '{exerciseSet.Id}' is duplicated in the collection");
                }

                _exerciseSets.Add(exerciseSet);
            }
        }

        private int IndexOf(Guid exerciseSetId)
        {
            var index = _exerciseSets.FindIndex(x => x.Id == exerciseSetId);

            if (index < 0)
            {
                throw new InvariantException($"Exercise set '{exerciseSetId}' was not found in the collection");
            }

            return index;
        }

        #endregion
    }
}
