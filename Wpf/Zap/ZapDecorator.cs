using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PixelLab.Common;

namespace PixelLab.Wpf
{
    public class ZapDecorator : Decorator
    {
        public ZapDecorator()
        {
            m_listener.Rendering += m_listener_rendering;
            m_listener.WireParentLoadedUnloaded(this);
        }

        public static readonly DependencyProperty TargetIndexProperty =
            DependencyProperty.Register("TargetIndex", typeof(int), typeof(ZapDecorator),
            new FrameworkPropertyMetadata(0, new PropertyChangedCallback(targetIndexChanged)));

        public int TargetIndex
        {
            get { return (int)GetValue(TargetIndexProperty); }
            set { SetValue(TargetIndexProperty, value); }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            UIElement child = this.Child;
            if (child != null)
            {
                m_listener.StartListening();
                child.Measure(availableSize);
            }
            return new Size();
        }

        #region Implementation

        private void m_listener_rendering(object sender, EventArgs e)
        {
            if (this.Child != m_zapPanel)
            {
                m_zapPanel = (ZapPanel)this.Child;
                m_zapPanel.RenderTransform = m_traslateTransform;
            }
            if (m_zapPanel != null)
            {
                int actualTargetIndex = Math.Max(0, Math.Min(m_zapPanel.Children.Count - 1, TargetIndex));

                double targetPercentOffset = -actualTargetIndex / (double)m_zapPanel.Children.Count;
                targetPercentOffset = double.IsNaN(targetPercentOffset) ? 0 : targetPercentOffset;

                bool stopListening = !GeoHelper.Animate(
                    m_percentOffset, m_velocity, targetPercentOffset,
                    .05, .3, .1, c_diff, c_diff,
                    out m_percentOffset, out m_velocity);

                double targetPixelOffset = m_percentOffset * (this.RenderSize.Width * m_zapPanel.Children.Count);
                m_traslateTransform.X = targetPixelOffset;

                if (stopListening)
                {
                    m_listener.StopListening();
                }
            }
        }

        private static void targetIndexChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
        {
            ((ZapDecorator)element).m_listener.StartListening();
        }

        private double m_velocity;
        private double m_percentOffset;

        private ZapPanel m_zapPanel;
        private readonly TranslateTransform m_traslateTransform = new TranslateTransform();
        private readonly CompositionTargetRenderingListener m_listener = new CompositionTargetRenderingListener();

        private const double c_diff = .00001;

        #endregion
    }
}