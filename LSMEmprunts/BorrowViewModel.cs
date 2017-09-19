using LSMEmprunts.Data;
using Mvvm;
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
            }
        }

        public BorrowViewModel()
        {
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
                if (matchingUsersByName.Count==1)
                {
                    System.Diagnostics.Debug.WriteLine("User input - found matching user by name");
                    SelectedUser = matchingUsersByName[0];
                    return;
                }
                
                var matchingUserByLicence = Users.FirstOrDefault(e => e.LicenceScanId == value);
                if (matchingUserByLicence!=null)
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

        private bool _UserSet = false;
        public bool UserSet
        {
            get => _UserSet;
            set => SetProperty(ref _UserSet, value);
        }

        public ObservableCollection<Gear> BorrowedGears { get; } = new ObservableCollection<Gear>();
        
    }
}
