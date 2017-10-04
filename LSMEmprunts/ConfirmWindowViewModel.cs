using System.Threading.Tasks;
using Mvvm.Commands;
using MvvmDialogs.ViewModels;

namespace LSMEmprunts
{
    public class ConfirmWindowViewModel : ModalDialogViewModelBase
    {
        public string Message { get; }

        public ConfirmWindowViewModel(string msg)
        {
            Message = msg;
            ConfirmCommand=new DelegateCommand(ConfirmCmd);
        }

        private readonly TaskCompletionSource<bool> _ResultTask = new TaskCompletionSource<bool>();
        public Task<bool> Result => _ResultTask.Task;

        private bool _Confirmed;

        public override void RequestClose()
        {
            _ResultTask.SetResult(_Confirmed);
            base.RequestClose();
        }

        public DelegateCommand ConfirmCommand { get; }

        private void ConfirmCmd()
        {
            _Confirmed = true;
            RequestClose();
        }
    }
}
