namespace MauiTrainApp.Domain.Exceptions;

public sealed class UnexpectedException : AppException
{
    private const string DefaultUserMessage = "Something went wrong. Please try again.";

    public UnexpectedException(Exception innerException)
        : base(innerException.Message, DefaultUserMessage, innerException)
    {
    }
}
