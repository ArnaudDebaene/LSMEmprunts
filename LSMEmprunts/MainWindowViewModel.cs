using Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSMEmprunts
{
    public class MainWindowViewModel : BindableBase
    {
        public static MainWindowViewModel Instance { get; } = new MainWindowViewModel();

        private MainWindowViewModel()
        {
            CurrentPageViewModel = new HomeViewModel();
        }

        private object _CurrentPageViewModel;
        public object CurrentPageViewModel
        {
            get => _CurrentPageViewModel;
            set
            {
                (_CurrentPageViewModel as IDisposable)?.Dispose();
                SetProperty(ref _CurrentPageViewModel, value);
            }
        }


    }
}
