using LSMEmprunts.Data;
using System.Collections.Generic;
using System.Linq;

namespace LSMEmprunts
{
    public class GearProxy : ProxyBase<Gear>
    {
        private readonly IEnumerable<GearProxy> _Collection;

        public GearProxy(Gear data, IEnumerable<GearProxy> collection)
            : base(data)
        {
            _Collection = collection;
            EvaluateNameValidity();
            EvaluateBarCodeValidity();
        }

        public string Name
        {
            get => WrappedElt.Name;
            set
            {
                SetProperty(e => e.Name, value);
                EvaluateNameValidity();
            }
        }

        private void EvaluateNameValidity()
        {
            ClearErrors(nameof(Name));
            if (string.IsNullOrEmpty(Name))
            {
                AddError(nameof(Name), "Nom requis");
            }
            if (_Collection.Any(e => e != this && e.Name == Name))
            {
                AddError(nameof(Name), "Le nom doit être unique");
            }
        }

        public GearType Type
        {
            get => WrappedElt.Type;
            set => SetProperty(e => e.Type, value);
        }

        public string BarCode
        {
            get => WrappedElt.BarCode;
            set
            {
                SetProperty(e => e.BarCode, value);
                EvaluateBarCodeValidity();
            }
        }

        private void EvaluateBarCodeValidity()
        {
            ClearErrors(nameof(BarCode));
            if (string.IsNullOrEmpty(BarCode))
            {
                AddError(nameof(BarCode), "Code barre requis");
            }
            if (_Collection.Any(e => e != this && e.BarCode == BarCode))
            {
                AddError(nameof(BarCode), "le code barre doit être unique");
            }
        }
    }
}
