using System;
using System.Windows.Controls;
using PixelLab.Demo.Core;

namespace PixelLab.Wpf.Demo
{
    [DemoMetadata("Visual Tree Viewer", "This may seem a bit silly, but it's such a great demo and amazingly useful every once-in-a-while. So I have a ListBox with a bunch of random stuff in it. What does the visual tree look like? What are the actual WPF elements that are getting created? Well, WPF has VisualTreeWalker which will let you get the children of any Visual. You could write some debug spew to walk the tree out to the Console. Or, you could create a helper class VisualTreeItem, point it at the root element you care about when you click a button, and bind to it with a TreeView. Yeah, a bit silly. :-)")]
    public partial class VisualTreeViewerPage : Page
    {
        public VisualTreeViewerPage()
        {
            InitializeComponent();
        }

        private void load_click(object sender, EventArgs args)
        {
            VisualTreeItem vti = new VisualTreeItem(listBox);

            treeView.ItemsSource = vti.Children;
        }
    }
}