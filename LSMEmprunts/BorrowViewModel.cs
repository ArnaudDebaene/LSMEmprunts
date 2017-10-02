﻿using LSMEmprunts.Data;
using Mvvm;
using Mvvm.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace LSMEmprunts
{
    public class BorrowViewModel : BindableBase
    {
        private readonly Context _Context;

        private readonly List<User> _UsersList;

        public ICollectionView Users { get; }

        private User _SelectedUser;
        public User SelectedUser
        {
            get => _SelectedUser;
            set
            {
                bool wasChanged = SetProperty(ref _SelectedUser, value);
                ValidateCommand.RaiseCanExecuteChanged();

                if (wasChanged && value != null)
                {
                    _SelectedUserText = null;
                    OnPropertyChanged(nameof(SelectedUserText));
                    Users.Refresh();
                }
            }
        }

        public BorrowViewModel()
        {
            _Context = ContextFactory.OpenContext();

            ValidateCommand = new DelegateCommand(ValidateCmd, CanValidateCmd);
            CancelCommand = new DelegateCommand(GoBackToHomeView);

            _UsersList = _Context.Users.ToList();
            Users = CollectionViewSource.GetDefaultView(_UsersList);
            Users.Filter = (item) =>
            {
                if (string.IsNullOrEmpty(_SelectedUserText))
                {
                    return true;
                }
                else
                {
                    return ((User)item).Name.StartsWith(_SelectedUserText, StringComparison.CurrentCultureIgnoreCase);
                }
            };
        }

        public void Dispose()
        {
            _Context.Dispose();
        }

        private string _SelectedUserText;
        public string SelectedUserText
        {
            get => _SelectedUserText;
            set
            {
                _SelectedUserText = value;
                Users.Refresh();

                if (Users.Cast<User>().Count() == 1)
                {
                    System.Diagnostics.Debug.WriteLine("User input - found matching user by name");
                    SelectedUser = Users.Cast<User>().First();
                    return;
                }

                var matchingUserByLicence = _UsersList.FirstOrDefault(e => e.LicenceScanId == value);
                if (matchingUserByLicence != null)
                {
                    System.Diagnostics.Debug.WriteLine("User input - found matching user by licence");
                    SelectedUser = matchingUserByLicence;
                    return;
                }

                OnPropertyChanged();
            }
        }

        private readonly List<Borrowing> _BorrowingsToForceClose = new List<Borrowing>();

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

                    //start auto close ticker if required
                    if (AutoValidateTicker==null)
                    {
                        AutoValidateTicker = new CountDownTicker(20);
                        AutoValidateTicker.Tick += () =>
                        {
                            if (CanValidateCmd())
                                ValidateCmd();
                        };
                    }
                    else
                    {
                        AutoValidateTicker.Reset();
                    }
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

        private CountDownTicker _AutoValidatTicker;
        public CountDownTicker AutoValidateTicker
        {
            get => _AutoValidatTicker;
            set => SetProperty(ref _AutoValidatTicker, value);
        }
    }
}
