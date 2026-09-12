using MauiTrainApp.Domain.Exceptions;

namespace MauiTrainApp.Domain.ValueObjects
{
    public record struct RepScheme
    {
        public required byte Reps
        {
            get;
            init
            {
                if (value == 0) throw new InvariantException("Reps couldn't be 0");
                field = value;
            }
        }
        public required double Weight
        {
            get;
            init
            {
                if (value < 0) throw new InvariantException("Weight couldn't be below 0");
                field = value;
            }
        }
    }
}
