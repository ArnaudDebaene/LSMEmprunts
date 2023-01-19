using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LSMEmprunts
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }
    }

    public class GearSizeTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TankSizeTemplate { get; set; }

        public DataTemplate BCDSizeTemplate { get; set; }

        public DataTemplate EmptyTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item==null)
            {
                return null;
            }

            var proxy = (GearProxy)item;
            switch (proxy.Type)
            {
                case Data.GearType.Tank:
                    return TankSizeTemplate;
                case Data.GearType.BCD:
                    return BCDSizeTemplate;
                default:
                    return EmptyTemplate;
            };
        }
    }
}
