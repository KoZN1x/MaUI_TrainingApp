using System.Diagnostics;
using MauiTrainApp.Services.Interfaces;
using Microsoft.Maui.Dispatching;

namespace MauiTrainApp.Services
{
    internal sealed class WorkoutClock : IWorkoutClock
    {
        private static readonly TimeSpan Interval = TimeSpan.FromSeconds(1);

        private readonly Stopwatch _stopwatch = new();

        private IDispatcherTimer? _timer;
        private TimeSpan _offset;

        public event EventHandler<TimeSpan>? Ticked;

        public TimeSpan Elapsed => _offset + _stopwatch.Elapsed;

        public bool IsRunning => _stopwatch.IsRunning;

        public void Start(TimeSpan initialElapsed = default)
        {
            if (IsRunning)
            {
                return;
            }

            _offset = initialElapsed;
            _stopwatch.Restart();

            _timer ??= CreateTimer();
            _timer?.Start();

            Ticked?.Invoke(this, Elapsed);
        }

        public void Stop()
        {
            _stopwatch.Stop();
            _timer?.Stop();
        }

        public void Dispose()
        {
            Stop();

            if (_timer is not null)
            {
                _timer.Tick -= OnTick;
                _timer = null;
            }
        }

        private IDispatcherTimer? CreateTimer()
        {
            var dispatcher = Microsoft.Maui.Controls.Application.Current?.Dispatcher
                ?? Dispatcher.GetForCurrentThread();

            if (dispatcher is null)
            {
                return null;
            }

            var timer = dispatcher.CreateTimer();
            timer.Interval = Interval;
            timer.IsRepeating = true;
            timer.Tick += OnTick;

            return timer;
        }

        private void OnTick(object? sender, EventArgs e)
        {
            Ticked?.Invoke(this, Elapsed);
        }
    }
}
