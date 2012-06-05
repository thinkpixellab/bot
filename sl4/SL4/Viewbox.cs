// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Markup;
using System.Windows.Media;

[assembly: SuppressMessage("Compatibility", "SWC4000:GeneralWPFCompatibilityRule", MessageId = "Viewbox, base is System.Windows.Controls.ContentControl (Silverlight) vs System.Windows.Controls.Decorator (wpf)", Justification = "Silverlight does not support direct manipulation of the visual tree, so Viewbox has to inherit from a core control that can.")]

namespace System.Windows.Controls
{
    /// <summary>
    /// Defines a content decorator that can stretch and scale a single child to
    /// fill the available space.
    /// </summary>
    /// <remarks>
    /// Viewbox should inherit from Decorator (which inherits from
    /// FrameworkElement), but the closest working base in Silverlight is the
    /// ContentControl class.  This provides a number of extra APIs not present
    /// in WPF that should be avoided (including the Template property which
    /// should not be changed from its default value).  Viewbox has been sealed
    /// to prevent the creation of derived classes that depend on these features
    /// not available on its WPF counterpart.
    /// </remarks>
    /// <QualityBand>Stable</QualityBand>
    [ContentProperty("Child")]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Viewbox", Justification = "Consistency with WPF")]
    public sealed partial class Viewbox : ContentControl
    {
        /// <summary>
        /// Name of child element in Viewbox's default template.
        /// </summary>
        private const string ChildElementName = "Child";

        /// <summary>
        /// XAML markup used to define the write-once Viewbox template.
        /// </summary>
        private const string DefaultTemplateMarkup =
            "<ControlTemplate " +
              "xmlns=\"http://schemas.microsoft.com/client/2007\" " +
              "xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">" +
                "<ContentPresenter x:Name=\"" + ChildElementName + "\" " +
                  "HorizontalAlignment=\"{TemplateBinding HorizontalAlignment}\" " +
                  "VerticalAlignment=\"{TemplateBinding VerticalAlignment}\"/>" +
            "</ControlTemplate>";

        /// <summary>
        /// Gets or sets the default ControlTemplate of the Viewbox.
        /// </summary>
        private ControlTemplate DefaultTemplate { get; set; }

        /// <summary>
        /// Gets or sets the element of the Viewbox that will render the child.
        /// </summary>
        private ContentPresenter ChildElement { get; set; }

        /// <summary>
        /// Gets or sets the transformation on the ChildElement used to scale the
        /// Child content.
        /// </summary>
        private ScaleTransform Scale { get; set; }

        /// <summary>
        /// Gets or sets the single child element of a
        /// <see cref="T:System.Windows.Controls.Viewbox" /> element.
        /// </summary>
        /// <value>
        /// The single child element of a
        /// <see cref="T:System.Windows.Controls.Viewbox" /> element.
        /// </value>
        /// <remarks>
        /// Child must be an alias of ContentControl.Content property to ensure
        /// continuous namescope, ie, named element within Viewbox can be found.
        /// </remarks>
        public UIElement Child
        {
            get { return Content as UIElement; }
            set { Content = value; }
        }

