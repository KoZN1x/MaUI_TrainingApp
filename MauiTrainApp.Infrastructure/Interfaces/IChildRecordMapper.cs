using MauiTrainApp.Domain.Interfaces;

namespace MauiTrainApp.Infrastructure.Interfaces
{
    internal interface IChildRecordMapper<TEntity, TRecord, in TParentKeyId>
        where TEntity : IEntity
        where TRecord : IDbRecord
    {
        TRecord ToRecord(TEntity entity, TParentKeyId parentKeyId);
        TEntity ToDomain(TRecord record);
    }
}
