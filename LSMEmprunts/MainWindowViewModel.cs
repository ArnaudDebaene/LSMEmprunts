using Mvvm;
using MvvmDialogs.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public ObservableCollection<IDialogViewModel> Dialogs { get; } = new ObservableCollection<IDialogViewModel>();
    }
}
