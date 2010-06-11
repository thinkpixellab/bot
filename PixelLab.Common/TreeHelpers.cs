using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PixelLab.Common {
  public static class TreeHelpers {
    // TODO: make this non-recursive, to impress the kids
    public static T FindChild<T>(DependencyObject obj) where T : DependencyObject {
      if (obj as T != null) return obj as T;

      int childCount = VisualTreeHelper.GetChildrenCount(obj);
      for (int i = 0; i < childCount; i++) {
        DependencyObject child = VisualTreeHelper.GetChild(obj, i);

        T foundChild = FindChild<T>(child);
        if (foundChild != null) return foundChild;
      }

      return null;
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
