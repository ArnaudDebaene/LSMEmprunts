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
            CancelCommand = new DelegateCommand(CancelCmd);

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
                    var matchingGear = context.Gears.FirstOrDefault(e => e.Name.ToLowerInvariant() == valueLower);


                    if (matchingGear != null)
                    {
                        System.Diagnostics.Debug.WriteLine("Found matching gear by name");
                    }
                    else
                    {
                        matchingGear = context.Gears.FirstOrDefault(e => e.BarCode == value);
                        if (matchingGear!=null)
                        {
                            System.Diagnostics.Debug.WriteLine("Found matching gear by scan");
                        }
                    }

                    if (matchingGear != null)
                    {
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
        }

        public DelegateCommand ValidateCommand { get; }
        private async void ValidateCmd()
        {
            var date = DateTime.Now;
            using (var context = ContextFactory.OpenContext())
            {
                await context.Borrowings.AddRangeAsync(BorrowedGears.Select(e =>
                new Borrowing
                {
                    BorrowTime = date,
                    GearId = e.Id,
                    UserId = SelectedUser.Id,
                    Comment = Comment,
                    State = BorrowingState.Open
                }));
                await context.SaveChangesAsync();
            }

            MainWindowViewModel.Instance.CurrentPageViewModel = new HomeViewModel();
        }
        private bool CanValidateCmd()
        {
            return SelectedUser != null && BorrowedGears.Any();
        }

        public DelegateCommand CancelCommand { get; }
        private void CancelCmd()
        {
            MainWindowViewModel.Instance.CurrentPageViewModel = new HomeViewModel();
        }
        
    }
}
