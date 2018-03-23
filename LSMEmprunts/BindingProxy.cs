using System.Windows;

namespace LSMEmprunts
{
    /// <summary>
    /// a proxy helper to bind smoething that is not in the WPF visual tree (such as a DatGrid column for example)
    /// </summary>
    public class BindingProxy : Freezable
    {
        public static readonly DependencyProperty DataProperty = DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));
        public object Data
        {
            get => GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }
        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }
    }
}
