using System.Windows;
using System.Windows.Controls;
using Microsoft.Practices.Prism.Commands;
using PixelLab.Demo.Core;

namespace PixelLab.SL.Demo
{
    [DemoMetadata("DoubleClickBehavior")]
    public partial class DoubleClickBehaviorPage : UserControl
    {
        public DoubleClickBehaviorPage()
        {
            DataContext = new DelegateCommand<string>((param) =>
            {
                MessageBox.Show(param, "The double click command has been fired.", MessageBoxButton.OK);
            });

            InitializeComponent();
        }
    }
}
