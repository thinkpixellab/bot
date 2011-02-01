using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PixelLab.Common;
using PixelLab.Demo.Core;

namespace PixelLab.Wpf.Demo
{
    [DemoMetadata("Animating Tile Panel", "This is a demonstration of two concepts: custom Panel creation and use of the Composition.Render event to do animations.")]
    public partial class AnimatingTilePanelPage : Page
    {
        public AnimatingTilePanelPage()
        {
            InitializeComponent();

            this.LayoutUpdated += delegate(object sender, EventArgs e)
            {
                if (Panel == null)
                {
                    Panel = m_itemsControl.FirstVisualDescendentOfType<AnimatingTilePanel>();
                }
            };

            m_itemsControl.ItemsSource = m_colors;
            m_stackPanelCommands.DataContext = m_colors;
        }

        public static readonly DependencyProperty PanelProperty =
            DependencyProperty.Register("Panel", typeof(AnimatingTilePanel), typeof(AnimatingTilePanelPage));

        public AnimatingTilePanel Panel
        {
            get
            {
                return (AnimatingTilePanel)GetValue(PanelProperty);
            }
            set
            {
                SetValue(PanelProperty, value);
            }
        }

        private readonly DemoCollection<SolidColorBrush> m_colors = DemoCollection<SolidColorBrush>.Create(
          App.DemoColors.Select(c => c.ToCachedBrush()).ToArray(), 48, 0, 96);
    }
}
