using MauiTrainApp.Domain.Entities;
using MauiTrainApp.Domain.Enums;
using MauiTrainApp.Domain.Interfaces;
using MauiTrainApp.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace MauiTrainApp.Infrastructure.Database.Seeding
{
    internal sealed class DatabaseSeeder
    {
        private readonly MauiTrainAppDbContext _dbContext;
        private readonly IWriteRepository<Exercise> _exercises;
        private readonly IWriteRepository<TrainingPlan> _trainingPlans;

        public DatabaseSeeder(
            MauiTrainAppDbContext dbContext,
            IWriteRepository<Exercise> exercises,
            IWriteRepository<TrainingPlan> trainingPlans)
        {
            _dbContext = dbContext;
            _exercises = exercises;
            _trainingPlans = trainingPlans;
        }

        public async Task SeedAsync(CancellationToken cancellationToken = default)
        {
            if (await _dbContext.Exercises.AnyAsync(cancellationToken)
                || await _dbContext.TrainingPlans.AnyAsync(cancellationToken))
            {
                await BackfillMuscleGroupsAsync(cancellationToken);

                return;
            }

            var exercises = await SeedExercisesAsync(cancellationToken);

            foreach (var plan in SeedPlans.All)
            {
                var exerciseSets = plan.ExerciseSets
                    .Select(x => new ExerciseSet(exercises[x.ExerciseName], WorkingSets(x)))
                    .ToList();

                await _trainingPlans.AddAsync(new TrainingPlan(plan.Name, exerciseSets), cancellationToken);
            }
        }

        private async Task BackfillMuscleGroupsAsync(CancellationToken cancellationToken)
        {
            var known = SeedCatalog.Exercises.ToDictionary(x => x.Name, x => x.MuscleGroup);

            var untagged = await _dbContext.Exercises
                .Where(x => x.MuscleGroup == MuscleGroup.Other)
                .ToListAsync(cancellationToken);

            var updated = false;

            foreach (var record in untagged)
            {
                if (!known.TryGetValue(record.Name, out var muscleGroup) || muscleGroup == MuscleGroup.Other)
                {
                    continue;
                }

                _dbContext.Entry(record).Property(x => x.MuscleGroup).CurrentValue = muscleGroup;
                updated = true;
            }

            if (updated)
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        private async Task<Dictionary<string, Exercise>> SeedExercisesAsync(CancellationToken cancellationToken)
        {
            var exercises = new Dictionary<string, Exercise>(SeedCatalog.Exercises.Count);

            foreach (var seed in SeedCatalog.Exercises)
            {
                var exercise = await _exercises.AddAsync(
                    new Exercise(seed.Name, seed.Description, seed.MuscleGroup),
                    cancellationToken);

                exercises.Add(seed.Name, exercise);
            }

            return exercises;
        }

        private static IEnumerable<WorkingSet> WorkingSets(SeedWorkingSet seed)
        {
            return Enumerable
                .Range(0, seed.Sets)
                .Select(_ => new WorkingSet
                {
                    RepScheme = new RepScheme
                    {
                        Reps = seed.Reps,
                        Weight = seed.Weight
                    }
                });
        }
    }
}
