using LSMEmprunts.Data;
using Mvvm;
using Mvvm.Commands;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace LSMEmprunts
{
    public class ReturnViewModel : BindableBase, IDisposable
    {
        private readonly Context _Context;

        public ObservableCollection<Borrowing> ClosingBorrowings { get; } = new ObservableCollection<Borrowing>();

        public ReturnViewModel()
        {
            ValidateCommand = new DelegateCommand(ValidateCmd, CanValidateCmd);
            CancelCommand = new DelegateCommand(CancelCmd);

            _Context = ContextFactory.OpenContext();
        }

        public void Dispose()
        {
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
                    var now = DateTime.Now;

                    var matchingBorrowing = _Context.Borrowings
                        .FirstOrDefault(e => e.Gear == matchingGear && e.State == BorrowingState.Open);

                    if (matchingBorrowing!=null)
                    {
                        matchingBorrowing.ReturnTime = now;
                        matchingBorrowing.State = BorrowingState.GearReturned;
                        ClosingBorrowings.Add(matchingBorrowing);
                    }
                    else
                    {
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
                        //TODO : display warning
                        ClosingBorrowings.Add(borrowing);
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
            _Context.SaveChanges();
            MainWindowViewModel.Instance.CurrentPageViewModel = new HomeViewModel();
        }
        private bool CanValidateCmd() => ClosingBorrowings.Any();        

        public DelegateCommand CancelCommand { get; }
        private void CancelCmd()
        {
            MainWindowViewModel.Instance.CurrentPageViewModel = new HomeViewModel();
        }
    }
}
