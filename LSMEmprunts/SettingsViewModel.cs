using LSMEmprunts.Data;
using Mvvm;
using Mvvm.Commands;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace LSMEmprunts
{
    public class GearProxy : ProxyBase<Gear>
    {
        public GearProxy(Gear data)
            :base(data)
        {}

        public string Name
        {
            get => WrappedElt.Name;
            set => SetProperty(e => e.Name, value);
        }

        public GearType Type
        {
            get => WrappedElt.Type;
            set => SetProperty(e => e.Type, value);
        }

        public string BarCode
        {
            get => WrappedElt.BarCode;
            set => SetProperty(e => e.BarCode, value);
        }
    }

    public class UserProxy : ProxyBase<User>
    {
        public UserProxy(User data)
            :base(data)
        { }

        public string Name
        {
            get => WrappedElt.Name;
            set => SetProperty(e => e.Name, value);
        }

        public string LicenceScanId
        {
            get => WrappedElt.LicenceScanId;
            set => SetProperty(e => e.LicenceScanId, value);
        }
    }

    public class SettingsViewModel : BindableBase, IDisposable
    {
        private readonly Context _Context;

        public SettingsViewModel()
        {
            ValidateCommand = new DelegateCommand(ValidateCmd, CanValidateCmd);
            CancelCommand = new DelegateCommand(GoBackToHomeView);

            CreateGearCommand = new DelegateCommand(CreateGear);
            DeleteGearCommand = new DelegateCommand<GearProxy>(DeleteGear);
            GearHistoryCommand = new DelegateCommand<GearProxy>(ShowGearHistory);

            CreateUserCommand = new DelegateCommand(CreateUser);
            DeleteUserCommand = new DelegateCommand<UserProxy>(DeleteUser);
            UserHistoryCommand = new DelegateCommand<UserProxy>(ShowUserHistory);
            

            _Context = ContextFactory.OpenContext();

            Users = new ObservableCollection<UserProxy>(_Context.Users.AsEnumerable().Select(u => BuildProxy(u)));
            Gears = new ObservableCollection<GearProxy>(_Context.Gears.AsEnumerable().Select(g => BuildProxy(g)));
        }

        public void Dispose()
        {
            _Context.Dispose();
        }

        public ObservableCollection<UserProxy> Users {get;}
        public ObservableCollection<GearProxy> Gears { get; }

        public DelegateCommand ValidateCommand { get; }
        private void ValidateCmd()
        {
            _Context.SaveChanges();
            GoBackToHomeView();
        }
        private bool CanValidateCmd() => _IsDirty;

        public DelegateCommand CancelCommand { get; }
        private void GoBackToHomeView()
        {
            MainWindowViewModel.Instance.CurrentPageViewModel = new HomeViewModel();
        }

        private bool _IsDirty = false;
        private void SetDirty()
        {
            _IsDirty = true;
            ValidateCommand.RaiseCanExecuteChanged();
        }

        public DelegateCommand CreateUserCommand { get; }
        private void CreateUser()
        {
            var user = new User();
            _Context.Users.Add(user);
            Users.Add(BuildProxy(user));
        }

        public DelegateCommand<UserProxy> DeleteUserCommand { get; }
        private void DeleteUser(UserProxy u)
        {
            _Context.Users.Remove(u.WrappedElt);
            Users.Remove(u);
            SetDirty();
        }

        public DelegateCommand<UserProxy> UserHistoryCommand { get; }
        private async void ShowUserHistory(UserProxy u)
        {
            var vm = new UserHistoryDlgViewModel(u.WrappedElt, _Context);
            MainWindowViewModel.Instance.Dialogs.Add(vm);
            if (await vm.HasModifiedData)
            {
                SetDirty();
            }
        }
        
        public DelegateCommand CreateGearCommand { get; }
        private void CreateGear()
        {
            var gear = new Gear();
            _Context.Gears.Add(gear);
            Gears.Add(BuildProxy(gear));
        }

        public DelegateCommand<GearProxy> DeleteGearCommand { get; }
        private void DeleteGear(GearProxy g)
        {
            _Context.Gears.Remove(g.WrappedElt);
            Gears.Remove(g);
            SetDirty();
        }

        public DelegateCommand<GearProxy> GearHistoryCommand { get; }
        private async void ShowGearHistory(GearProxy g)
        {
            var vm = new GearHistoryDlgViewModel(g.WrappedElt, _Context);
            MainWindowViewModel.Instance.Dialogs.Add(vm);
            if (await vm.HasModifiedData)
            {
                SetDirty();
            }
        }

        private UserProxy BuildProxy(User u)
        {
            var proxy = new UserProxy(u);
            proxy.PropertyChanged += (s, e) => SetDirty();
            return proxy;
        }

        private GearProxy BuildProxy(Gear g)
        {
            var proxy = new GearProxy(g);
            proxy.PropertyChanged += (s, e) => SetDirty();
            return proxy;
        }
    }
}
