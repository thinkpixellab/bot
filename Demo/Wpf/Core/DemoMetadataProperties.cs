using System.Diagnostics;
using System.Windows;
using Microsoft.Xaml.Tools.XamlDom;

namespace PixelLab.Wpf.Demo.Core
{
    public static class DemoMetadataProperties
    {
        public static readonly DependencyProperty DemoNameProperty = DependencyProperty.RegisterAttached("DemoName", typeof(string), typeof(DemoMetadataProperties));

        public static string GetDemoName(UIElement element)
        {
            return (string)element.GetValue(DemoNameProperty);
        }

        public static void SetDemoName(UIElement element, string value)
        {
            element.SetValue(DemoNameProperty, value);
        }

        public static readonly DependencyProperty DemoDescriptionProperty = DependencyProperty.RegisterAttached("DemoDescription", typeof(string), typeof(DemoMetadataProperties));

        public static string GetDemoDescription(UIElement element)
        {
            return (string)element.GetValue(DemoDescriptionProperty);
        }

        public static void SetDemoDescription(UIElement element, string value)
        {
            element.SetValue(DemoDescriptionProperty, value);
        }

        public static T GetAttatchedPropertyValueOrDefault<T>(this XamlDomObject sourceObject, DependencyProperty property)
        {
            T value;
            sourceObject.TryGetAttatchedPropertyValue(property, out value);
            return value;
        }

        public static bool TryGetAttatchedPropertyValue<T>(this XamlDomObject sourceObject, DependencyProperty property, out T value)
        {
            Debug.Assert(typeof(T).IsAssignableFrom(property.PropertyType));
            var demoMember = sourceObject.GetAttachableMemberNode(property.OwnerType, property.Name);

            if (demoMember != null && demoMember.Item is XamlDomValue)
            {
                var item = (XamlDomValue)demoMember.Item;
                value = (T)item.Value;
                return true;
            }
            else
            {
                value = default(T);
                return false;
            }
        }
    }
}
