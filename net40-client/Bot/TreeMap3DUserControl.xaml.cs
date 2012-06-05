using System.Collections;
using System.Windows.Controls;

namespace PixelLab.Wpf
{
    public partial class TreeMap3DUserControl : UserControl
    {
        public TreeMap3DUserControl()
        {
            InitializeComponent();

            _treeMap3D.SelectionChanged += delegate(object sender, SelectionChangedEventArgs e)
            {
                object selectedData = (e.AddedItems.Count == 0) ? null : e.AddedItems[0];

                if (selectedData != null)
                {
                    _dataDisplay.DataContext = e.AddedItems[0];
                }

                if (_lastSelected == null)
                {
                    _transition3D.IsExpanded = true;
                }
                else if (selectedData == null)
                {
                    _transition3D.IsExpanded = false;
                }

                _lastSelected = selectedData;
            };
        }

        /// <remarks>
        ///     This is a weak ItemsSource. It does not support binding or
        ///     INotifyCollectionChanged.
        /// </remarks>
        public IList ItemsSource
        {
            get
            {
                return _treeMap3D.ItemsSource;
            }
            set
            {
                _treeMap3D.ItemsSource = value;
            }
        }

        private object _lastSelected = null;
    }
}