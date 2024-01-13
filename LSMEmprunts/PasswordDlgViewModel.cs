using CommunityToolkit.Mvvm.Input;
using MvvmDialogs.ViewModels;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace LSMEmprunts
{
    public sealed class PasswordDlgViewModel : ModalDialogViewModelBase
    {
        private readonly TaskCompletionSource<string> _ResultTask = new TaskCompletionSource<string>();
        public Task<string> Result => _ResultTask.Task;

        public PasswordDlgViewModel()
        {
            OkCommand = new RelayCommand<PasswordBox>(OnOk);
            CancelCommand = new RelayCommand(OnCancel);
        }

        public RelayCommand<PasswordBox> OkCommand { get; }

        public void OnOk(PasswordBox box)
        {
            RequestClose();
            _ResultTask.SetResult(box.Password);
        }

        public RelayCommand CancelCommand { get; }

        public void OnCancel()
        {
            RequestClose();
            _ResultTask.SetResult(null);
        }
    }
}