        #region public Stretch Stretch
        /// <summary>
        /// Gets or sets the <see cref="T:System.Windows.Media.Stretch" /> mode,
        /// which determines how content fits into the available space.
        /// </summary>
        /// <value>
        /// A <see cref="T:System.Windows.Media.Stretch" /> mode, which
        /// determines how content fits in the available space.  The default is
        /// <see cref="F:System.Windows.Media.Stretch.Uniform" />.
        /// </value>
        public Stretch Stretch
        {
            get { return (Stretch)GetValue(StretchProperty); }
            set { SetValue(StretchProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="P:System.Windows.Controls.Viewbox.Stretch" />
        /// dependency property.
        /// </summary>
        /// <value>
        /// The identifier for the
        /// <see cref="P:System.Windows.Controls.Viewbox.Stretch" /> dependency
        /// property.
        /// </value>
        public static readonly DependencyProperty StretchProperty =
            DependencyProperty.Register(
                "Stretch",
                typeof(Stretch),
                typeof(Viewbox),
                new PropertyMetadata(Stretch.Uniform, OnStretchPropertyChanged));

        /// <summary>
        /// StretchProperty property changed handler.
        /// </summary>
        /// <param name="d">Viewbox that changed its Stretch.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnStretchPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Viewbox vb = (Viewbox)d;
            if (!IsValidStretchValue(e.NewValue))
            {
                // revert the change
                vb.Stretch = (Stretch)e.OldValue;
                throw new ArgumentException();
            }

            // The Stretch property affects measuring
            vb.InvalidateMeasure();
        }

        /// <summary>
        /// Check whether the passed in object value is a valid Stretch enum value.
        /// </summary>
        /// <param name="o">The object typed value to be checked.</param>
        /// <returns>True if o is a valid Stretch enum value, false o/w.</returns>
        private static bool IsValidStretchValue(object o)
        {
            Stretch s = (Stretch)o;
            return s == Stretch.None || s == Stretch.Uniform || s == Stretch.Fill || s == Stretch.UniformToFill;
        }
        #endregion public Stretch Stretch

        #region public StretchDirection StretchDirection
        /// <summary>
        /// Gets or sets the
        /// <see cref="T:System.Windows.Controls.StretchDirection" />, which
        /// determines how scaling is applied to the contents of a
        /// <see cref="T:System.Windows.Controls.Viewbox" />.
        /// </summary>
        /// <value>
        /// A <see cref="T:System.Windows.Controls.StretchDirection" />, which
        /// determines how scaling is applied to the contents of a
        /// <see cref="T:System.Windows.Controls.Viewbox" />. The default is
        /// <see cref="F:System.Windows.Controls.StretchDirection.Both" />.
        /// </value>
        public StretchDirection StretchDirection
        {
            get { return (StretchDirection)GetValue(StretchDirectionProperty); }
            set { SetValue(StretchDirectionProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="P:System.Windows.Controls.Viewbox.StretchDirection" />
        /// dependency property.
        /// </summary>
        /// <value>
        /// The identifier for the
        /// <see cref="P:System.Windows.Controls.Viewbox.StretchDirection" />
        /// dependency property.
        /// </value>
        public static readonly DependencyProperty StretchDirectionProperty =
            DependencyProperty.Register(
                "StretchDirection",
                typeof(StretchDirection),
                typeof(Viewbox),
                new PropertyMetadata(StretchDirection.Both, OnStretchDirectionPropertyChanged));

        /// <summary>
        /// StretchDirectionProperty property changed handler.
        /// </summary>
        /// <param name="d">Viewbox that changed its StretchDirection.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnStretchDirectionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Viewbox vb = (Viewbox)d;
            if (!IsValidStretchDirectionValue(e.NewValue))
            {
                // revert the change
                vb.StretchDirection = (StretchDirection)e.OldValue;

                throw new ArgumentException();
            }

            // The StretchDirection property affects measuring
            vb.InvalidateMeasure();
        }

        /// <summary>
        /// Check whether the passed in object value is a valid StretchDirection enum value.
        /// </summary>
        /// <param name="o">The object typed value to be checked.</param>
        /// <returns>True if o is a valid StretchDirection enum value, false o/w.</returns>
        private static bool IsValidStretchDirectionValue(object o)
        {
            StretchDirection sd = (StretchDirection)o;
            return sd == StretchDirection.UpOnly || sd == StretchDirection.DownOnly || sd == StretchDirection.Both;
        }
        #endregion public StretchDirection StretchDirection

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:System.Windows.Controls.Viewbox" /> class.
        /// </summary>
        public Viewbox()
        {
            // Load the default template
            Template = DefaultTemplate = XamlReader.Load(DefaultTemplateMarkup) as ControlTemplate;
            ApplyTemplate();
            IsTabStop = false;
        }

        /// <summary>
        /// Builds the visual tree for the
        /// <see cref="T:System.Windows.Controls.Viewbox" /> control when a new
        /// template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            // Ensure the Template property never changes from the
            // DefaultTemplate, and only apply it one time.
            if (Template != DefaultTemplate)
            {
                throw new InvalidOperationException();
            }

            // Get the root visual of the template
            ChildElement = GetTemplateChild(ChildElementName) as ContentPresenter;
            Debug.Assert(ChildElement != null, "The required template part ChildElement was not found!");

            // Create the transformation to scale the container
            ChildElement.RenderTransform = Scale = new ScaleTransform();
        }

        /// <summary>
        /// Measures the child element of a Viewbox to prepare for arranging
        /// it during the ArrangeOverride pass.
        /// </summary>
        /// <remarks>
        /// Viewbox measures it's child at an infinite constraint; it allows the child to be however large it so desires.
        /// The child's returned size will be used as it's natural size for scaling to Viewbox's size during Arrange.
        /// </remarks>
        /// <param name="availableSize">
        /// An upper limit Size that should not be exceeded.
        /// </param>
        /// <returns>The target Size of the element.</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            Size size = new Size();
            if (Child != null)
            {
                Debug.Assert(ChildElement != null, "The required template part ChildElement was not found!");

                // Get the child's desired size
                ChildElement.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Size desiredSize = ChildElement.DesiredSize;

                // Determine how much we should scale the child
                Size scale = ComputeScaleFactor(availableSize, desiredSize);
                Debug.Assert(!double.IsPositiveInfinity(scale.Width), "The scale scaleX should not be infinite.");
                Debug.Assert(!double.IsPositiveInfinity(scale.Height), "The scale scaleY should not be infinite.");

                // Determine the desired size of the Viewbox
                size.Width = scale.Width * desiredSize.Width;
                size.Height = scale.Height * desiredSize.Height;
            }
            return size;
        }

        /// <summary>
        /// Arranges the content of a Viewbox element.
        /// Viewbox always sets the child to its desired size.  It then computes and applies a transformation
        /// from that size to the space available: Viewbox's own input size less child margin.
        /// </summary>
        /// <param name="finalSize">
        /// The Size this element uses to arrange its child content.
        /// </param>
        /// <returns>
        /// The Size that represents the arranged size of this Viewbox element
        /// and its child.
        /// </returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            Debug.Assert(ChildElement != null, "The required template part ChildElement was not found!");
            if (Child != null)
            {
                // Determine the scale factor given the final size
                Size desiredSize = ChildElement.DesiredSize;
                Size scale = ComputeScaleFactor(finalSize, desiredSize);

                // Scale the ChildElement by the necessary factor
                Debug.Assert(Scale != null, "Scale should not be null!");
                Scale.ScaleX = scale.Width;
                Scale.ScaleY = scale.Height;

                // Position the ChildElement to fill the ChildElement
                Rect originalPosition = new Rect(0, 0, desiredSize.Width, desiredSize.Height);
                ChildElement.Arrange(originalPosition);

                // Determine the final size used by the Viewbox
                finalSize.Width = scale.Width * desiredSize.Width;
                finalSize.Height = scale.Height * desiredSize.Height;
            }
            return finalSize;
        }

        /// <summary>
        /// Compute the scale factor of the Child content.
        /// </summary>
        /// <param name="availableSize">
        /// Available size to fill with content.
        /// </param>
        /// <param name="contentSize">Desired size of the content.</param>
        /// <returns>Width and Height scale factors.</returns>
        private Size ComputeScaleFactor(Size availableSize, Size contentSize)
        {
            double scaleX = 1.0;
            double scaleY = 1.0;

            bool isConstrainedWidth = !double.IsPositiveInfinity(availableSize.Width);
            bool isConstrainedHeight = !double.IsPositiveInfinity(availableSize.Height);
            Stretch stretch = Stretch;

            // Don't scale if we shouldn't stretch or the scaleX and scaleY are both infinity.
            if ((stretch != Stretch.None) && (isConstrainedWidth || isConstrainedHeight))
            {
                // Compute the individual scaleX and scaleY scale factors
                scaleX = IsZero(contentSize.Width) ? 0.0 : (availableSize.Width / contentSize.Width);
                scaleY = IsZero(contentSize.Height) ? 0.0 : (availableSize.Height / contentSize.Height);

                // Make the scale factors uniform by setting them both equal to
                // the larger or smaller (depending on infinite lengths and the
                // Stretch value)
                if (!isConstrainedWidth)
                {
                    scaleX = scaleY;
                }
                else if (!isConstrainedHeight)
                {
                    scaleY = scaleX;
                }
                else
                {
                    // (isConstrainedWidth && isConstrainedHeight)
                    switch (stretch)
                    {
                        case Stretch.Uniform:
                            // Use the smaller factor for both
                            scaleX = scaleY = Math.Min(scaleX, scaleY);
                            break;
                        case Stretch.UniformToFill:
                            // Use the larger factor for both
                            scaleX = scaleY = Math.Max(scaleX, scaleY);
                            break;
                        case Stretch.Fill:
                        default:
                            break;
                    }
                }

                // Prevent scaling in an undesired direction
                switch (StretchDirection)
                {
                    case StretchDirection.UpOnly:
                        scaleX = Math.Max(1.0, scaleX);
                        scaleY = Math.Max(1.0, scaleY);
                        break;
                    case StretchDirection.DownOnly:
                        scaleX = Math.Min(1.0, scaleX);
                        scaleY = Math.Min(1.0, scaleY);
                        break;
                    case StretchDirection.Both:
                    default:
                        break;
                }
            }

            return new Size(scaleX, scaleY);
        }

        /// <summary>
        /// Check if a number is zero.
        /// </summary>
        /// <param name="value">The number to check.</param>
        /// <returns>True if the number is zero, false otherwise.</returns>
        private static bool IsZero(double value)
        {
            // We actually consider anything within an order of magnitude of
            // epsilon to be zero
            return Math.Abs(value) < 2.2204460492503131E-15;
        }
    }
}