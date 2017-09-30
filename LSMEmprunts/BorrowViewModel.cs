using LSMEmprunts.Data;
using Mvvm;
using Mvvm.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LSMEmprunts
{
    public class BorrowViewModel : BindableBase
    {
        private readonly Context _Context;

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
            _Context = ContextFactory.OpenContext();

            ValidateCommand = new DelegateCommand(ValidateCmd, CanValidateCmd);
            CancelCommand = new DelegateCommand(GoBackToHomeView);

            Users = _Context.Users.ToList();
        }

        public void Dispose()
        {
            _Context.Dispose();
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
                if (matchingUsersByName.Count == 1)
                {
                    System.Diagnostics.Debug.WriteLine("User input - found matching user by name");
                    SelectedUser = matchingUsersByName[0];
                    return;
                }

                var matchingUserByLicence = Users.FirstOrDefault(e => e.LicenceScanId == value);
                if (matchingUserByLicence != null)
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

        private List<Borrowing> _BorrowingsToForceClose = new List<Borrowing>();

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
                var valueLower = value.ToLowerInvariant();
                var matchingGear = _Context.Gears.FirstOrDefault(e => e.Name.ToLowerInvariant() == valueLower);


                if (matchingGear != null)
                {
                    System.Diagnostics.Debug.WriteLine("Found matching gear by name");
                }
                else
                {
                    matchingGear = _Context.Gears.FirstOrDefault(e => e.BarCode == value);
                    if (matchingGear != null)
                    {
                        System.Diagnostics.Debug.WriteLine("Found matching gear by scan");
                    }
                }

                if (matchingGear != null)
                {
                    //check for double input of a given gear
                    if (BorrowedGears.Contains(matchingGear))
                    {
                        var vm = new WarningWindowViewModel("Matériel déjà emprunté");
                        MainWindowViewModel.Instance.Dialogs.Add(vm);
                        SetProperty(ref _SelectedGearId, string.Empty);
                        return;
                    }

                    //try to find a still open borrowing of the same gear - force close it if found
                    var existingBorrowing = _Context.Borrowings.FirstOrDefault(e => e.GearId == matchingGear.Id && e.State == BorrowingState.Open);
                    if (existingBorrowing != null)
                    {
                        _BorrowingsToForceClose.Add(existingBorrowing);
                    }

                    BorrowedGears.Add(matchingGear);
                    SetProperty(ref _SelectedGearId, string.Empty);
                    ValidateCommand.RaiseCanExecuteChanged();
                }
                else
                {
                    //if the gear was not found, simply update the typed value
                    SetProperty(ref _SelectedGearId, value);
                }

            }
        }

        public DelegateCommand ValidateCommand { get; }
        private async void ValidateCmd()
        {
            var date = DateTime.Now;

            foreach(var existingBorrowing in _BorrowingsToForceClose)
            {
                existingBorrowing.State = BorrowingState.ForcedClose;
                existingBorrowing.Comment = existingBorrowing.Comment ?? string.Empty + " Clos de force car matériel réemprunté";
                existingBorrowing.ReturnTime = date;
            }

            await _Context.Borrowings.AddRangeAsync(BorrowedGears.Select(e =>
            new Borrowing
            {
                BorrowTime = date,
                Gear = e,
                User = SelectedUser,
                Comment = Comment,
                State = BorrowingState.Open
            }));
            await _Context.SaveChangesAsync();

            GoBackToHomeView();
        }
        private bool CanValidateCmd()
        {
            return SelectedUser != null && BorrowedGears.Any();
        }

        public DelegateCommand CancelCommand { get; }
        private void GoBackToHomeView()
        {
            MainWindowViewModel.Instance.CurrentPageViewModel = new HomeViewModel();
        }
        
    }
}
