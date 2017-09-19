using LSMEmprunts.Data;
using Mvvm;
using Mvvm.Commands;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LSMEmprunts
{
    public class BorrowViewModel : BindableBase
    {
        public List<User> Users { get; }

        private User _SelectedUser;
        public User SelectedUser
        {
            get => _SelectedUser;
            set
            {
                SetProperty(ref _SelectedUser, value);

                _SelectedUserText = value.Name;
                OnPropertyChanged(nameof(SelectedUserText));
                ValidateCommand.RaiseCanExecuteChanged();
            }
        }

        public BorrowViewModel()
        {
            ValidateCommand = new DelegateCommand(ValidateCmd, CanValidateCmd);
            CancelCommand = new DelegateCommand(CancelCmd, CanCancelCmd);

            using (var context = ContextFactory.OpenContext())
            {
                Users = context.Users.ToList();
            }
        }

        private string _SelectedUserText;
        public string SelectedUserText
        {
            get
            {
                return _SelectedUserText;
            }
            set
            {
                var lowerValue = value.ToLowerInvariant();
                var matchingUsersByName = Users.Where(e => e.Name.ToLowerInvariant().StartsWith(lowerValue)).ToList();
                if (matchingUsersByName.Count==1)
                {
                    System.Diagnostics.Debug.WriteLine("User input - found matching user by name");
                    SelectedUser = matchingUsersByName[0];
                    return;
                }
                
                var matchingUserByLicence = Users.FirstOrDefault(e => e.LicenceScanId == value);
                if (matchingUserByLicence!=null)
                {
                    System.Diagnostics.Debug.WriteLine("User input - found matching user by licence");
                    SelectedUser = matchingUserByLicence;
                    return;
                }

                //if we were not able to find any matching user, just remember what is currently typed
                SetProperty(ref _SelectedUserText, value);
                _SelectedUser = null;
                OnPropertyChanged(nameof(SelectedUser));
            }
        }

        private bool _UserSet = false;
        public bool UserSet
        {
            get => _UserSet;
            set => SetProperty(ref _UserSet, value);
        }

        public ObservableCollection<Gear> BorrowedGears { get; } = new ObservableCollection<Gear>();

        private string _Comment;
        public string Comment
        {
            get => _Comment;
            set => SetProperty(ref _Comment, value);
        }

        private string _SelectedGearId;
        public string SelectedGearId
        {
            get => _SelectedGearId;
            set
            {
                using (var context = ContextFactory.OpenContext())
                {
                    var valueLower = value.ToLowerInvariant();
                    var gearByName = context.Gears.FirstOrDefault(e => e.Name.ToLowerInvariant() == valueLower);
                    if (gearByName!=null)
                    {
                        System.Diagnostics.Debug.WriteLine("Found matching gear by name");
                        BorrowedGears.Add(gearByName);
                        SetProperty(ref _SelectedGearId, string.Empty);
                        return;
                    }

                    var gearByScan = context.Gears.FirstOrDefault(e => e.BarCode == value);
                    if (gearByScan!=null)
                    {
                        System.Diagnostics.Debug.WriteLine("Found matching gear by scan");
                        BorrowedGears.Add(gearByScan);
                        SetProperty(ref _SelectedGearId, string.Empty);
                        return;
                    }

                    //if the gear was not found, simply update the typed value
                    SetProperty(ref _SelectedGearId, value);
                }
            }
        }

        public DelegateCommand ValidateCommand { get; }
        private void ValidateCmd()
        {
            if (!UserSet)
            {
                UserSet = true;
                return;
            }
        }
        private bool CanValidateCmd()
        {
            if (!UserSet)
            {
                return SelectedUser != null;
            }
            else
            {
                return BorrowedGears.Any();
            }
        }

        public DelegateCommand CancelCommand { get; }
        private void CancelCmd()
        {
        }
        private bool CanCancelCmd()
        {
            return true;
        }
    }
}
