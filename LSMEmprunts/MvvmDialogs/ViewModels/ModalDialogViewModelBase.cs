using Mvvm.Commands;
using System;

namespace MvvmDialogs.ViewModels
{
    public abstract class ModalDialogViewModelBase : Mvvm.BindableBase, IUserDialogViewModel
    {
        protected ModalDialogViewModelBase()
        {
            CloseCommand = new DelegateCommand(RequestClose);
        }

        public virtual bool IsModal => true;

        public event EventHandler DialogClosing;

        public virtual void RequestClose()
        {
            DialogClosing?.Invoke(this, null);
        }

        protected bool DialogResult = false;

        public DelegateCommand CloseCommand { get; }
    }
}
