namespace MauiTrainApp.Domain.Interfaces;

public interface IWriteRepository<TEntity>
    where TEntity : class, IEntity
{
    Task<TEntity> AddAsync(TEntity model, CancellationToken cancellationToken = default);
    Task<TEntity> UpdateAsync(TEntity model, CancellationToken cancellationToken = default);
    Task DeleteAsync(TEntity model, CancellationToken cancellationToken = default);
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
