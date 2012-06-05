using System.Windows.Controls;
using PixelLab.Common;
using PixelLab.Demo.Core;

namespace PixelLab.Wpf.Demo
{
    [DemoMetadata("Zap Scroller", "An example of the union between custom elements, custom animation, and data binding.")]
    public partial class ZapPage : Page
    {
        public ZapPage()
        {
            InitializeComponent();

            m_tabItemColors.DataContext = m_strings;
        }

        private readonly DemoCollection<string> m_strings =
            DemoCollection<string>.Create(new string[] { "red", "orange", "yellow", "green", "blue", "violet" }, 6, 0, 12);
    }
}
