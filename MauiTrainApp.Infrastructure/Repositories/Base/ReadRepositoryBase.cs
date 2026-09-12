using MauiTrainApp.Domain.Interfaces;
using MauiTrainApp.Infrastructure.Database;
using MauiTrainApp.Infrastructure.Records.Base;
using Microsoft.EntityFrameworkCore;

namespace MauiTrainApp.Infrastructure.Repositories.Base
{
    internal abstract class ReadRepositoryBase<TRecord, TListItem, TDetails> : IReadRepository<TListItem, TDetails>
        where TRecord : BaseRecord
        where TListItem : class
        where TDetails : class
    {
        protected ReadRepositoryBase(MauiTrainAppDbContext dbContext)
        {
            DbContext = dbContext;
        }

        #region Properties

        protected MauiTrainAppDbContext DbContext { get; }

        protected IQueryable<TRecord> Query => DbContext.Set<TRecord>().AsNoTracking();

        #endregion

        #region Methods

        public virtual Task<IReadOnlyCollection<TListItem>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return ListAsync(Query, cancellationToken);
        }

        public virtual Task<TDetails?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return DetailsAsync(Query.Where(x => x.Id == id), cancellationToken);
        }

        protected abstract Task<IReadOnlyCollection<TListItem>> ListAsync(
            IQueryable<TRecord> query,
            CancellationToken cancellationToken);

        protected abstract Task<TDetails?> DetailsAsync(
            IQueryable<TRecord> query,
            CancellationToken cancellationToken);

        #endregion
    }
}
