namespace MauiTrainApp.Domain.Exceptions
{
    public sealed class InvariantException : Exception
    {
        public InvariantException(string message) : base(message)
        {
        }
    }
}
