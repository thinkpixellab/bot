using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using PixelLab.Common;
using PixelLab.Demo.Core;

namespace PixelLab.Wpf.Demo
{
    [DemoMetadata("TreeMapPanel", "Another example of a custom panel.")]
    public partial class TreeMapPanelPage : Page
    {
        public TreeMapPanelPage()
        {
            InitializeComponent();

            m_colors
              .Select(b => new Rectangle() { Fill = b, Margin = new Thickness(2) })
              .ForEach(r =>
              {
                  TreeMapPanel.SetArea(r, Util.Rnd.NextDouble());
                  m_treeMap.Children.Add(r);
              });
        }

        private readonly DemoCollection<SolidColorBrush> m_colors = DemoCollection<SolidColorBrush>.Create(
          App.DemoColors.Select(c => c.ToCachedBrush()).ToArray(), 20, 0, 40);
    }
}