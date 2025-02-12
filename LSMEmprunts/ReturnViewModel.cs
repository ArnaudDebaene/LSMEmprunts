﻿using LSMEmprunts.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace LSMEmprunts
{
    public sealed class GearReturnInfo
    {
        public Gear Gear { get; set; }
        public bool Borrowed { get; set; }

        public string Name
        {
            get
            {
                var converter = new GearTypeToStringConverter();
                var converter2 = new GearDisplayNameConverter();
                return converter.Convert(Gear.Type, typeof(string), null, null) + " " + converter2.Convert(Gear, typeof(string), null, null);
            }
        }
    }

    public sealed class ReturnInfo : ObservableObject
    {
        public Borrowing Borrowing { get; set; }
        public string Comment { get; set; }
    }

    public sealed class ReturnViewModel : ObservableObject, IDisposable
    {
        private readonly Context _Context;

        public ObservableCollection<ReturnInfo> ClosingBorrowings { get; } = new ObservableCollection<ReturnInfo>();

        public ICollectionView Gears { get; }
  

        public ReturnViewModel()
        {
            SelectGearCommand = new RelayCommand<GearReturnInfo>(SelectGearCmd);
            ValidateCommand = new RelayCommand(ValidateCmd, CanValidateCmd);
            CancelCommand = new RelayCommand(GoBackToHomeView);

            ClosingBorrowings.CollectionChanged += (s, e) => ValidateCommand.NotifyCanExecuteChanged();

            _Context = ContextFactory.OpenContext();

            var gears = (from gear in _Context.Gears
                         let borrowed = gear.Borrowings.Any(e => e.State == BorrowingState.Open)
                         select new GearReturnInfo { Gear = gear, Borrowed = borrowed }).OrderByDescending(e=>e.Borrowed)
                         .ToList();

            Gears = CollectionViewSource.GetDefaultView(gears);
            Gears.Filter = (item) =>
            {
                var gearInfo = (GearReturnInfo)item;
                if (ClosingBorrowings.Any(e=>e.Borrowing.Gear==gearInfo.Gear))
                {
                    return false;
                }
                return true;
            };

            GearInputFocused = true;
        }

        public void Dispose()
        {
            AutoValidateTicker?.Dispose();
            _Context?.Dispose();
        }

        private string _SelectedGearId;
        public string SelectedGearId
        {
            get => _SelectedGearId;
            set
            {
                var valueLower = value.ToLower();
                var matchingGear = _Context.Gears.FirstOrDefault(e => e.Name.ToLower() == valueLower);
                if (matchingGear != null)
                {
                    System.Diagnostics.Debug.WriteLine("Found a matching gear by name");
                }
                else
                {
                    matchingGear = _Context.Gears.FirstOrDefault(e => e.BarCode == value);
                    if (matchingGear != null)
                    {
                        System.Diagnostics.Debug.WriteLine("Found a matching gear by scan");
                    }
                }

                if (matchingGear != null)
                {
                    SelectGearToReturn(matchingGear);
                }
                else
                {
                    SetProperty(ref _SelectedGearId, value);
                }
            }
        }

        public RelayCommand ValidateCommand { get; }
        private void ValidateCmd()
        {
            _AutoValidateTicker?.Dispose();

            _Context.SaveChanges();
            GoBackToHomeView();
        }
        private bool CanValidateCmd() => ClosingBorrowings.Any();

        public RelayCommand CancelCommand { get; }
        private void GoBackToHomeView()
        {
            MainWindowViewModel.Instance.CurrentPageViewModel = new HomeViewModel();
        }

        public RelayCommand<GearReturnInfo> SelectGearCommand { get; }
        public void SelectGearCmd(GearReturnInfo info)
        {
            SelectGearToReturn(info.Gear);
        }

        private CountDownTicker _AutoValidateTicker;
        public CountDownTicker AutoValidateTicker
        {
            get => _AutoValidateTicker;
            set => SetProperty(ref _AutoValidateTicker, value);
        }

        private void SelectGearToReturn(Gear gear)
        {
            //check for double input of a given gear
            if (ClosingBorrowings.Any(e => e.Borrowing.Gear == gear))
            {
                var vm = new WarningWindowViewModel("Matériel déjà rendu");
                MainWindowViewModel.Instance.Dialogs.Add(vm);
                SetProperty(ref _SelectedGearId, string.Empty);
                return;
            }

            var now = DateTime.Now;

            var matchingBorrowing = _Context.Borrowings.Include(e => e.Gear)
                .FirstOrDefault(e => e.Gear == gear && e.State == BorrowingState.Open);

            if (matchingBorrowing != null)
            {
                matchingBorrowing.ReturnTime = now;
                matchingBorrowing.State = BorrowingState.GearReturned;
                ClosingBorrowings.Add(new ReturnInfo { Borrowing = matchingBorrowing });
            }
            else
            {
                var msgVm = new WarningWindowViewModel("Matériel retourné sans avoir été emprunté");
                MainWindowViewModel.Instance.Dialogs.Add(msgVm);

                //create a "fake" borrowing with same Borrow / Return date
                var borrowing = new Borrowing
                {
                    BorrowTime = now,
                    ReturnTime = now,
                    Gear = gear,
                    State = BorrowingState.GearReturned,
                    Comment = "Retourné sans avoir été emprunté",
                };
                _Context.Borrowings.Add(borrowing);
                ClosingBorrowings.Add(new ReturnInfo { Borrowing = borrowing, Comment = "Retourné sans avoir été emprunté" });
            }

            if (AutoValidateTicker == null)
            {
                AutoValidateTicker = new CountDownTicker(60);
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

            SetProperty(ref _SelectedGearId, string.Empty, nameof(SelectedGearId));
            Gears.Refresh();
            GearInputFocused = true;
        }

        private bool _GearInputFocused;
        public bool GearInputFocused
        {
            get => _GearInputFocused;
            set => SetProperty(ref _GearInputFocused, value);
        }
    }
}
