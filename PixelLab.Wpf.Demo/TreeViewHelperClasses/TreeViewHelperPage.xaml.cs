using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using PixelLab.Common;

namespace PixelLab.Wpf.Demo
{
    public partial class TreeViewHelperPage : Page
    {
        public TreeViewHelperPage()
        {
            InitializeComponent();

            resetData();

            this.AddHandler(Button.ClickEvent, new RoutedEventHandler(click), true);
        }

        private void resetData()
        {
            do
            {
                _theThing = new TreeViewDataItem(DataTree.GetRandomDataTree(5, 5), "Children");
            }
            while (_theThing.Children.Count < 3);
            
            treeView.ItemsSource = _theThing.Children;
        }

        private void click(object sender, RoutedEventArgs args)
        {
            Button button = args.OriginalSource as Button;
            if (button != null)
            {
                string buttonContent = button.Content as string;
                if (buttonContent != null)
                {
                    switch (buttonContent)
                    {

                        case "Refresh data":

                            resetData();

                            break;

                        case "Expand by objects":
                            object[] path;
                            do
                            {
                                path = getRandomObjectPath(_theThing);
                            }
                            while (path.Length < 1);
                            TreeViewHelper.Show(treeView, path);

                            break;
                        case "Expand by indicies":
                            int[] intPath;
                            do
                            {
                                intPath = getRandomIntPath(_theThing);
                            }
                            while (intPath.Length < 1);

                            TreeViewHelper.Show(treeView, intPath);
                            break;

                        case "Expand All":
                            TreeViewHelper.ExpandAll(treeView);

                            break;

                        case "Expand All (via data)":

                            _theThing.ExpandAll();

                            break;

                        default:
                            Debug.WriteLine("no handler for '" + buttonContent + "'");
                            break;
                    }
                }
            }
        }

        private TreeViewDataItem _theThing;

        private static object[] getRandomObjectPath(TreeViewDataItem thing)
        {
            List<TreeViewDataItem> path = new List<TreeViewDataItem>();
            Random rnd = Util.Rnd;
            while (thing != null)
            {
                if (thing.Children.Count > 0)
                {
                    thing = thing.Children[rnd.Next(thing.Children.Count)];
                    path.Add(thing);
                }
                else
                {
                    thing = null;
                }
            }
            return path.ToArray();
        }

        private static int[] getRandomIntPath(TreeViewDataItem thing)
        {
            List<int> path = new List<int>();
            Random rnd = Util.Rnd;
            while (thing != null)
            {
                if (thing.Children.Count > 0)
                {
                    int index = rnd.Next(thing.Children.Count);
                    thing = thing.Children[index];
                    path.Add(index);
                }
                else
                {
                    thing = null;
                }
            }
            return path.ToArray();
        }
    }
}