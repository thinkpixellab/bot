using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace PixelLab.Wpf.Demo
{
    public partial class IntroductionPage : Page
    {
        public IntroductionPage()
        {
            InitializeComponent();
        }

        private void link_navigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
            e.Handled = true;
        }
    }
}
