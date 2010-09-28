using System.Windows;

namespace PixelLab.Working {
  public static class DemoMetadataProperties {

    public static readonly DependencyProperty DemoNameProperty = DependencyProperty.RegisterAttached("DemoName", typeof(string), typeof(DemoMetadataProperties));

    public static string GetDemoName(UIElement element) {
      return (string)element.GetValue(DemoNameProperty);
    }

    public static void SetDemoName(UIElement element, string value) {
      element.SetValue(DemoNameProperty, value);
    }

  }
}
