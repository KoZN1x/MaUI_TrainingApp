using MauiTrainApp.Infrastructure.Interfaces;

namespace MauiTrainApp.Infrastructure.Records.Base
{
    internal abstract record BaseRecord : IDbRecord
    {
        public Guid Id { get; init; }

        public DateTimeOffset CreatedAt { get; init; }

        public DateTimeOffset? UpdatedAt { get; init; }
    }
}
