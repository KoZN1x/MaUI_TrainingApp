namespace MauiTrainApp.Domain.Exceptions;

public sealed class NotFoundException : AppException
{
    public NotFoundException(string message, Exception? innerException = null)
        : base(message, message, innerException)
    {
    }

    public static NotFoundException For<TEntity>(Guid id) =>
        new($"{typeof(TEntity).Name} with id '{id}' was not found");
}
