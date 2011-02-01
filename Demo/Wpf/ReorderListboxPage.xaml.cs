using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using PixelLab.Common;
using PixelLab.Demo.Core;

namespace PixelLab.Wpf.Demo
{
    [DemoMetadata("Reorder Listbox")]
    public partial class ReorderListBoxPage : Page
    {
        public ReorderListBoxPage()
        {
            var brushes = new ObservableCollection<SolidColorBrush>(App.DemoColors.Select(c => c.ToCachedBrush()));

            DataContext = brushes;

            AddHandler(
              ReorderListBox.ReorderRequestedEvent,
              new EventHandler<ReorderEventArgs>(
                delegate(object sender, ReorderEventArgs args)
                {
                    var reorderListBox = (ReorderListBox)args.OriginalSource;

                    var draggingBrush = (SolidColorBrush)reorderListBox.ItemContainerGenerator.ItemFromContainer(args.ItemContainer);
                    var toBrush = (SolidColorBrush)reorderListBox.ItemContainerGenerator.ItemFromContainer(args.ToContainer);

                    brushes.Move(brushes.IndexOf(draggingBrush), brushes.IndexOf(toBrush));
                }));

            InitializeComponent();
        }
    }
}
