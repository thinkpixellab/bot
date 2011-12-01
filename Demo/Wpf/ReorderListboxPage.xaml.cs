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
        private readonly ObservableCollection<SolidColorBrush> _brushes;

        public ReorderListBoxPage()
        {
            DataContext = _brushes = new ObservableCollection<SolidColorBrush>(App.DemoColors.Select(c => c.ToCachedBrush()));

            InitializeComponent();
        }

        private void listbox_Reorder(object sender, ReorderEventArgs args)
        {
            var reorderListBox = (ReorderListBox)args.OriginalSource;

            var draggingBrush = (SolidColorBrush)reorderListBox.ItemContainerGenerator.ItemFromContainer(args.ItemContainer);
            var toBrush = (SolidColorBrush)reorderListBox.ItemContainerGenerator.ItemFromContainer(args.ToContainer);

            _brushes.Move(_brushes.IndexOf(draggingBrush), _brushes.IndexOf(toBrush));
        }
    }
}
