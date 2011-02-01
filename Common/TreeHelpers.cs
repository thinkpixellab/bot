using System.Collections.Generic;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PixelLab.Common
{
    public static class TreeHelpers
    {
        public static IEnumerable<T> VisualDescendentsOfType<T>(this DependencyObject obj) where T : DependencyObject
        {
            Contract.Requires(obj != null);
            return obj.GetVisualDescendents().OfType<T>();
        }

        public static T FirstVisualDescendentOfType<T>(this DependencyObject obj) where T : DependencyObject
        {
            Contract.Requires(obj != null);
            return VisualDescendentsOfType<T>(obj).FirstOrDefault();
        }

        public static T FindParent<T>(this DependencyObject obj) where T : DependencyObject
        {
            return obj.GetAncestors().OfType<T>().FirstOrDefault();
        }

        /// <remarks>Includes element.</remarks>
        public static IEnumerable<DependencyObject> GetAncestors(this DependencyObject element)
        {
            Contract.Requires(element != null);
            Contract.Ensures(Contract.Result<IEnumerable<DependencyObject>>() != null);
            do
            {
                yield return element;
                element = VisualTreeHelper.GetParent(element);
            } while (element != null);
        }

        public static bool HasAncestor(this DependencyObject element, DependencyObject ancestor)
        {
            Contract.Requires(element != null);
            Contract.Requires(ancestor != null);
            return element
              .GetAncestors()
              .Where(e => ancestor.Equals(e))
              .Any();
        }

        public static IEnumerable<DependencyObject> GetVisualChildren(this DependencyObject source)
        {
            Contract.Requires(source != null);
            int count = VisualTreeHelper.GetChildrenCount(source);

            for (int i = 0; i < count; i++)
            {
                yield return VisualTreeHelper.GetChild(source, i);
            }
        }

        public static IEnumerable<DependencyObject> GetVisualDescendents(this DependencyObject source)
        {
            Contract.Requires(source != null);
            return source
              .GetVisualChildren()
              .SelectRecursive(element => element.GetVisualChildren());
        }

        public static FrameworkElement FirstVisualDescendentByName(this DependencyObject source, string name)
        {
            return source.GetVisualDescendents()
              .OfType<FrameworkElement>()
              .Where(fe => fe.Name.Equals(name))
              .FirstOrDefault();
        }

        public static UIElement GetItemContainerFromChildElement(ItemsControl itemsControl, UIElement child)
        {
            Contract.Requires(itemsControl != null);
            Contract.Requires(child != null);

            if (itemsControl.Items.Count > 0)
            {
                // find the ItemsPanel
                Panel panel = VisualTreeHelper.GetParent(itemsControl.ItemContainerGenerator.ContainerFromIndex(0)) as Panel;

                if (panel != null)
                {
                    // Walk the tree until we get to the ItemsPanel, once we get there we know
                    // that the immediate child of the parent is going to be the ItemContainer

                    UIElement parent;
                    do
                    {
                        parent = VisualTreeHelper.GetParent(child) as UIElement;
                        if (parent == panel)
                        {
                            return child;
                        }
                        child = parent;
                    }
                    while (parent != null);
                }
            }
            return null;
        }
    }
}
