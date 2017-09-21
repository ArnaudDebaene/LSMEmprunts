using LSMEmprunts.Data;
using Microsoft.EntityFrameworkCore;
using Mvvm.Commands;
using MvvmDialogs.ViewModels;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LSMEmprunts
{
    class GearHistoryDlgViewModel : ModalDialogViewModelBase
    {
        private readonly TaskCompletionSource<bool> _ResultTask = new TaskCompletionSource<bool>();
        public Task<bool> HasModifiedData => _ResultTask.Task;

        private readonly Context _Context;
        private readonly Gear _Gear;

        public ObservableCollection<Borrowing> Borrowings { get; }

        public string Title { get; }
             

        public GearHistoryDlgViewModel(Gear gear, Context context)
        {
            ClearHistoryCommand = new DelegateCommand(ClearHistory);

            _Context = context;
            _Gear = gear;

            Title = "Historique d'emprunt " + gear.Type + " " + gear.Name;

            Borrowings = new ObservableCollection<Borrowing>(context.Borrowings.Include(e => e.User)
                .Where(e => e.Gear == gear).OrderByDescending(e => e.BorrowTime));
        }

        public DelegateCommand ClearHistoryCommand { get; }
        private void ClearHistory()
        {
            _Context.Borrowings.RemoveRange(Borrowings);
            Borrowings.Clear();
            _HasModifiedData = true;
        }

        private bool _HasModifiedData = false;

        public override void RequestClose()
        {
            _ResultTask.SetResult(_HasModifiedData);
            base.RequestClose();
        }
    }
}
