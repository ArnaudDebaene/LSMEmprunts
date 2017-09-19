using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Input;
using System.Windows.Data;

namespace LSMEmprunts
{
    public class BindOnEnterBehavior : Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewKeyDown += OnPreviewKeyDown;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewKeyDown += OnPreviewKeyDown;
            base.OnDetaching();
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key==Key.Enter)
            {
                var bindingExpression = BindingOperations.GetBindingExpression(AssociatedObject, TextBox.TextProperty);
                bindingExpression?.UpdateSource();

                e.Handled = true;
            }
        }
    }
}
