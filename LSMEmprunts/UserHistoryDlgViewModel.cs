using LSMEmprunts.Data;
using Microsoft.EntityFrameworkCore;
using Mvvm.Commands;
using MvvmDialogs.ViewModels;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LSMEmprunts
{
    internal sealed class UserHistoryDlgViewModel : ModalDialogViewModelBase
    {
        private readonly TaskCompletionSource<bool> _ResultTask = new TaskCompletionSource<bool>();
        public Task<bool> HasModifiedData => _ResultTask.Task;

        private readonly Context _Context;

        public ObservableCollection<Borrowing> Borrowings { get; }

        public string Title { get; }

        public UserHistoryDlgViewModel(User user, Context context)
        {
            ClearHistoryCommand = new DelegateCommand(ClearHistory);

            _Context = context;

            Title = "Matériel emprunté par " + user.Name;

            Borrowings = new ObservableCollection<Borrowing>(context.Borrowings.Include(e => e.Gear)
                .Where(e => e.User == user).OrderByDescending(e => e.BorrowTime));
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