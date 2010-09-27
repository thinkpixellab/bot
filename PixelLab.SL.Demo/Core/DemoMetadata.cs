using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Reflection;
using System.Windows;
using PixelLab.Common;
using System.Diagnostics.Contracts;

namespace PixelLab.SL.Demo.Core {
  public class DemoMetadata {
    public DemoMetadata(string name, ExportDefinition definition, ComposablePartDefinition partDefinition) {
      Name = name;
      ExportDefinition = definition;
      PartDefinition = partDefinition;
    }

    public string Name { get; private set; }
    public ExportDefinition ExportDefinition { get; private set; }
    public ComposablePartDefinition PartDefinition { get; private set; }

    public FrameworkElement CreateElement() {
      return (FrameworkElement)PartDefinition.CreatePart().GetExportedValue(ExportDefinition);
    }

    public static IList<DemoMetadata> GetDemos(Assembly sourceAssembly, string firstName = null) {
      Contract.Requires(sourceAssembly != null);

      var catalog = new AssemblyCatalog(sourceAssembly);
      var items = (from part in catalog.Parts
                   from definition in part.ExportDefinitions
                   where definition.ContractName == DemoMetadataAttribute.DemoContractName
                   select new DemoMetadata(definition.Metadata["Name"] as string, definition, part)
                   )
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
