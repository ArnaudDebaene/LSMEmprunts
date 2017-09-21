using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace LSMEmprunts
{
    public abstract class ProxyBase<WrappedType> : INotifyPropertyChanged
        where WrappedType:class
    {
        public readonly WrappedType WrappedElt; 

        protected ProxyBase(WrappedType inner)
        {
            WrappedElt = inner;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(Expression<Func<WrappedType, T>>selector, T value, [CallerMemberName] string propertyName=null)
        {
            var memberExpression = selector.Body as MemberExpression;
            if (memberExpression==null)
            {
                throw new InvalidEnumArgumentException("selector shall be a non static = property of WrappedType");
            }
            var propertyInfo = memberExpression.Member as PropertyInfo;
            if (propertyInfo==null || propertyInfo.GetMethod.IsStatic)
            {
                throw new InvalidEnumArgumentException("selector shall be a non static property of WrappedType");
            }

            var currentValue = propertyInfo.GetValue(WrappedElt);
            if (object.Equals(currentValue, value))
            {
                return false;
            }

            propertyInfo.SetValue(WrappedElt, value);
            RaisePropertyChanged(propertyName);
            return true;
        }

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
