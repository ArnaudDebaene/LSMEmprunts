using LSMEmprunts.Data;
using Microsoft.EntityFrameworkCore;
using Mvvm;
using Mvvm.Commands;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace LSMEmprunts
{
    public class HomeViewModel : BindableBase
    {       
        public ObservableCollection<Borrowing> ActiveBorrowings { get; }

        public HomeViewModel()
        {
            using (var context = ContextFactory.OpenContext())
            {
                BorrowCommand = new DelegateCommand(BorrowCmd);
                ReturnCommand = new DelegateCommand(ReturnCmd);
                SettingsCommand = new DelegateCommand(SettingsCmd);

                ActiveBorrowings = new ObservableCollection<Borrowing>(
                    context.Borrowings.Include(e=>e.User).Include(e=>e.Gear)
                    .Where(e => e.State == BorrowingState.Open));
            }
        }

        public ICommand BorrowCommand { get; }
        private void BorrowCmd()
        {
            MainWindowViewModel.Instance.CurrentPageViewModel = new BorrowViewModel();
        }

        public ICommand ReturnCommand { get; }
        private void ReturnCmd()
        {
            MainWindowViewModel.Instance.CurrentPageViewModel = new ReturnViewModel();
        }

        public ICommand SettingsCommand { get; }
        private void SettingsCmd()
        {
            MainWindowViewModel.Instance.CurrentPageViewModel = new SettingsViewModel();
        }

    }
}
