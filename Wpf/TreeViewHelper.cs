using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Threading;

namespace PixelLab.Wpf
{
    public static class TreeViewHelper
    {
        public static TreeViewItem GetTreeViewItemByIndex(TreeView treeView, int index)
        {
            return GetItemByIndexInternal(treeView, index);
        }
        public static TreeViewItem GetTreeViewItemByIndex(TreeViewItem treeViewItem, int index)
        {
            return GetItemByIndexInternal(treeViewItem, index);
        }
        public static TreeViewItem GetTreeViewItemByData(TreeView treeView, object reference)
        {
            return GetItemByReferenceInternal(treeView, reference);
        }
        public static TreeViewItem GetTreeViewItemByData(TreeViewItem treeViewItem, object reference)
        {
            return GetItemByReferenceInternal(treeViewItem, reference);
        }

        public static IList<TreeViewItem> GetTreeViewItems(TreeView treeView)
        {
            return GetItemsInternal(treeView);
        }
        public static IList<TreeViewItem> GetTreeViewItems(TreeViewItem treeView)
        {
            return GetItemsInternal(treeView);
        }

        public static TreeViewItem Show(TreeView treeView, object[] objectPath)
        {
            return ShowItemInternal(treeView, objectPath, null);
        }
        public static TreeViewItem Show(TreeViewItem treeViewItem, object[] objectPath)
        {
            return ShowItemInternal(treeViewItem, objectPath, null);
        }

        public static TreeViewItem Show(TreeView treeView, int[] indexPath)
        {
            return ShowItemInternal(treeView, null, indexPath);
        }
        public static TreeViewItem Show(TreeViewItem treeViewItem, int[] indexPath)
        {
            return ShowItemInternal(treeViewItem, null, indexPath);
        }

        public static void ExpandAll(TreeView treeView)
        {
            ExpandAllInternal(treeView);
        }
        public static void ExpandAll(ItemsControl treeViewItem)
        {
            ExpandAllInternal(treeViewItem);
        }

        #region implementation
        private static TreeViewItem GetItemByReferenceInternal(ItemsControl treeViewThing, object reference)
        {
            ValidateItemsControl(treeViewThing);
            if (treeViewThing.Items.Contains(reference))
            {
                return treeViewThing.ItemContainerGenerator.ContainerFromItem(reference) as TreeViewItem;
            }
            else
            {
                throw new ArgumentOutOfRangeException("reference");
            }
        }
        private static TreeViewItem GetItemByIndexInternal(ItemsControl treeViewThing, int index)
        {
            ValidateItemsControl(treeViewThing);
            if (index >= 0 && index < treeViewThing.Items.Count)
            {
                return treeViewThing.ItemContainerGenerator.ContainerFromIndex(index) as TreeViewItem;
            }
            else
            {
                throw new ArgumentOutOfRangeException("index");
            }
        }
        private static IList<TreeViewItem> GetItemsInternal(ItemsControl treeViewThing)
        {
            ValidateItemsControl(treeViewThing);

            TreeViewItem[] nodes = new TreeViewItem[treeViewThing.Items.Count];

            for (int i = 0; i < treeViewThing.Items.Count; i++)
            {
                TreeViewItem node = treeViewThing.Items[i] as TreeViewItem;

                if (node == null)
                {
                    node = treeViewThing.ItemContainerGenerator.ContainerFromItem(treeViewThing.Items[i]) as TreeViewItem;
                }

                nodes[i] = node;
            }

            return nodes;
        }

        private static void ExpandAllInternal(ItemsControl treeViewThing)
        {
            ValidateItemsControl(treeViewThing);

            TreeViewItem theNode;

            theNode = treeViewThing as TreeViewItem;
            if (theNode != null)
            {
                UIExpand(theNode);
            }

            for (int i = 0; i < treeViewThing.Items.Count; i++)
            {
                theNode = treeViewThing.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;
                UIExpand(theNode);

                ExpandAllInternal(theNode);
            }
        }

        private static TreeViewItem ShowItemInternal(ItemsControl treeViewThing,
            object[] objectPath, int[] indexPath)
        {
            ValidateItemsControl(treeViewThing);

            bool useObjectPath = true;
            TreeViewItem theItem = null;

            GetTreeViewItem(treeViewThing, objectPath, indexPath, out useObjectPath, out theItem);

            //2) See if the target item is already visible

            //Current state:
            //If 'theItem' is null, the parent needs to be expanded.
            //If 'theItem' != null, the parent doesn't need to be expanded. If this was "the item"
            //that was looked for (the path array.Legth == 0) then return the TVI
            //otherwise, go down another level

            if (theItem == null)
            {
                //the item is null. This is sad.

                //the first thing to do is make sure the "conditions are right" for the item to appear.
                //this means the parent TVI should be expanded.

                TreeViewItem tvi = treeViewThing as TreeViewItem;
                if (tvi != null)
                {
                    tvi.IsExpanded = true;
                }
                else
                {
                    Debug.Assert(treeViewThing is TreeView, "if it's not a TVI, it should be a TreeView");
                }

                //pump the dispatcher
                WaitForPriority(DispatcherPriority.Background);

                bool useObjectPath2;

                GetTreeViewItem(treeViewThing, objectPath, indexPath, out useObjectPath2, out theItem);
                Debug.Assert(useObjectPath2 == useObjectPath);
                if (theItem == null)
                {
                    throw new ApplicationException("Error getting ahold of the item");
                }
            }

            Debug.Assert(theItem != null);

