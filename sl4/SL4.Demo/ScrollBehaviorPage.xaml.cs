using System.Linq;
using System.Windows.Controls;
using PixelLab.Common;
using PixelLab.Demo.Core;

namespace PixelLab.SL.Demo
{
    [DemoMetadata("ScrollBehavior", "A behavior to attach to a ScrollViewer to expose scroll direction commands.")]
    public partial class ScrollBehaviorPage : UserControl
    {
        public ScrollBehaviorPage()
        {
            InitializeComponent();

            _demoListBox.ItemsSource = Enumerable.Range(1, 10).Select(i => "Item {0}".DoFormat(i)).ToList();
        }
    }
}
