using LSMEmprunts.Data;
using Mvvm;
using Mvvm.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace LSMEmprunts
{
    public class GearBorrowInfo
    {
        public Gear Gear { get; set; }
        public bool Available { get; set; }

        public string Name
        {
            get
            {
                var converter = new GearTypeToStringConverter();
                return converter.Convert(Gear.Type, typeof(string), null, null) + " " + Gear.Name;
            }
        }       
    }

    /// <summary>
    /// comparer for ordering of gears list : by type then by name (if name is a number)
    /// </summary>
    class GearComparer : IComparer
    {
        private static GearType[] Types = { GearType.Tank, GearType.Regulator, GearType.BCD };

        public int Compare(object x, object y)
        {
            var xx = (GearBorrowInfo)x;
            var yy = (GearBorrowInfo)y;

            if (xx.Gear.Type != yy.Gear.Type)
            {
                return Array.IndexOf(Types, xx.Gear.Type) - Array.IndexOf(Types, yy.Gear.Type);
            }

            bool successParseNameX = int.TryParse(xx.Gear.Name, out var xxName);
            bool successParseNameY = int.TryParse(yy.Gear.Name, out var yyName);
            if (successParseNameX && successParseNameY)
            {
                return xxName - yyName;
            }
            else if (successParseNameX)
            {
                return -1;
            }
            else if (successParseNameY)
            {
                return 1;
            }
            else
            {
                return string.Compare(xx.Gear.Name, yy.Gear.Name);
            }
        }
    }

    public class BorrowViewModel : BindableBase, IDisposable
    {
        private readonly Context _Context;

        private readonly List<User> _UsersList;

        public ICollectionView Users { get; }
        public ICollectionView Gears { get; }

        private User _SelectedUser;
        public User SelectedUser
        {
            get => _SelectedUser;
            set
            {
                var wasChanged = SetProperty(ref _SelectedUser, value);
                ValidateCommand.RaiseCanExecuteChanged();

                if (wasChanged && value != null)
                {
                    _SelectedUserText = null;
                    OnPropertyChanged(nameof(SelectedUserText));
                    Users.Refresh();
                    UserSelected = true;
                    StartOrResetValidateTicker();
                    GearInputFocused = true; //move focus to gear input.
                    
                }
            }
        }

        private bool _UserSelected = false;
        public bool UserSelected
        {
            get => _UserSelected;
            set => SetProperty(ref _UserSelected, value);
        }

        public BorrowViewModel()
        {
            _Context = ContextFactory.OpenContext();

            ValidateCommand = new DelegateCommand(ValidateCmd, CanValidateCmd);
            CancelCommand = new DelegateCommand(GoBackToHomeView);
            SelectGearCommand = new DelegateCommand<GearBorrowInfo>(SelectGearCmd);

            _UsersList = _Context.Users.ToList();
            Users = CollectionViewSource.GetDefaultView(_UsersList);
            Users.Filter = (item) =>
            {
                if (string.IsNullOrEmpty(_SelectedUserText))
                {
                    return true;
                }
                return ((User)item).Name.StartsWith(_SelectedUserText, StringComparison.CurrentCultureIgnoreCase);
            };

            var gears = (from gear in _Context.Gears
                    let borrowed = gear.Borrowings.Any(e => e.State == BorrowingState.Open)
                    select new GearBorrowInfo { Gear = gear, Available = !borrowed }).OrderByDescending(e => e.Available)
                .ToList();
            Gears = CollectionViewSource.GetDefaultView(gears);
            ((ListCollectionView)Gears).CustomSort = new GearComparer();
            Gears.Filter = (item) =>
            {
                var gearInfo = (GearBorrowInfo) item;
                if (BorrowedGears.Any(e => e == gearInfo.Gear))
                {
                    return false;
                }
                return true;
            };
        }

        public void Dispose()
        {
            AutoValidateTicker?.Dispose();
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
                if (SetProperty(ref _SelectedGearId, value))
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() => AnalyzeSelectedGearId(value)));
                }
            }
                
        }

        private async void AnalyzeSelectedGearId(string value)
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
                await SelectGearToBorrow(matchingGear);
                SetProperty(ref _SelectedGearId, string.Empty, nameof(SelectedGearId));
            }
        }

        public DelegateCommand ValidateCommand { get; }
        private async void ValidateCmd()
        {
            AutoValidateTicker?.Dispose();

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

        private CountDownTicker _AutoValidateTicker;
        public CountDownTicker AutoValidateTicker{
            get => _AutoValidateTicker;
            set => SetProperty(ref _AutoValidateTicker, value);
        }

        private void StartOrResetValidateTicker()
        {
            if (AutoValidateTicker == null && SelectedUser != null)
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
                AutoValidateTicker?.Reset();
            }
        }

        private bool _UserInputFocused = true;
        public bool UserInputFocused
        {
            get => _UserInputFocused;
            set => SetProperty(ref _UserInputFocused, value);
        }

        private bool _GearInputFocused;
        public bool GearInputFocused
        {
            get => _GearInputFocused;
            set => SetProperty(ref _GearInputFocused, value);
        }

        public DelegateCommand<GearBorrowInfo> SelectGearCommand { get; }
        public async void SelectGearCmd(GearBorrowInfo info)
        {
            await SelectGearToBorrow(info.Gear);
        }

        private async Task SelectGearToBorrow(Gear gear)
        {
            //check for double input of a given gear
            if (BorrowedGears.Contains(gear))
            {
                var vm = new WarningWindowViewModel("Matériel déjà emprunté");
                MainWindowViewModel.Instance.Dialogs.Add(vm);
                return;
            }

            //try to find a still open borrowing of the same gear - force close it if found
            var existingBorrowing = _Context.Borrowings.FirstOrDefault(e => e.GearId == gear.Id && e.State == BorrowingState.Open);
            if (existingBorrowing != null)
            {
                var confirmDlg = new ConfirmWindowViewModel("Ce matériel est déjà noté comme emprunté. L'emprunt en cours sera fermé. Etes vous sûr(e)?");
                MainWindowViewModel.Instance.Dialogs.Add(confirmDlg);
                if (await confirmDlg.Result == false)
                {
                    return;
                }
                _BorrowingsToForceClose.Add(existingBorrowing);
            }

            BorrowedGears.Add(gear);

            ValidateCommand.RaiseCanExecuteChanged();

            //start auto close ticker if required
            StartOrResetValidateTicker();

            Gears.Refresh();
            GearInputFocused = true; //move focus to gear input.
        }
    }
}
