using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Windows;
using PixelLab.Common;

namespace PixelLab.SL.Demo.Core {
  public class DemoMetadata {
    public DemoMetadata(FunctionExportDefinition<FrameworkElement> export) {
      Name = (string)export.Metadata["Name"];
      Description = (string)export.Metadata["Description"];
      ExportDefinition = export;
    }

    public string Name { get; private set; }
    public string Description { get; private set; }
    public FunctionExportDefinition<FrameworkElement> ExportDefinition { get; private set; }

    public FrameworkElement CreateElement() {
      return ExportDefinition.GetValue();
    }

    public FrameworkElement Instance { get { return CreateElement(); } }

    public static IList<DemoMetadata> GetDemos(Assembly sourceAssembly, string firstName = null) {
      Contract.Requires(sourceAssembly != null);

      var catalog = new AssemblyCatalog(sourceAssembly);
      var items = (from export in catalog.GetExports<FrameworkElement>()
                   where export.ContractName == DemoMetadataAttribute.DemoContractName
                   select new DemoMetadata(export))
                   .OrderBy(_ => _.Name)
                   .ToList();

      if (firstName != null) {
        var welcome = items.Where(metadata => metadata.Name.Equals(firstName)).FirstOrDefault();
        if (welcome != null) {
          items.Remove(welcome);
          items.Insert(0, welcome);
        }
      }

      return items.ToReadOnlyCollection();
    }

  }
}
