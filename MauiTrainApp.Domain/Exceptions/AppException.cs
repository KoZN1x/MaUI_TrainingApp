namespace MauiTrainApp.Domain.Exceptions;

public abstract class AppException : Exception
{
    protected AppException(string message, string? userMessage = null, Exception? innerException = null)
        : base(message, innerException)
    {
        UserMessage = userMessage ?? message;
    }

    public string UserMessage { get; }
}
