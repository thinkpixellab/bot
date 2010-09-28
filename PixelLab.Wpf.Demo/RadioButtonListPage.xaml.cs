using System.Windows.Controls;
using PixelLab.SL.Demo.Core;

namespace PixelLab.Wpf.Demo {
  [DemoMetadata("RadioButtonList", "I've received a lot of questions about RadioButtonList (RBL). Why don't we have it as a control? The decision was a long and interesting one. At the end of the day we decided it was just too easy to build an RBL by styling a ListBox. (We had a whole host of other issues, too, but I won't bore you with them.) The power of using styling is that you can take an ordinary, data-bound ListBox and make it look like an RBL. Pretty cool.")]
  public partial class RadioButtonListPage : Page {
    public RadioButtonListPage() {
      InitializeComponent();
    }
  }
}
