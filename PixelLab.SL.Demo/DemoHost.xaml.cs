using System.Windows.Controls;
using PixelLab.Demo.Core;

namespace PixelLab.SL.Demo {
  public partial class DemoHost : UserControl {
    public DemoHost() {
      InitializeComponent();

      m_items.ItemsSource = DemoMetadata.GetDemos(typeof(DemoHost).Assembly, "Welcome");

      m_items.SelectionChanged += (sender, args) => {
        var item = m_items.SelectedItem as DemoMetadata;
        if (item != null) {
          m_container.Child = item.CreateElement();
        }

      };

      m_items.SelectedIndex = 0;
    }
  }
}
