using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using PixelLab.Demo.Core;

namespace PixelLab.Wpf.Demo
{
    [DemoMetadata("TreeView Double-Click", "This is really two demos: 1) It's a demo of adding double-click handling to an items control. Since Click is a RoutedEvent, you can listen to it anywhere up the tree. Here I just use it to find the TreeViewItem that was double-clicked and show the XML content that is bound to it. You can use the same logic in a ListBox, if you like. 2) It's a demo of XML binding to a TreeView. (Since the folder picker already does CLR binding.) Take a look at the XAML. You'll see we have full XPath support.")]
    public partial class TreeViewDoubleClickPage : Page
    {
        public TreeViewDoubleClickPage()
        {
            InitializeComponent();
            treeView.AddHandler(TreeView.MouseLeftButtonDownEvent, new MouseButtonEventHandler(treeView_MouseLeftButtonDown), true);
        }

        void treeView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                TreeViewItem tvi = GetTreeViewItemFromChild(e.OriginalSource as DependencyObject);
                if (tvi != null)
                {
                    XmlNode xNode = tvi.Header as XmlNode;
                    if (xNode != null)
                    {
                        textBox.Text = xNode.OuterXml;
                    }
                    else
                    {
                        textBox.Text = tvi.Header.ToString();
                    }
                }
            }
        }

        private static TreeViewItem GetTreeViewItemFromChild(DependencyObject child)
        {
            if (child != null)
            {
                if (child is TreeViewItem)
                {
                    return child as TreeViewItem;
                }
                else if (child is TreeView)
                {
                    return null; //we've walked through the tree. The source of the click is not something else
                }
                else
                {
                    return GetTreeViewItemFromChild(VisualTreeHelper.GetParent(child));
                }
            }
            return null;
        }
    }
}