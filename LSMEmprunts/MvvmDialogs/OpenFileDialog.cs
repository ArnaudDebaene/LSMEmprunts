using Microsoft.Win32;
using MvvmDialogs.Presenters;
using MvvmDialogs.ViewModels;
using System.Threading.Tasks;


namespace MvvmDialogs
{
    /// <summary>
    /// A ViewModel for a system standard Open File Dialog
    /// </summary>
    public sealed class OpenFileDialogViewModel : IDialogViewModel
    {
        public bool Multiselect { get; set; } = false;
        public bool ReadOnlyChecked { get; set; }
        public bool ShowReadOnly { get; set; } = true;
        public string FileName { get; set; }
        public string[] FileNames { get; set; }
        public string Filter { get; set; } = string.Empty;
        public string InitialDirectory { get; set; } = string.Empty;
        public string SafeFileName { get; set; }
        public string[] SafeFileNames { get; set; }
        public string Title { get; set; }
        public bool ValidateNames { get; set; }

        internal readonly TaskCompletionSource<bool> ResultPromise = new TaskCompletionSource<bool>();
        public Task<bool> Completion => ResultPromise.Task;
    }

    /// <summary>
    /// the dialog box presenter to "glue" an OpenFileDialogViewModel with the MVVM dialog framework
    /// </summary>
    public class OpenFileDialogPresenter : IDialogBoxPresenter<OpenFileDialogViewModel>
    {
        public void Show(OpenFileDialogViewModel vm)
        {
            var dlg = new OpenFileDialog
            {
                Multiselect = vm.Multiselect,
                ReadOnlyChecked = vm.ReadOnlyChecked,
                ShowReadOnly = vm.ShowReadOnly,
                FileName = vm.FileName,
                Filter = vm.Filter,
                InitialDirectory = vm.InitialDirectory,
                Title = vm.Title,
                ValidateNames = vm.ValidateNames
            };


            var result = dlg.ShowDialog();
            vm.ResultPromise.SetResult(result ?? false);

            vm.Multiselect = dlg.Multiselect;
            vm.ReadOnlyChecked = dlg.ReadOnlyChecked;
            vm.ShowReadOnly = dlg.ShowReadOnly;
            vm.FileName = dlg.FileName;
            vm.FileNames = dlg.FileNames;
            vm.Filter = dlg.Filter;
            vm.InitialDirectory = dlg.InitialDirectory;
            vm.SafeFileName = dlg.SafeFileName;
            vm.SafeFileNames = dlg.SafeFileNames;
            vm.Title = dlg.Title;
            vm.ValidateNames = dlg.ValidateNames;
        }
    }
}
