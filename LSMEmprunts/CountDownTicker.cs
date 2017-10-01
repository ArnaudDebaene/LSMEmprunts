using Mvvm;
using System;
using System.Windows.Threading;

namespace LSMEmprunts
{
    public class CountDownTicker : BindableBase
    {
        public event Action Tick;

        private readonly int _InitialTime;
        private DispatcherTimer _Timer;

        public CountDownTicker(int duration)
        {
            _InitialTime = duration;
            RemainingTime = duration;
            _Timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _Timer.Tick += OnTimerTick;
            _Timer.Start(); ;
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            RemainingTime = RemainingTime - 1;
            if (RemainingTime==0)
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

    }
}
