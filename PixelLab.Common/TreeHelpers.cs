using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PixelLab.Common {
  public static class TreeHelpers {

    public static T FindChild<T>(DependencyObject obj) where T : DependencyObject {
      Contract.Requires(obj != null);
      return (T)obj.GetChildren()
        .SelectRecursive(element => element.GetChildren())
        .Where(element => element is T)
        .FirstOrDefault();
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

    public static bool HasAncestor(this DependencyObject element, DependencyObject ancestor) {
      do {
        if (element == ancestor) {
          return true;
        }
        else {
          element = VisualTreeHelper.GetParent(element);
        }
      } while (element != null);
      return false;
    }

    public static IEnumerable<DependencyObject> GetChildren(this DependencyObject source) {
      Contract.Requires(source != null);
      int count = VisualTreeHelper.GetChildrenCount(source);

      for (int i = 0; i < count; i++) {
        yield return VisualTreeHelper.GetChild(source, i);
      }
    }

    public static UIElement GetItemContainerFromChildElement(ItemsControl itemsControl, UIElement child) {
      if (itemsControl.Items.Count > 0) {
        // find the ItemsPanel
        Panel p = VisualTreeHelper.GetParent(itemsControl.ItemContainerGenerator.ContainerFromIndex(0)) as Panel;

        if (p != null) {
          // Walk the tree until we get to the ItemsPanel, once we get there we know 
          // that the immediate child of the parent is going to be the ItemContainer

          UIElement element = child;
          UIElement parent = VisualTreeHelper.GetParent(element) as UIElement;

          // TODO: replace this with a do,while
          while (true) {
            if (parent == p) return element;
            if (parent == null) return parent;

            element = parent;
            parent = VisualTreeHelper.GetParent(element) as UIElement;
          }
        }
      }
      return null;
    }
  }
}
