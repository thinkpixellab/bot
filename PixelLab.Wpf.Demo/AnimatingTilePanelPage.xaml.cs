using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PixelLab.Common;

namespace PixelLab.Wpf.Demo {
  public partial class AnimatingTilePanelPage : Page {
    public AnimatingTilePanelPage() {
      InitializeComponent();

      this.LayoutUpdated += delegate(object sender, EventArgs e) {
        if (Panel == null) {
          Panel = TreeHelpers.FindChild<AnimatingTilePanel>(m_itemsControl);
        }
      };

      m_itemsControl.ItemsSource = m_colors;
      m_stackPanelCommands.DataContext = m_colors;

    }

    public static readonly DependencyProperty PanelProperty =
        DependencyProperty.Register("Panel", typeof(AnimatingTilePanel), typeof(AnimatingTilePanelPage));

    public AnimatingTilePanel Panel {
      get {
        return (AnimatingTilePanel)GetValue(PanelProperty);
      }
      set {
        SetValue(PanelProperty, value);
      }
    }

    private readonly DemoCollection<Brush> m_colors = DemoCollection<Brush>.Create(
      App.DemoColors.Select(c => c.ToCachedBrush()), 48, 0, 96);
  }
}
