using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PixelLab.Common;

namespace PixelLab.SL
{
    public class SpriteButton : Button
    {
        public SpriteButton()
        {
            DefaultStyleKey = typeof(SpriteButton);
        }

        #region DPs
        public ImageSource ImageSource
        {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        public static readonly DependencyProperty ImageSourceProperty =
          DependencyPropHelper.Register<SpriteButton, ImageSource>("ImageSource");

        public int SpriteWidth
        {
            get { return (int)GetValue(SpriteWidthProperty); }
            set { SetValue(SpriteWidthProperty, value); }
        }

        public static readonly DependencyProperty SpriteWidthProperty =
          DependencyPropHelper.Register<SpriteButton, int>("SpriteWidth");

        public int SpriteHeight
        {
            get { return (int)GetValue(SpriteHeightProperty); }
            set { SetValue(SpriteHeightProperty, value); }
        }

        public static readonly DependencyProperty SpriteHeightProperty =
          DependencyPropHelper.Register<SpriteButton, int>("SpriteHeight");

        public int NextOffsetX
        {
            get { return (int)GetValue(NextOffsetXProperty); }
            set { SetValue(NextOffsetXProperty, value); }
        }

        public static readonly DependencyProperty NextOffsetXProperty =
            DependencyPropHelper.Register<SpriteButton, int>("NextOffsetX");

        public int NextOffsetY
        {
            get { return (int)GetValue(NextOffsetYProperty); }
            set { SetValue(NextOffsetYProperty, value); }
        }

        public static readonly DependencyProperty NextOffsetYProperty =
            DependencyPropHelper.Register<SpriteButton, int>("NextOffsetY");

        public int SpriteIndex
        {
            get { return (int)GetValue(SpriteIndexProperty); }
            set { SetValue(SpriteIndexProperty, value); }
        }

        public static readonly DependencyProperty SpriteIndexProperty =
            DependencyPropHelper.Register<SpriteButton, int>("SpriteIndex");

        public int PressedOffsetX
        {
            get { return (int)GetValue(PressedOffsetXProperty); }
            set { SetValue(PressedOffsetXProperty, value); }
        }

        public static readonly DependencyProperty PressedOffsetXProperty =
          DependencyPropHelper.Register<SpriteButton, int>("PressedOffsetX");

        public int PressedOffsetY
        {
            get { return (int)GetValue(PressedOffsetYProperty); }
            set { SetValue(PressedOffsetYProperty, value); }
        }

        public static readonly DependencyProperty PressedOffsetYProperty =
          DependencyPropHelper.Register<SpriteButton, int>("PressedOffsetY");

        public int MouseOverOffsetX
        {
            get { return (int)GetValue(MouseOverOffsetXProperty); }
            set { SetValue(MouseOverOffsetXProperty, value); }
        }

        public static readonly DependencyProperty MouseOverOffsetXProperty =
          DependencyPropHelper.Register<SpriteButton, int>("MouseOverOffsetX");

        public int MouseOverOffsetY
        {
            get { return (int)GetValue(MouseOverOffsetYProperty); }
            set { SetValue(MouseOverOffsetYProperty, value); }
        }

        public static readonly DependencyProperty MouseOverOffsetYProperty =
          DependencyPropHelper.Register<SpriteButton, int>("MouseOverOffsetY");

        #endregion
    }
}
