using MauiTrainApp.Domain.Interfaces;

namespace MauiTrainApp.Infrastructure.Interfaces
{
    internal interface IRecordMapper<TEntity, TRecord> 
        where TEntity : IEntity 
        where TRecord : IDbRecord
    {
        TEntity ToDomain(TRecord record);
        TRecord ToRecord(TEntity entity);
    }
}
