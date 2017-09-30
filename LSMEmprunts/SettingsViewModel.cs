﻿using LSMEmprunts.Data;
using Mvvm;
using Mvvm.Commands;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace LSMEmprunts
{
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

            Users = new ObservableCollection<UserProxy>();
            foreach(var user in _Context.Users)
            {
                Users.Add(BuildProxy(user));
            }
            Gears = new ObservableCollection<GearProxy>();
            foreach (var gear in _Context.Gears)
            {
                Gears.Add(BuildProxy(gear));
            }
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
        private bool CanValidateCmd() => _IsDirty && ! HasErrors;

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

        public bool HasErrors => Gears.Any(e => e.HasErrors) || Users.Any(e => e.HasErrors);

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
            var proxy = new UserProxy(u, Users);
            proxy.PropertyChanged += (s, e) => SetDirty();
            proxy.ErrorsChanged += (s, e) => ValidateCommand.RaiseCanExecuteChanged();
            return proxy;
        }

        private GearProxy BuildProxy(Gear g)
        {
            var proxy = new GearProxy(g, Gears);
            proxy.PropertyChanged += (s, e) => SetDirty();
            proxy.ErrorsChanged += (s, e) => ValidateCommand.RaiseCanExecuteChanged();
            return proxy;
        }
    }
}