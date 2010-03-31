using System.Windows.Controls;
using PixelLab.Wpf;

namespace PixelLab.Wpf.Demo
{
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
