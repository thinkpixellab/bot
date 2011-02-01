using System.Diagnostics;
using System.Windows.Controls;
using PixelLab.Demo.Core;

namespace PixelLab.Wpf.Demo
{
    public partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            m_sampleList.ItemsSource = DemoMetadata.GetDemos(typeof(MainPage).Assembly, "Introduction");

            m_contentFrame.Navigating += (sender, e) =>
            {
                if (e.Uri != null)
                {
                    var uri = e.Uri.ToString();
                    if (uri.StartsWith("http://"))
                    {
                        Process.Start(uri);
                        e.Cancel = true;
                    }
                }
            };
        }
    }
}
