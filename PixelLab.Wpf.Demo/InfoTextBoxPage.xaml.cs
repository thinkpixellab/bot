using System.Windows.Controls;
using PixelLab.SL.Demo.Core;

namespace PixelLab.Wpf.Demo {
  [DemoMetadata("Info TextBox", "When should I build a custom control? This is a question I get a lot. I think InfoTextBox is a great example of a where a custom control adds a lot of value. You'll notice that I didn't start from scratch. I simply added a couple of properties to the existing TextBox. The properties I add are pretty boring, actually, just HasText (bool) and TextBoxInfo (string). The magic comes when I leverage these new properties in a new ControlTemplate for InfoTextBox. I think you'll agree that the results are pretty cool.")]
  public partial class InfoTextBoxPage : Page {
    public InfoTextBoxPage() {
      InitializeComponent();
    }
  }
}
