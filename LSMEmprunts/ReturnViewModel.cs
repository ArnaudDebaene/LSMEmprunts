using LSMEmprunts.Data;
using Microsoft.EntityFrameworkCore;
using Mvvm;
using Mvvm.Commands;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace LSMEmprunts
{
    public class GearInfo
    {
        public Gear Gear { get; set; }
        public bool Borrowed { get; set; }

        public string Name
        {
            get
            {
                var converter = new GearTypeToStringConverter();
                return converter.Convert(Gear.Type, typeof(string), null, null) + " " + Gear.Name;
            }
        }
    }

    public class ReturnInfo : BindableBase
    {
        public Borrowing Borrowing { get; set; }
        public string Comment { get; set; }
    }

    public class ReturnViewModel : BindableBase, IDisposable
    {
        private readonly Context _Context;

        public ObservableCollection<ReturnInfo> ClosingBorrowings { get; } = new ObservableCollection<ReturnInfo>();

        public ICollectionView Gears { get; }
  

        public ReturnViewModel()
        {
            SelectGearCommand = new DelegateCommand<GearInfo>(SelectGearCmd);
            ValidateCommand = new DelegateCommand(ValidateCmd, CanValidateCmd);
            CancelCommand = new DelegateCommand(GoBackToHomeView);

            _Context = ContextFactory.OpenContext();

            var gears = (from gear in _Context.Gears
                         let borrowed = gear.Borrowings.Any(e => e.State == BorrowingState.Open)
                         select new GearInfo { Gear = gear, Borrowed = borrowed }).OrderByDescending(e=>e.Borrowed)
                         .ToList();

            Gears = CollectionViewSource.GetDefaultView(gears);
            Gears.Filter = (item) =>
            {
                var gearInfo = (GearInfo)item;
                if (ClosingBorrowings.Any(e=>e.Borrowing.Gear==gearInfo.Gear))
                {
                    return false;
                }
                return true;
            };
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
                var valueLower = value.ToLowerInvariant();
                var matchingGear = _Context.Gears.FirstOrDefault(e => e.Name.ToLowerInvariant() == valueLower);
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

        public DelegateCommand ValidateCommand { get; }
        private void ValidateCmd()
        {
            _AutoValidateTicker?.Dispose();

            _Context.SaveChanges();
            GoBackToHomeView();
        }
        private bool CanValidateCmd() => ClosingBorrowings.Any();

        public DelegateCommand CancelCommand { get; }
        private void GoBackToHomeView()
        {
            MainWindowViewModel.Instance.CurrentPageViewModel = new HomeViewModel();
        }

        public DelegateCommand<GearInfo> SelectGearCommand { get; }
        public void SelectGearCmd(GearInfo info)
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

            SetProperty(ref _SelectedGearId, string.Empty, nameof(SelectedGearId));
            Gears.Refresh();
        }
    }
}
