using MauiTrainApp.Domain.Exceptions;
using MauiTrainApp.Domain.Interfaces;
using MauiTrainApp.Infrastructure.Interfaces;
using MauiTrainApp.Infrastructure.Records.Base;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace MauiTrainApp.Infrastructure.Repositories.Base
{
    internal abstract class WriteRepositoryBase<TEntity, TRecord> : IWriteRepository<TEntity>
        where TEntity : class, IEntity
        where TRecord : BaseRecord
    {
        protected WriteRepositoryBase(
            IRecordMapper<TEntity, TRecord> recordMapper,
            DbContext dbContext)
        {
            Mapper = recordMapper;
            DbContext = dbContext;
        }

        #region Properties

        protected IRecordMapper<TEntity, TRecord> Mapper { get; }

        protected DbContext DbContext { get; }

        protected DbSet<TRecord> DbSet => DbContext.Set<TRecord>();

        protected virtual IQueryable<TRecord> Query => DbSet;

        #endregion

        #region Methods

        public virtual async Task<TEntity> AddAsync(TEntity model, CancellationToken cancellationToken = default)
        {
            var record = Mapper.ToRecord(model);

            await DbSet.AddAsync(record, cancellationToken);
            await SaveChangesAsync(cancellationToken);

            return model;
        }

        public virtual async Task<TEntity> UpdateAsync(TEntity model, CancellationToken cancellationToken = default)
        {
            var record = Mapper.ToRecord(model);

            Detach<TRecord>(record.Id);
            DbSet.Update(record);
            await SaveChangesAsync(cancellationToken);

            return model;
        }

        public virtual async Task DeleteAsync(TEntity model, CancellationToken cancellationToken = default)
        {
            var record = await DbSet.FindAsync([model.Id], cancellationToken)
                ?? throw new EntityNotFoundException<TEntity>(model);

            DbSet.Remove(record);
            await SaveChangesAsync(cancellationToken);
        }

        public virtual Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        protected async Task<TEntity?> FirstOrDefaultAsync(
            Expression<Func<TRecord, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            var record = await Query
                .AsNoTracking()
                .FirstOrDefaultAsync(predicate, cancellationToken);

            return record is null ? null : Mapper.ToDomain(record);
        }

        protected async Task SyncChildrenAsync<TChild>(
            ICollection<TChild> children,
            Expression<Func<TChild, bool>> parentFilter,
            CancellationToken cancellationToken = default)
            where TChild : BaseRecord, new()
        {
            var storedIds = await DbContext.Set<TChild>()
                .AsNoTracking()
                .Where(parentFilter)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            foreach (var child in children)
            {
                Detach<TChild>(child.Id);

                DbContext.Entry(child).State = storedIds.Contains(child.Id)
                    ? EntityState.Modified
                    : EntityState.Added;
            }

            var removedIds = storedIds.Except(children.Select(x => x.Id));

            foreach (var removedId in removedIds)
            {
                Detach<TChild>(removedId);

                DbContext.Entry(new TChild { Id = removedId }).State = EntityState.Deleted;
            }
        }

        protected virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return DbContext.SaveChangesAsync(cancellationToken);
        }

        protected void Detach<TTracked>(Guid id)
            where TTracked : BaseRecord
        {
            var tracked = DbContext.Set<TTracked>().Local.FirstOrDefault(x => x.Id == id);

            if (tracked is not null)
            {
                DbContext.Entry(tracked).State = EntityState.Detached;
            }
        }

        #endregion
    }
}
