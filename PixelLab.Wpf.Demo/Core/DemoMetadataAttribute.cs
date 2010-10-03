using System;
using System.ComponentModel.Composition;
using System.Windows;

namespace PixelLab.Demo.Core {

  [MetadataAttribute]
  [AttributeUsage(AttributeTargets.Class)]
  public class DemoMetadataAttribute : ExportAttribute {
    public DemoMetadataAttribute(string name) : this(name, string.Empty) { }
    public DemoMetadataAttribute(string name, string description)
      : base(DemoContractName, typeof(FrameworkElement)) {
      Name = name;
      Description = description;
    }

    public string Name { get; private set; }
    public string Description { get; private set; }

    public const string DemoContractName = "PixelLab.SL.Demo";
  }
}
