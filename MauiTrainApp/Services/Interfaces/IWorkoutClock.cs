namespace MauiTrainApp.Services.Interfaces
{
    public interface IWorkoutClock : IDisposable
    {
        event EventHandler<TimeSpan>? Ticked;

        TimeSpan Elapsed { get; }

        bool IsRunning { get; }

        void Start(TimeSpan initialElapsed = default);

        void Stop();
    }
}
