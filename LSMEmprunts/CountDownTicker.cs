using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Windows.Threading;

namespace LSMEmprunts
{
    public sealed class CountDownTicker : ObservableObject, IDisposable
    {
        public event Action Tick;

        private readonly int _InitialTime;
        private readonly DispatcherTimer _Timer;

        public CountDownTicker(int duration)
        {
            _InitialTime = duration;
            RemainingTime = duration;
            _Timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _Timer.Tick += OnTimerTick;
            _Timer.Start();
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            RemainingTime = RemainingTime - 1;
            if (RemainingTime==0 && !_Disposed)
            {
                Tick?.Invoke();
            }
        }

        private int _RemainingTime;
        public int RemainingTime
        {
            get => _RemainingTime;
            set => SetProperty(ref _RemainingTime, value);
        }

        public void Reset()
        {
            RemainingTime = _InitialTime;
        }

        private bool _Disposed;

        public void Dispose()
        {
            if (!_Disposed)
            {
                _Disposed = true;
                _Timer.Stop();
            }
        }
    }
}
