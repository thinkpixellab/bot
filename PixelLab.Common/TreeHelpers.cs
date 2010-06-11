using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PixelLab.Common {
  public static class TreeHelpers {

    public static IEnumerable<T> FindChildren<T>(DependencyObject obj) where T : DependencyObject {
      Contract.Requires(obj != null);
      return obj.GetChildren()
        .SelectRecursive(element => element.GetChildren())
        .OfType<T>();
    }

    public static T FindChild<T>(DependencyObject obj) where T : DependencyObject {
      Contract.Requires(obj != null);
      return FindChildren<T>(obj).FirstOrDefault();
    }

    public static T FindParent<T>(DependencyObject obj) where T : DependencyObject {
      while (obj != null) {
        obj = VisualTreeHelper.GetParent(obj);

        if (obj is T) {
          return (T)obj;
        }
      }
      return null;
    }

    /// <remarks>Includes element.</remarks>
    public static IEnumerable<DependencyObject> GetAncestors(this DependencyObject element) {
      Contract.Requires(element != null);
      do {
        yield return element;
        element = VisualTreeHelper.GetParent(element);
      } while (element != null);
    }

    public static bool HasAncestor(this DependencyObject element, DependencyObject ancestor) {
      return element
        .GetAncestors()
        .Where(e => e == ancestor)
        .Any();
    }

    public static IEnumerable<DependencyObject> GetChildren(this DependencyObject source) {
      Contract.Requires(source != null);
      int count = VisualTreeHelper.GetChildrenCount(source);

      for (int i = 0; i < count; i++) {
        yield return VisualTreeHelper.GetChild(source, i);
      }
    }

    public static UIElement GetItemContainerFromChildElement(ItemsControl itemsControl, UIElement child) {
      Contract.Requires(itemsControl != null);
      Contract.Requires(child != null);

      if (itemsControl.Items.Count > 0) {
        // find the ItemsPanel
        Panel panel = VisualTreeHelper.GetParent(itemsControl.ItemContainerGenerator.ContainerFromIndex(0)) as Panel;

        if (panel != null) {
          // Walk the tree until we get to the ItemsPanel, once we get there we know 
          // that the immediate child of the parent is going to be the ItemContainer

          UIElement parent;
          do {
            parent = VisualTreeHelper.GetParent(child) as UIElement;
            if (parent == panel) {
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
