using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PixelLab.Common;
using PixelLab.Demo.Core;

namespace PixelLab.SL.Demo
{
    public partial class DemoHost : UserControl
    {
        public DemoHost()
        {
            InitializeComponent();

            Catalog = new List<ExportFactory<FrameworkElement, IDemoMetadata>>();
            CompositionInitializer.SatisfyImports(this);

            var items = Catalog.OrderBy(v => v.Metadata.Name).ToList();
            var welcome = items.FirstOrDefault(m => m.Metadata.Name.Equals("welcome", System.StringComparison.InvariantCultureIgnoreCase));
            if (welcome != null)
            {
                items.Remove(welcome);
                items.Insert(0, welcome);
            }
            Catalog = items.ToReadOnlyCollection();

            m_items.ItemsSource = Catalog;

            m_items.SelectionChanged += (sender, args) =>
            {
                var item = m_items.SelectedItem as ExportFactory<FrameworkElement, IDemoMetadata>;
                if (item != null)
                {
                    using (var export = item.CreateExport())
                    {
                        m_container.Child = export.Value;
                    }
                }
            };

            m_items.SelectedIndex = 0;
        }

        [ImportMany(DemoMetadataAttribute.DemoContractName)]
        public IList<ExportFactory<FrameworkElement, IDemoMetadata>> Catalog { get; private set; }
    }
}
