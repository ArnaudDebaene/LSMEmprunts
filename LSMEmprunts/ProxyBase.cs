using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace LSMEmprunts
{
    public abstract class ProxyBase<WrappedType> : INotifyPropertyChanged, INotifyDataErrorInfo, IEditableObject
        where WrappedType : class
    {
        public readonly WrappedType WrappedElt;

        protected ProxyBase(WrappedType inner)
        {
            WrappedElt = inner;
        }

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(Expression<Func<WrappedType, T>> selector, T value, [CallerMemberName] string propertyName = null)
        {
            var memberExpression = selector.Body as MemberExpression;
            if (memberExpression == null)
            {
                throw new InvalidEnumArgumentException("selector shall be a non static = property of WrappedType");
            }
            var propertyInfo = memberExpression.Member as PropertyInfo;
            if (propertyInfo == null || propertyInfo.GetMethod.IsStatic)
            {
                throw new InvalidEnumArgumentException("selector shall be a non static property of WrappedType");
            }

            var currentValue = propertyInfo.GetValue(WrappedElt);
            if (Equals(currentValue, value))
            {
                return false;
            }

            propertyInfo.SetValue(WrappedElt, value);
            RaisePropertyChanged(propertyName);
            return true;
        }

        protected bool SetProperty<T>(ref T backingField, T value, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(backingField, value))
            {
                backingField = value;
                RaisePropertyChanged(propertyName);
                return true;
            }
            return false;
        }

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region INotifyDataErrorInfo implementation
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return null;
            }
            _ErrorsPerProperty.TryGetValue(propertyName, out List<object> retval);
            return retval;
        }

        public bool HasErrors => _ErrorsPerProperty.Any(e => e.Value.Count > 0);

        private readonly Dictionary<string, List<object>> _ErrorsPerProperty = new Dictionary<string, List<object>>();

        protected void AddError(string propertyName, object error)
        {
            if (_ErrorsPerProperty.TryGetValue(propertyName, out List<object> errors) == false)
            {
                errors = new List<object>();
                _ErrorsPerProperty.Add(propertyName, errors);
            }
            errors.Add(error);
            RaiseErrorsChanged(propertyName);
        }

        protected void ClearErrors(string propertyName)
        {
            _ErrorsPerProperty.Remove(propertyName);
            RaiseErrorsChanged(propertyName);
        }

        protected void RemoveError(string propertyName, object error)
        {
            if (_ErrorsPerProperty.TryGetValue(propertyName, out List<object> errors))
            {
                errors.Remove(error);
                RaiseErrorsChanged(propertyName);
            }
        }

        private void RaiseErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
        #endregion

        #region IEditableObject implementation
        private Memento<WrappedType> _Memento;

        public void BeginEdit()
        {
            if (_Memento==null)
            {
                _Memento = new Memento<WrappedType>(WrappedElt);
            }
        }

        public void EndEdit()
        {
            _Memento = null;
        }

        public void CancelEdit()
        {
            if (_Memento!=null)
            {
                _Memento.Restore(WrappedElt);
                foreach(var propName in _Memento.SavedProperties)
                {
                    RaisePropertyChanged(propName);
                }
                _Memento = null;
            }
        }
        #endregion
    }
}
