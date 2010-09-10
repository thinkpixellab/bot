using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using PixelLab.Common;

namespace PixelLab.SL {
  public class SpriteElement : UserControl {
    public SpriteElement() {
      m_transform = new TranslateTransform();
      m_brush = new ImageBrush() { Stretch = Stretch.None, AlignmentX = AlignmentX.Left, AlignmentY = AlignmentY.Top, Transform = m_transform };

      this.Content = new Rectangle() { Fill = m_brush };
    }

    #region DPs

    public bool IsPressed {
      get { return (bool)GetValue(IsPressedProperty); }
      set { SetValue(IsPressedProperty, value); }
    }

    public static readonly DependencyProperty IsPressedProperty = DependencyPropertyHelper.Register<SpriteElement, bool>("IsPressed", (element, newValue, oldValue) => element.updateTransform());

    public ImageSource ImageSource {
      get { return (ImageSource)GetValue(ImageSourceProperty); }
      set { SetValue(ImageSourceProperty, value); }
    }

    public static readonly DependencyProperty ImageSourceProperty =
      DependencyPropertyHelper.Register<SpriteElement, ImageSource>("ImageSource", (element, newVal, oldVal) => {
        element.m_brush.ImageSource = newVal;
        element.updateTransform();
      });

    public int NextOffsetX {
      get { return (int)GetValue(NextOffsetXProperty); }
      set { SetValue(NextOffsetXProperty, value); }
    }

    public static readonly DependencyProperty NextOffsetXProperty =
      DependencyPropertyHelper.Register<SpriteElement, int>("NextOffsetXProperty", onValueChange);

    public int NextOffsetY {
      get { return (int)GetValue(NextOffsetYProperty); }
      set { SetValue(NextOffsetYProperty, value); }
    }

    public static readonly DependencyProperty NextOffsetYProperty =
      DependencyPropertyHelper.Register<SpriteElement, int>("NextOffsetYProperty", onValueChange);

    public int SpriteIndex {
      get { return (int)GetValue(SpriteIndexProperty); }
      set { SetValue(SpriteIndexProperty, value); }
    }

    public static readonly DependencyProperty SpriteIndexProperty =
      DependencyPropertyHelper.Register<SpriteElement, int>("SpriteIndex", onValueChange);

    public int PressedOffsetY {
      get { return (int)GetValue(PressedOffsetYProperty); }
      set { SetValue(PressedOffsetYProperty, value); }
    }

    public static readonly DependencyProperty PressedOffsetYProperty =
      DependencyPropertyHelper.Register<SpriteElement, int>("PressedOffset", onValueChange);
    #endregion

    private void updateTransform() {
      m_transform.X = -NextOffsetX * SpriteIndex;
      m_transform.Y = -NextOffsetY * SpriteIndex;
      if (IsPressed) {
        m_transform.Y -= PressedOffsetY;
      }
    }

    private static void onValueChange(SpriteElement element, int newValue, int oldValue) {
      element.updateTransform();
    }

    private readonly ImageBrush m_brush;
    private readonly TranslateTransform m_transform;
  }
}
