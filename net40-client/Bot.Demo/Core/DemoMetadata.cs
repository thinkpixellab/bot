using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Windows;
using PixelLab.Common;
using PixelLab.Wpf.Demo.Core;

namespace PixelLab.Demo.Core
{
    public class DemoMetadata
    {
        public DemoMetadata(string name, string description, Func<FrameworkElement> factory)
        {
            Name = name;
            Description = description;
            m_factory = factory;
        }

        public static DemoMetadata Create(FunctionExportDefinition<FrameworkElement> export)
        {
            return new DemoMetadata((string)export.Metadata["Name"], (string)export.Metadata["Description"], export.GetValue);
        }

        public string Name { get; private set; }
        public string Description { get; private set; }

        public FrameworkElement CreateElement()
        {
            return m_factory();
        }

        public FrameworkElement Instance { get { return CreateElement(); } }

        private readonly Func<FrameworkElement> m_factory;

        public static IList<DemoMetadata> GetDemos(Assembly sourceAssembly, string firstName = null)
        {
            Contract.Requires(sourceAssembly != null);

            var catalog = new AssemblyCatalog(sourceAssembly);
            var mefItems = from export in catalog.GetExports<FrameworkElement>()
                           where export.ContractName == DemoMetadataAttribute.DemoContractName
                           select DemoMetadata.Create(export);

            var xamlItems = XamlComponent.GetDemos(sourceAssembly);
            var items = mefItems.Concat(xamlItems).OrderBy(_ => _.Name).ToList();

            if (firstName != null)
            {
                var welcome = items.Where(metadata => metadata.Name.Equals(firstName)).FirstOrDefault();
                if (welcome != null)
                {
                    items.Remove(welcome);
                    items.Insert(0, welcome);
                }
            }

            return items.ToReadOnlyCollection();
        }
    }
}
