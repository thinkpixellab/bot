using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using PixelLab.Common;

namespace PixelLab.SL
{
    public class SpriteElement : UserControl
    {
        public SpriteElement()
        {
            m_transform = new TranslateTransform();
            m_brush = new ImageBrush() { Stretch = Stretch.None, AlignmentX = AlignmentX.Left, AlignmentY = AlignmentY.Top, Transform = m_transform };

            var rect = new Rectangle() { Fill = m_brush };
            rect.SetBinding(Rectangle.WidthProperty, new Binding("SpriteWidth") { Source = this });
            rect.SetBinding(Rectangle.HeightProperty, new Binding("SpriteHeight") { Source = this });
            this.Content = rect;
        }

        #region DPs

        #region pressed
        public bool IsPressed
        {
            get { return (bool)GetValue(IsPressedProperty); }
            set { SetValue(IsPressedProperty, value); }
        }

        public static readonly DependencyProperty IsPressedProperty = DependencyPropHelper.Register<SpriteElement, bool>("IsPressed", (element, newValue, oldValue) => element.updateTransform());

        public int PressedOffsetX
        {
            get { return (int)GetValue(PressedOffsetXProperty); }
            set { SetValue(PressedOffsetXProperty, value); }
        }

        public static readonly DependencyProperty PressedOffsetXProperty =
          DependencyPropHelper.Register<SpriteElement, int>("PressedOffsetX", onValueChange);

        public int PressedOffsetY
        {
            get { return (int)GetValue(PressedOffsetYProperty); }
            set { SetValue(PressedOffsetYProperty, value); }
        }

        public static readonly DependencyProperty PressedOffsetYProperty =
          DependencyPropHelper.Register<SpriteElement, int>("PressedOffsetY", onValueChange);
        #endregion

        #region mouse over
        public bool IsMouseOver
        {
            get { return (bool)GetValue(IsMouseOverProperty); }
            set { SetValue(IsMouseOverProperty, value); }
        }

        public static readonly DependencyProperty IsMouseOverProperty = DependencyPropHelper.Register<SpriteElement, bool>("IsMouseOver", (element, newValue, oldValue) => element.updateTransform());

        public int MouseOverOffsetX
        {
            get { return (int)GetValue(MouseOverOffsetXProperty); }
            set { SetValue(MouseOverOffsetXProperty, value); }
        }

        public static readonly DependencyProperty MouseOverOffsetXProperty =
          DependencyPropHelper.Register<SpriteElement, int>("MouseOverOffsetX", onValueChange);

        public int MouseOverOffsetY
        {
            get { return (int)GetValue(MouseOverOffsetYProperty); }
            set { SetValue(MouseOverOffsetYProperty, value); }
        }

        public static readonly DependencyProperty MouseOverOffsetYProperty =
          DependencyPropHelper.Register<SpriteElement, int>("MouseOverOffsetY", onValueChange);
        #endregion

        public ImageSource ImageSource
        {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        public static readonly DependencyProperty ImageSourceProperty =
          DependencyPropHelper.Register<SpriteElement, ImageSource>("ImageSource", (element, newVal, oldVal) =>
          {
              element.m_brush.ImageSource = newVal;
              element.updateTransform();
          });

        public int SpriteWidth
        {
            get { return (int)GetValue(SpriteWidthProperty); }
            set { SetValue(SpriteWidthProperty, value); }
        }

        public static readonly DependencyProperty SpriteWidthProperty =
          DependencyPropHelper.Register<SpriteElement, int>("SpriteWidth");

        public int SpriteHeight
        {
            get { return (int)GetValue(SpriteHeightProperty); }
            set { SetValue(SpriteHeightProperty, value); }
        }

        public static readonly DependencyProperty SpriteHeightProperty =
          DependencyPropHelper.Register<SpriteElement, int>("SpriteHeight");

        public int NextOffsetX
        {
            get { return (int)GetValue(NextOffsetXProperty); }
            set { SetValue(NextOffsetXProperty, value); }
        }

        public static readonly DependencyProperty NextOffsetXProperty =
          DependencyPropHelper.Register<SpriteElement, int>("NextOffsetX", onValueChange);

        public int NextOffsetY
        {
            get { return (int)GetValue(NextOffsetYProperty); }
            set { SetValue(NextOffsetYProperty, value); }
        }

        public static readonly DependencyProperty NextOffsetYProperty =
          DependencyPropHelper.Register<SpriteElement, int>("NextOffsetY", onValueChange);

        public int SpriteIndex
        {
            get { return (int)GetValue(SpriteIndexProperty); }
            set { SetValue(SpriteIndexProperty, value); }
        }

        public static readonly DependencyProperty SpriteIndexProperty =
          DependencyPropHelper.Register<SpriteElement, int>("SpriteIndex", onValueChange);

        #endregion

        private void updateTransform()
        {
            m_transform.X = -NextOffsetX * SpriteIndex;
            m_transform.Y = -NextOffsetY * SpriteIndex;
            if (IsPressed)
            {
                m_transform.X -= PressedOffsetX;
                m_transform.Y -= PressedOffsetY;
            }
            else if (IsMouseOver)
            {
                m_transform.X -= MouseOverOffsetX;
                m_transform.Y -= MouseOverOffsetY;
            }
        }

        private static void onValueChange(SpriteElement element, int newValue, int oldValue)
        {
            element.updateTransform();
        }

        private readonly ImageBrush m_brush;
        private readonly TranslateTransform m_transform;
    }
}
