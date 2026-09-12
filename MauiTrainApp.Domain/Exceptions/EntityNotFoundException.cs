using MauiTrainApp.Domain.Interfaces;

namespace MauiTrainApp.Domain.Exceptions;

public class EntityNotFoundException<TEntity> : Exception where TEntity : IEntity
{
    public EntityNotFoundException(TEntity entity) :  base($"{entity.GetType()} with  id '{entity.Id}' was not found.")
    {
        
    }
}