using System.Diagnostics;
using System.Windows.Controls;

namespace PixelLab.Wpf.Demo {
  public partial class MainPage : Page {
    public MainPage() {
      InitializeComponent();

      m_contentFrame.Navigating += (sender, e) => {
        var uri = e.Uri.ToString();
        if (uri.StartsWith("http://")) {
          Process.Start(uri);
          e.Cancel = true;
        }
      };

    }
  }
}
