using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace PixelLab.Wpf.Demo {
  public partial class IntroductionPage : Page {
    public IntroductionPage() {
      InitializeComponent();

      AddHandler(Hyperlink.RequestNavigateEvent, new RequestNavigateEventHandler(
        (sender, e) => {
          var uri = e.Uri.ToString();
          if (uri.StartsWith("http://")) {
            Process.Start(uri);
            e.Handled = true;
          }
        }));
    }
  }
}
