namespace MauiTrainApp.Domain.Exceptions;

public sealed class ValidationException : AppException
{
    public ValidationException(string message, Exception? innerException = null)
        : base(message, message, innerException)
    {
    }
}