            //we have the item
            if (useObjectPath)
            {
                Debug.Assert(objectPath.Length != 0);
                if (objectPath.Length == 1)
                {
                    return theItem;
                }
                else
                {
                    //we need to go down another level
                    objectPath = TrimArray<object>(objectPath);
                }
            }
            else
            {
                Debug.Assert(indexPath.Length != 0);
                if (indexPath.Length == 1)
                {
                    return theItem;
                }
                else
                {
                    indexPath = TrimArray<int>(indexPath);
                }
            }

            Debug.Assert(theItem != null);
            return ShowItemInternal(theItem, objectPath, indexPath);
        }

        private static void GetTreeViewItem(ItemsControl treeViewThing, object[] objectPath, int[] indexPath,
            out bool useObjectPath, out TreeViewItem theItem)
        {
            if (objectPath == null)
            {
                useObjectPath = false;
                if (indexPath == null)
                {
                    throw new ArgumentException("If objectPath is null, indexPath must have a value", "indexPath");
                }
                if (indexPath.Length < 1)
                {
                    throw new ArgumentException("indexPath.Length must be at least 1", "indexPath");
                }

                if (indexPath[0] < 0 || indexPath[0] >= treeViewThing.Items.Count)
                {
                    throw new ArgumentException("indexPath[0] is out of rang for treeViewThing.Items", "indexPath");
                }
                else
                {
                    theItem = GetItemByIndexInternal(treeViewThing, indexPath[0]);
                }
            }
            else
            {
                useObjectPath = true;
                if (indexPath != null)
                {
                    throw new ArgumentException("If objectPath is defined, indexPath should be null", "indexPath");
                }
                if (objectPath.Length < 1)
                {
                    throw new ArgumentException("objectPath.Length must be at least 1", "objectPath");
                }

                if (!treeViewThing.Items.Contains(objectPath[0]))
                {
                    throw new ArgumentException("treeViewThing does not contain the first item defined in objectPath", "objectPath");
                }
                else
                {
                    theItem = GetItemByReferenceInternal(treeViewThing, objectPath[0]);
                }
            }
        }

        private static void UIExpand(TreeViewItem treeViewItem)
        {
            treeViewItem.IsExpanded = true;

            //this is the special sauce: it let's layout happen for this object
            //KMOORE 2006-05-19: stollen from DRTS. Needs to be reviewed by someone "smart"
            WaitForPriority(DispatcherPriority.Background);

#if DEBUG
            if (treeViewItem.Items.Count > 0)
            {
                TreeViewItem subNode = treeViewItem.ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItem;
                Debug.Assert(subNode != null, "we should have a good node now");
            }
#endif
        }

        private static void ValidateItemsControl(ItemsControl itemsControl)
        {
            Debug.Assert(itemsControl is TreeViewItem || itemsControl is TreeView);
        }

        private static T[] TrimArray<T>(T[] items)
        {
            if (items.Length > 0)
            {
                T[] newItems = new T[items.Length - 1];
                Array.Copy(items, 1, newItems, 0, newItems.Length);
                return newItems;
            }
            else
            {
                return new T[0];
            }
        }

        #endregion

        #region Stolen DRT code

        private static bool WaitForPriority(DispatcherPriority priority)
        {
            const int defaultTimeout = 30000;

            // Schedule the ExitFrame operation to end the nested pump after the timeout trigger happens
            TimeoutFrame frame = new TimeoutFrame();

            FrameTimer timeoutTimer = new FrameTimer(frame, defaultTimeout,
                TimeoutFrameOperationInstance, DispatcherPriority.Send);
            timeoutTimer.Start();

            // exit after a priortity has been processed
            DispatcherOperation opExit = Dispatcher.CurrentDispatcher.BeginInvoke(priority,
                ExitFrameOperationInstance, frame);

            // Pump the dispatcher
            Dispatcher.PushFrame(frame);

            // abort the operations that did not get processed
            if (opExit.Status != DispatcherOperationStatus.Completed)
            {
                opExit.Abort();
            }
            if (!timeoutTimer.IsCompleted)
            {
                timeoutTimer.Stop();
            }

            return !frame.TimedOut;
        }

        private static object ExitFrameOperation(object obj)
        {
            DispatcherFrame frame = obj as DispatcherFrame;
            frame.Continue = false;
            return null;
        }

        private static object TimeoutFrameOperation(object obj)
        {
            TimeoutFrame frame = obj as TimeoutFrame;
            frame.Continue = false;
            frame.TimedOut = true;
            return null;
        }

        private static readonly DispatcherOperationCallback
            ExitFrameOperationInstance = new DispatcherOperationCallback(ExitFrameOperation);
        private static readonly DispatcherOperationCallback
            TimeoutFrameOperationInstance = new DispatcherOperationCallback(TimeoutFrameOperation);

        #region helper classes

        private class TimeoutFrame : DispatcherFrame
        {
            bool timedout = false;

            public bool TimedOut
            {
                get { return timedout; }
                set { timedout = value; }
            }
        }

        private class FrameTimer : DispatcherTimer
        {
            DispatcherFrame frame;
            DispatcherOperationCallback callback;
            bool isCompleted = false;

            public FrameTimer(DispatcherFrame frame, int milliseconds, DispatcherOperationCallback callback, DispatcherPriority priority)
                : base(priority)
            {
                this.frame = frame;
                this.callback = callback;
                Interval = TimeSpan.FromMilliseconds(milliseconds);
                Tick += new EventHandler(OnTick);
            }

            public bool IsCompleted
            {
                get { return isCompleted; }
            }

            void OnTick(object sender, EventArgs args)
            {
                isCompleted = true;
                Stop();
                callback(frame);
            }
        }

        #endregion
        #endregion
    }
}

