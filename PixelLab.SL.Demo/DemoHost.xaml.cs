using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using PixelLab.Common;
using PixelLab.SL.Demo.Core;

namespace PixelLab.SL.Demo {
  public partial class DemoHost : UserControl {
    public DemoHost() {
      InitializeComponent();
      CompositionInitializer.SatisfyImports(this);
      m_layoutRoot.Children.Add(Demos.Random().Value);
    }

    [ImportMany(typeof(FrameworkElement))]
    public IList<Lazy<FrameworkElement, IDemoMetadata>> Demos {
      get { return m_demos; }
    }

    private readonly List<Lazy<FrameworkElement, IDemoMetadata>> m_demos = new List<Lazy<FrameworkElement, IDemoMetadata>>();
  }
}
