using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PixelLab.SL {

  // TODO: really need to wire up change handlers for properties
  // TODO: Need to add PressedOffsetX property
  [TemplatePart(Name="ImageHolder", Type=typeof(Rectangle))]
  public class SpriteButton : Button {
    public SpriteButton() {
      DefaultStyleKey = typeof(SpriteButton);

      m_transform = new TranslateTransform();
      m_brush = new ImageBrush() { Stretch = Stretch.None, AlignmentX = AlignmentX.Left, AlignmentY = AlignmentY.Top, Transform = m_transform };
    }

    public override void OnApplyTemplate() {
      var child = GetTemplateChild("ImageHolder") as Rectangle;
      if (child != null) {
        child.Fill = m_brush;
        updateSize(child);
        updateTransform();
      }
      base.OnApplyTemplate();
    }

    protected override void OnIsPressedChanged(DependencyPropertyChangedEventArgs e) {
      updateTransform();
    }
    #region DPs
    public ImageSource ImageSource {
      get { return (ImageSource)GetValue(ImageSourceProperty); }
      set { SetValue(ImageSourceProperty, value); }
    }

    public static readonly DependencyProperty ImageSourceProperty =
        DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(SpriteButton),
        new PropertyMetadata(new PropertyChangedCallback((element, args) => {
          ((SpriteButton)element).m_brush.ImageSource = (ImageSource)args.NewValue;
        }))
        );

    public int SpriteWidth {
      get { return (int)GetValue(SpriteWidthProperty); }
      set { SetValue(SpriteWidthProperty, value); }
    }

    public static readonly DependencyProperty SpriteWidthProperty =
        DependencyProperty.Register("SpriteWidth", typeof(int), typeof(SpriteButton), null);

    public int SpriteHeight {
      get { return (int)GetValue(SpriteHeightProperty); }
      set { SetValue(SpriteHeightProperty, value); }
    }

    public static readonly DependencyProperty SpriteHeightProperty =
        DependencyProperty.Register("SpriteHeight", typeof(int), typeof(SpriteButton), null);

    public int NextOffsetX {
      get { return (int)GetValue(NextOffsetXProperty); }
      set { SetValue(NextOffsetXProperty, value); }
    }

    public static readonly DependencyProperty NextOffsetXProperty =
        DependencyProperty.Register("NextOffsetX", typeof(int), typeof(SpriteButton), null);

    public int NextOffsetY {
      get { return (int)GetValue(NextOffsetYProperty); }
      set { SetValue(NextOffsetYProperty, value); }
    }

    public static readonly DependencyProperty NextOffsetYProperty =
        DependencyProperty.Register("NextOffsetY", typeof(int), typeof(SpriteButton), null);

    public int SpriteIndex {
      get { return (int)GetValue(SpriteIndexProperty); }
      set { SetValue(SpriteIndexProperty, value); }
    }

    public static readonly DependencyProperty SpriteIndexProperty =
        DependencyProperty.Register("SpriteIndex", typeof(int), typeof(SpriteButton), null);

    public int PressedOffsetY {
      get { return (int)GetValue(PressedOffsetYProperty); }
      set { SetValue(PressedOffsetYProperty, value); }
    }

    public static readonly DependencyProperty PressedOffsetYProperty =
        DependencyProperty.Register("PressedOffsetY", typeof(int), typeof(SpriteButton), null);
    #endregion

    private void updateTransform() {
      m_transform.X = -NextOffsetX * SpriteIndex;
      m_transform.Y = -NextOffsetY * SpriteIndex;
      if (IsPressed) {
        m_transform.Y -= PressedOffsetY;
      }
    }

    private void updateSize(Rectangle child = null) {
      if (child == null) {
        child = GetTemplateChild("ImageHolder") as Rectangle;
      }
      if (child != null) {
        child.Width = SpriteWidth;
        child.Height = SpriteHeight;
      }
    }

    private readonly ImageBrush m_brush;
    private readonly TranslateTransform m_transform;
  }
}
