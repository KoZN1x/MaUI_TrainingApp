using MauiTrainApp.Domain.Exceptions;

namespace MauiTrainApp.Domain.ValueObjects
{
    public readonly record struct WeekSchedule
    {
        private const int FullWeek = 0b111_1111;

        private readonly byte _mask;

        private WeekSchedule(byte mask)
        {
            _mask = mask;
        }

        #region Properties

        public static WeekSchedule Empty => new(0);

        public int Mask => _mask;

        public bool IsEmpty => _mask == 0;

        public IReadOnlyCollection<DayOfWeek> Days =>
            [.. Ordered().Where(Contains)];

        #endregion

        #region Methods

        public static WeekSchedule FromMask(int mask)
        {
            if (mask < 0 || mask > FullWeek)
            {
                throw new InvariantException($"Week schedule mask {mask} is out of range");
            }

            return new WeekSchedule((byte)mask);
        }

        public static WeekSchedule From(IEnumerable<DayOfWeek> days)
        {
            ArgumentNullException.ThrowIfNull(days);

            var mask = 0;

            foreach (var day in days)
            {
                mask |= Bit(day);
            }

            return new WeekSchedule((byte)mask);
        }

        public bool Contains(DayOfWeek day) => (_mask & Bit(day)) != 0;

        public WeekSchedule Toggle(DayOfWeek day) => new((byte)(_mask ^ Bit(day)));

        public int? DaysUntil(DayOfWeek from)
        {
            if (IsEmpty)
            {
                return null;
            }

            for (var offset = 0; offset < 7; offset++)
            {
                if (Contains((DayOfWeek)(((int)from + offset) % 7)))
                {
                    return offset;
                }
            }

            return null;
        }

        private static int Bit(DayOfWeek day)
        {
            if (day < DayOfWeek.Sunday || day > DayOfWeek.Saturday)
            {
                throw new InvariantException($"Day of week {day} is not supported");
            }

            return 1 << (int)day;
        }

        private static IEnumerable<DayOfWeek> Ordered()
        {
            yield return DayOfWeek.Monday;
            yield return DayOfWeek.Tuesday;
            yield return DayOfWeek.Wednesday;
            yield return DayOfWeek.Thursday;
            yield return DayOfWeek.Friday;
            yield return DayOfWeek.Saturday;
            yield return DayOfWeek.Sunday;
        }

        #endregion
    }
}
