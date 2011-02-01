using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PixelLab.Common;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif

namespace PixelLab.Wpf
{
    public abstract class AnimatingPanel : Panel
    {
        protected AnimatingPanel()
        {
            m_listener.Rendering += compositionTarget_Rendering;
            m_listener.WireParentLoadedUnloaded(this);
        }

        #region DPs

        public double Dampening
        {
            get { return (double)GetValue(DampeningProperty); }
            set { SetValue(DampeningProperty, value); }
        }

        public static readonly DependencyProperty DampeningProperty =
            CreateDoubleDP("Dampening", 0.2, FrameworkPropertyMetadataOptions.None, 0, 1, false);

        public double Attraction
        {
            get { return (double)GetValue(AttractionProperty); }
            set { SetValue(AttractionProperty, value); }
        }

        public static readonly DependencyProperty AttractionProperty =
            CreateDoubleDP("Attraction", 2, FrameworkPropertyMetadataOptions.None, 0, double.PositiveInfinity, false);

        public double Variation
        {
            get { return (double)GetValue(VariationProperty); }
            set { SetValue(VariationProperty, value); }
        }

        public static readonly DependencyProperty VariationProperty =
            CreateDoubleDP("Variation", 1, FrameworkPropertyMetadataOptions.None, 0, true, 1, true, false);

        #endregion

        protected virtual Point ProcessNewChild(UIElement child, Rect providedBounds)
        {
            return providedBounds.Location;
        }

        protected void ArrangeChild(UIElement child, Rect bounds)
        {
            m_listener.StartListening();

            AnimatingPanelItemData data = (AnimatingPanelItemData)child.GetValue(DataProperty);
            if (data == null)
            {
                data = new AnimatingPanelItemData();
                child.SetValue(DataProperty, data);
                Debug.Assert(child.RenderTransform == Transform.Identity);
                child.RenderTransform = data.Transform;

                data.Current = ProcessNewChild(child, bounds);
            }
            Debug.Assert(child.RenderTransform == data.Transform);

            //set the location attached DP
            data.Target = bounds.Location;

            child.Arrange(bounds);
        }

        private void compositionTarget_Rendering(object sender, EventArgs e)
        {
            double dampening = this.Dampening;
            double attractionFactor = this.Attraction * .01;
            double variation = this.Variation;

            bool shouldChange = false;
            for (int i = 0; i < Children.Count; i++)
            {
                shouldChange = updateChildData(
                    (AnimatingPanelItemData)Children[i].GetValue(DataProperty),
                    dampening,
                    attractionFactor,
                    variation) || shouldChange;
            }

            if (!shouldChange)
            {
                m_listener.StopListening();
            }
        }

        private static bool updateChildData(AnimatingPanelItemData data, double dampening, double attractionFactor, double variation)
        {
            if (data == null)
            {
                return false;
            }
            else
            {
                Debug.Assert(dampening > 0 && dampening < 1);
                Debug.Assert(attractionFactor > 0 && !double.IsInfinity(attractionFactor));

                attractionFactor *= 1 + (variation * data.RandomSeed - .5);

                Point newLocation;
                Vector newVelocity;

                bool anythingChanged =
                    GeoHelper.Animate(data.Current, data.LocationVelocity, data.Target,
                        attractionFactor, dampening, c_terminalVelocity, c_diff, c_diff,
                        out newLocation, out newVelocity);

                data.Current = newLocation;
                data.LocationVelocity = newVelocity;

                var transformVector = data.Current - data.Target;
                data.Transform.SetToVector(transformVector);

                return anythingChanged;
            }
        }

        private readonly CompositionTargetRenderingListener m_listener = new CompositionTargetRenderingListener();

        protected static DependencyProperty CreateDoubleDP(
          string name,
          double defaultValue,
          FrameworkPropertyMetadataOptions metadataOptions,
          double minValue,
          double maxValue,
          bool attached)
        {
            return CreateDoubleDP(name, defaultValue, metadataOptions, minValue, false, maxValue, false, attached);
        }

        protected static DependencyProperty CreateDoubleDP(
            string name,
            double defaultValue,
            FrameworkPropertyMetadataOptions metadataOptions,
            double minValue,
            bool includeMin,
            double maxValue,
            bool includeMax,
            bool attached)
        {
            Contract.Requires(!double.IsNaN(minValue));
            Contract.Requires(!double.IsNaN(maxValue));
            Contract.Requires(maxValue >= minValue);

            ValidateValueCallback validateValueCallback = delegate(object objValue)
            {
                double value = (double)objValue;

                if (includeMin)
                {
                    if (value < minValue)
                    {
                        return false;
                    }
                }
                else
                {
                    if (value <= minValue)
                    {
                        return false;
                    }
                }
                if (includeMax)
                {
                    if (value > maxValue)
                    {
                        return false;
                    }
                }
                else
                {
                    if (value >= maxValue)
                    {
                        return false;
                    }
                }

                return true;
            };

            if (attached)
            {
                return DependencyProperty.RegisterAttached(
                    name,
                    typeof(double),
                    typeof(AnimatingTilePanel),
                    new FrameworkPropertyMetadata(defaultValue, metadataOptions), validateValueCallback);
            }
            else
            {
                return DependencyProperty.Register(
                    name,
                    typeof(double),
                    typeof(AnimatingTilePanel),
                    new FrameworkPropertyMetadata(defaultValue, metadataOptions), validateValueCallback);
            }
        }

        private static readonly DependencyProperty DataProperty =
            DependencyProperty.RegisterAttached("Data", typeof(AnimatingPanelItemData), typeof(AnimatingTilePanel));

        private const double c_diff = 0.1;
        private const double c_terminalVelocity = 10000;

        private class AnimatingPanelItemData
        {
            public Point Target;
            public Point Current;
            public Vector LocationVelocity;
            public readonly double RandomSeed = Util.Rnd.NextDouble();
            public readonly TranslateTransform Transform = new TranslateTransform();
        }
    }
}
