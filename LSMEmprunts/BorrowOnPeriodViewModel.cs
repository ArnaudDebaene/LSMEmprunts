using CommunityToolkit.Mvvm.Input;
using LSMEmprunts.Data;
using MvvmDialogs;
using MvvmDialogs.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace LSMEmprunts
{
    public sealed class BorrowInfo
    {
        public string User { get; set; }
        public GearType GearType { get; set; }
        public string Gear { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class BorrowOnPeriodViewModel : ModalDialogViewModelBase
    {
        private readonly Context _Context;

        public BorrowOnPeriodViewModel(Context context)
        {
            _Context = context;

            ExportCsvCommand = new RelayCommand(ExportCsv);

            var now = DateTime.Now;
            _ToDateTime = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Local);
            _FromDateTime = ToDateTime - TimeSpan.FromDays(7);
            FillBorrows();
        }

        #region Mvvm properties

        private DateTime _FromDateTime;

        [LesserThan(nameof(ToDateTime))]
        public DateTime FromDateTime
        {
            get => _FromDateTime;
            set
            {
                if (SetProperty(ref _FromDateTime, value))
                {
                    ValidateAllProperties();
                    FillBorrows();
                }
            }
        }

        private DateTime _ToDateTime;

        [GreaterThan(nameof(FromDateTime))]
        public DateTime ToDateTime
        {
            get => _ToDateTime;
            set
            {
                if (SetProperty(ref _ToDateTime, value))
                {
                    ValidateAllProperties();
                    FillBorrows();
                }
            }
        }

        public ObservableCollection<BorrowInfo> Borrows { get; } = new();

        private bool _InclusivePeriods = false;

        public bool InclusivePeriods
        {
            get => _InclusivePeriods;
            set
            {
                if (SetProperty(ref _InclusivePeriods, value))
                {
                    FillBorrows();
                }
            }
        }

        #endregion Mvvm properties

        private void FillBorrows()
        {
            Borrows.Clear();

            if (FromDateTime < ToDateTime)
            {
                IQueryable<BorrowInfo> query;
                if (InclusivePeriods)
                {
                    query = _Context.Borrowings.Where(e => e.BorrowTime >= FromDateTime && e.ReturnTime <= ToDateTime).Select(e => new BorrowInfo
                    {
                        FromDate = e.BorrowTime,
                        ToDate = e.ReturnTime,
                        User = e.User.Name,
                        Gear = e.Gear.Name,
                        GearType = e.Gear.Type,
                    });
                }
                else
                {
                    query = _Context.Borrowings.Where(e =>
                    (e.BorrowTime < FromDateTime && e.ReturnTime > FromDateTime && e.ReturnTime <= ToDateTime) ||
                    (e.BorrowTime >= FromDateTime && e.BorrowTime < ToDateTime && e.ReturnTime > ToDateTime) ||
                    (e.BorrowTime >= FromDateTime && e.ReturnTime <= ToDateTime))
                        .Select(e => new BorrowInfo
                        {
                            FromDate = e.BorrowTime,
                            ToDate = e.ReturnTime,
                            User = e.User.Name,
                            Gear = e.Gear.Name,
                            GearType = e.Gear.Type,
                        });
                }

                foreach (var item in query)
                {
                    Borrows.Add(item);
                }
            }
        }

        #region Commands
        public RelayCommand ExportCsvCommand { get; }

        private async void ExportCsv()
        {
            var vm = new SaveFileDialogViewModel
            {
                Filter = "(*.csv)|*.csv"
            };
            MainWindowViewModel.Instance.Dialogs.Add(vm);
            if (await vm.Completion)
            {
                using var writer = new StreamWriter(vm.FileName, false, Encoding.UTF8);
                writer.WriteLine("Nom;Type;Matériel;Date d'emprunt;Date de retour");
                var converter = new GearTypeToStringConverter();
                foreach (var bi in Borrows)
                {
                    writer.WriteLine($"{bi.User};{converter.Convert(bi.GearType, typeof(string), null, Thread.CurrentThread.CurrentUICulture)};{bi.Gear};{bi.FromDate};{bi.ToDate};");
                }
            }
        }
        #endregion

    }
}