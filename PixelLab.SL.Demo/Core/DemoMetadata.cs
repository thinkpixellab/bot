using System.ComponentModel.Composition.Primitives;

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

  }
}
