using Microsoft.Win32;
using MvvmDialogs.Presenters;
using MvvmDialogs.ViewModels;
using System.Threading.Tasks;

namespace MvvmDialogs
{
    public sealed class SaveFileDialogViewModel : IDialogViewModel
    {
        public bool OverwritePrompt { get; set; } = true;
        public bool CreatePrompt { get; set; } = false;
        public string FileName { get; set; }
        public string InitialDirectory { get; set; } = string.Empty;
        public string Filter { get; set; } = string.Empty;
        public bool AddExtension { get; set; } = true;
        public string Title { get; set; }
        public bool ValidateNames { get; set; }

        internal readonly TaskCompletionSource<bool> ResultPromise = new TaskCompletionSource<bool>();
        public Task<bool> Completion => ResultPromise.Task;
    }

    public class SaveFileDialogPresenter : IDialogBoxPresenter<SaveFileDialogViewModel>
    {
        public void Show(SaveFileDialogViewModel vm)
        {
            var dlg = new SaveFileDialog
            {
                OverwritePrompt = vm.OverwritePrompt,
                CreatePrompt = vm.CreatePrompt,
                FileName = vm.FileName,
                InitialDirectory = vm.InitialDirectory,
                Filter = vm.Filter,
                AddExtension = vm.AddExtension,
                Title = vm.Title,
                ValidateNames = vm.ValidateNames,
            };

            var result = dlg.ShowDialog();

            vm.FileName = dlg.FileName;
            vm.Filter = dlg.Filter;
            vm.AddExtension = dlg.AddExtension;
            vm.InitialDirectory = dlg.InitialDirectory;
            vm.Title = dlg.Title;
            vm.ValidateNames = dlg.ValidateNames;
            vm.ResultPromise.SetResult(result ?? false);
        }
    }
}
