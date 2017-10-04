using LSMEmprunts.Data;
using Mvvm;
using Mvvm.Commands;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace LSMEmprunts
{
    public class ReturnInfo : BindableBase
    {
        public Borrowing Borrowing { get; set; }
        public string Comment { get; set; }
    }

    public class ReturnViewModel : BindableBase, IDisposable
    {
        private readonly Context _Context;

        public ObservableCollection<ReturnInfo> ClosingBorrowings { get; } = new ObservableCollection<ReturnInfo>();

        public ReturnViewModel()
        {
            ValidateCommand = new DelegateCommand(ValidateCmd, CanValidateCmd);
            CancelCommand = new DelegateCommand(GoBackToHomeView);

            _Context = ContextFactory.OpenContext();
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
                    //check for double input of a given gear
                    if (ClosingBorrowings.Any(e => e.Borrowing.Gear == matchingGear))
                    {
                        var vm = new WarningWindowViewModel("Matériel déjà rendu");
                        MainWindowViewModel.Instance.Dialogs.Add(vm);
                        SetProperty(ref _SelectedGearId, string.Empty);
                        return;
                    }

                    var now = DateTime.Now;

                    var matchingBorrowing = _Context.Borrowings
                        .FirstOrDefault(e => e.Gear == matchingGear && e.State == BorrowingState.Open);

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
                            Gear = matchingGear,
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

                    SetProperty(ref _SelectedGearId, string.Empty);
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

        private CountDownTicker _AutoValidateTicker;
        public CountDownTicker AutoValidateTicker
        {
            get => _AutoValidateTicker;
            set => SetProperty(ref _AutoValidateTicker, value);
        }
    }
}
