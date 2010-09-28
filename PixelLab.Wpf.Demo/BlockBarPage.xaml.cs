using System.Windows.Controls;
using PixelLab.SL.Demo.Core;

namespace PixelLab.Wpf.Demo {
  [DemoMetadata("BlockBar Control", "Sometimes you just want a control that does its own rendering without worrying about a template. This is an example of a control that supports all of the standard data binding tricks and also draws itself. A baseclass is used to centralize common infrastructure.")]
  public partial class BlockBarPage : Page {
    public BlockBarPage() {
      InitializeComponent();
    }
  }
}
