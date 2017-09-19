using LSMEmprunts.Data;
using Microsoft.EntityFrameworkCore;
using Mvvm;
using Mvvm.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace LSMEmprunts
{
    public class HomeViewModel : BindableBase
    {
        private readonly ObservableCollection<Borrowing> _ActiveBorrowings;
        
        public ICollectionView ActiveBorrowings { get; }

        public HomeViewModel()
        {
            using (var context = ContextFactory.OpenContext())
            {
                BorrowCommand = new DelegateCommand(BorrowCmd);
                ReturnCommand = new DelegateCommand(ReturnCmd);

                _ActiveBorrowings = new ObservableCollection<Borrowing>(
                    context.Borrowings.Include(e=>e.User).Include(e=>e.Gear)
                    .Where(e => e.State == BorrowingState.Open));
            }

            ActiveBorrowings = CollectionViewSource.GetDefaultView(_ActiveBorrowings);
        }

        public ICommand BorrowCommand { get; }
        private void BorrowCmd()
        {
            MainWindowViewModel.Instance.CurrentPageViewModel = new BorrowViewModel();
        }

        public ICommand ReturnCommand { get; }
        private void ReturnCmd()
        { }

    }
}
