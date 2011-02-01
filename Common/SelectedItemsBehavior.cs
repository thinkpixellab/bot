using System.Collections;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace PixelLab.Common
{
    /// <summary>
    /// Provides an attached behavior to enable binding the selected items of a <see cref="ListBox"/>.
    /// </summary>
    public class SelectedItemsBehavior : Behavior<ListBox>
    {
        /// <summary>
        /// Identifies the SelectedItems dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedItemsProperty = DependencyPropHelper.Register<SelectedItemsBehavior, IList>("SelectedItems");

        /// <summary>
        /// Gets or sets the selected items. This is a dependency property.
        /// </summary>
        /// <value>The selected items.</value>
        public IList SelectedItems
        {
            get { return (IList)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        /// <summary>
        /// Called after the behavior is attached to an AssociatedObject.
        /// </summary>
        /// <remarks>Override this to hook up functionality to the AssociatedObject.</remarks>
        protected override void OnAttached()
        {
            base.OnAttached();

            // NOTE: For a better implementation of this behavior we should probably handle changes
            //       to the DataContext, but a) there's no event for that in SL4 yet and b) we
            //       we don't actually need it in this application.
            this.AssociatedObject.SelectionChanged += listBox_selectionChanged;
        }

        /// <summary>
        /// Called when the behavior is being detached from its AssociatedObject, but before it has actually occurred.
        /// </summary>
        /// <remarks>Override this to unhook functionality from the AssociatedObject.</remarks>
        protected override void OnDetaching()
        {
            this.AssociatedObject.SelectionChanged -= listBox_selectionChanged;
            base.OnDetaching();
        }

        private void listBox_selectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedItems != null)
            {
                foreach (var removed in e.RemovedItems)
                {
                    Debug.Assert(SelectedItems.Contains(removed));
                    this.SelectedItems.Remove(removed);
                }

                foreach (var added in e.AddedItems)
                {
                    this.SelectedItems.Add(added);
                }

                Debug.Assert(SelectedItems.Count == AssociatedObject.SelectedItems.Count);
            }
        }
    }
}
