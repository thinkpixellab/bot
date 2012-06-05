using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using PixelLab.Common;

namespace PixelLab.SL
{
    public partial class SpinningProgressControl : UserControl
    {
        private const int SectionCount = 12;
        private const double Delta = 0.01;

        private readonly CompositionTargetRenderingListener _listener;
        private double _location;

        public SpinningProgressControl()
        {
            InitializeComponent();

            Foreground = Color.FromArgb(214, 0, 0, 0).ToBrush();

            for (var i = 0; i < SectionCount; i++)
            {
                var container = new Border()
                {
                    Child = new Rectangle()
                    {
                        Stretch = Stretch.Fill,
                        Fill = this.Foreground,
                        Margin = new Thickness(22, 0, 0, 0),
                        RadiusX = 4,
                        RadiusY = 4
                    },
                    Width = 50,
                    Height = 8,
                    RenderTransformOrigin = new Point(0, 0.5),
                    RenderTransform = new CompositeTransform()
                    {
                        Rotation = 360 * i / SectionCount
                    }
                };
                Canvas.SetLeft(container, _visualRoot.Width / 2);
                Canvas.SetTop(container, (_visualRoot.Height - container.Height) / 2);

                _visualRoot.Children.Add(container);
            }

            _listener = new CompositionTargetRenderingListener();
            _listener.WireParentLoadedUnloaded(this);
            _listener.Rendering += (sender, args) => onFrame();
            isSpinningChanged();
        }

        public static readonly DependencyProperty IsSpinningProperty = DependencyPropHelper.Register<SpinningProgressControl, bool>("IsSpinning", true, (owner, newVal, oldVal) => owner.isSpinningChanged());

        public bool IsSpinning
        {
            get { return (bool)GetValue(IsSpinningProperty); }
            set { SetValue(IsSpinningProperty, value); }
        }

        private void isSpinningChanged()
        {
            if (IsSpinning)
            {
                _listener.StartListening();
            }
            else
            {
                _listener.StopListening();
            }
        }

        private void onFrame()
        {
            if (!IsSpinning)
            {
                _listener.StopListening();
            }
            else
            {
                Debug.Assert(_visualRoot.Children.Count == SectionCount);
                for (var i = 0; i < SectionCount; i++)
                {
                    var location = i / (double)SectionCount;

                    var delta = _location - location;
                    if (delta < 0)
                    {
                        delta += 1;
                    }
                    delta *= 2;

                    delta = Math.Min(delta, 1);

                    _visualRoot.Children[i].Opacity = delta;
                }
                _location += Delta;
                if (_location >= 1)
                {
                    _location = 0;
                }
            }
        }
    }
}
