using MvvmDialogs.Presenters;
using MvvmDialogs.ViewModels;
using System.Collections.Generic;
using System.Windows;

namespace MvvmDialogs
{
    public sealed class MessageBoxViewModel : IDialogViewModel
    {
        public string Caption { get; set; } = "";

        public string Message { get; set; } = "";

        public MessageBoxButton Buttons { get; set; } = MessageBoxButton.OK;

        public MessageBoxImage Image { get; set; } = MessageBoxImage.None;

        public MessageBoxResult Result { get; set; }

        public MessageBoxViewModel(string message = "", string caption = "")
        {
            Message = message;
            Caption = caption;
        }

        public MessageBoxResult Show(IList<IDialogViewModel> collection)
        {
            collection.Add(this);
            return Result;
        }
    }

    public class MessageBoxPresenter : IDialogBoxPresenter<MessageBoxViewModel>
    {
        public void Show(MessageBoxViewModel vm)
        {
            vm.Result = MessageBox.Show(vm.Message, vm.Caption, vm.Buttons, vm.Image);
        }
    }
}