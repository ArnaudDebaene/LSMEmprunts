using MvvmDialogs.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace LSMEmprunts
{
    public class WarningWindowViewModel : ModalDialogViewModelBase
    {
        public string Message { get; }

        public TimeSpan Duration;

        public WarningWindowViewModel(string msg)
        {
            Message = msg;
            Duration = TimeSpan.FromSeconds(4);
        }

        private DispatcherTimer _Timer;

        public void OnViewLoaded()
        {
            _Timer = new DispatcherTimer { Interval = Duration };
            _Timer.Tick += OnTimerElapsed;
            _Timer.Start();
        }

        private void OnTimerElapsed(object sender, EventArgs e)
        {
            _Timer.Stop();
            RequestClose();
        }
    }
}
