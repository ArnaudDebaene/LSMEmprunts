using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs.ViewModels;

namespace LSMEmprunts
{
    public sealed class ConfirmWindowViewModel : ModalDialogViewModelBase
    {
        public string Message { get; }

        public ConfirmWindowViewModel(string msg)
        {
            Message = msg;
            ConfirmCommand=new RelayCommand(ConfirmCmd);
        }

        private readonly TaskCompletionSource<bool> _ResultTask = new TaskCompletionSource<bool>();
        public Task<bool> Result => _ResultTask.Task;

        private bool _Confirmed;

        public override void RequestClose()
        {
            _ResultTask.SetResult(_Confirmed);
            base.RequestClose();
        }

        public RelayCommand ConfirmCommand { get; }

        private void ConfirmCmd()
        {
            _Confirmed = true;
            RequestClose();
        }
    }
}
