using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace MvvmDialogs.ViewModels
{
    public abstract class ModalDialogViewModelBase : ObservableObject, IUserDialogViewModel
    {
        protected ModalDialogViewModelBase()
        {
            CloseCommand = new RelayCommand(RequestClose);
        }

        public virtual bool IsModal => true;

        public event EventHandler DialogClosing;

        public virtual void RequestClose()
        {
            DialogClosing?.Invoke(this, EventArgs.Empty);
        }

        protected bool DialogResult = false;

        public RelayCommand CloseCommand { get; }
    }
}
