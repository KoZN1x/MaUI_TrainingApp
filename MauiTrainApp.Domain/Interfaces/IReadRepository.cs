namespace MauiTrainApp.Domain.Interfaces;

public interface IReadRepository<TListItem, TDetails>
    where TListItem : class
    where TDetails : class
{
    Task<IReadOnlyCollection<TListItem>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<TDetails?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
