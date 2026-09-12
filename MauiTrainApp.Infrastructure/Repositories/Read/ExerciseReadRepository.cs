using MauiTrainApp.Domain.Interfaces;
using MauiTrainApp.Domain.ReadModels;
using MauiTrainApp.Infrastructure.Database;
using MauiTrainApp.Infrastructure.Records;
using MauiTrainApp.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace MauiTrainApp.Infrastructure.Repositories.Read
{
    internal sealed class ExerciseReadRepository
        : ReadRepositoryBase<ExerciseRecord, ExerciseReadModel, ExerciseReadModel>, IExerciseReadRepository
    {
        public ExerciseReadRepository(MauiTrainAppDbContext dbContext)
            : base(dbContext)
        {
        }

        public Task<IReadOnlyCollection<ExerciseReadModel>> SearchByNameAsync(
            string searchTerm,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(searchTerm);

            return ListAsync(
                Query.Where(x => EF.Functions.Like(x.Name, $"%{searchTerm.Trim()}%")),
                cancellationToken);
        }

        public Task<bool> ExistsWithNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return Query.AnyAsync(x => x.Name == name, cancellationToken);
        }

        protected override async Task<IReadOnlyCollection<ExerciseReadModel>> ListAsync(
            IQueryable<ExerciseRecord> query,
            CancellationToken cancellationToken)
        {
            return await Project(query.OrderBy(x => x.Name))
                .ToListAsync(cancellationToken);
        }

        protected override Task<ExerciseReadModel?> DetailsAsync(
            IQueryable<ExerciseRecord> query,
            CancellationToken cancellationToken)
        {
            return Project(query).FirstOrDefaultAsync(cancellationToken);
        }

        private static IQueryable<ExerciseReadModel> Project(IQueryable<ExerciseRecord> query)
        {
            return query.Select(x => new ExerciseReadModel(x.Id, x.Name, x.Description));
        }
    }
}
