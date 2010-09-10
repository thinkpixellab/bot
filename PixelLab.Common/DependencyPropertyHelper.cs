using System;
using System.Windows;

namespace PixelLab.Common {
  public static class DependencyPropertyHelper {
    public static PropertyChangedCallback GetTypedDPChangedHandler<TElement, TProperty>(Action<TElement, TProperty, TProperty> handler) where TElement : DependencyObject {
      return new PropertyChangedCallback((element, args) => {
        handler((TElement)element, (TProperty)args.NewValue, (TProperty)args.OldValue);
      });
    }

    public static PropertyMetadata GetTypePropertyMetadata<TElement, TProperty>(Action<TElement, TProperty, TProperty> handler) where TElement : DependencyObject {
      return new PropertyMetadata(GetTypedDPChangedHandler<TElement, TProperty>(handler));
    }

    public static DependencyProperty Register<TElement, TProperty>(string name, Action<TElement, TProperty, TProperty> changeHandler = null) where TElement : DependencyObject {
      PropertyMetadata metadata;
      if (changeHandler == null) {
        metadata = null;
      }
      else {
        metadata = GetTypePropertyMetadata(changeHandler);
      }

      return DependencyProperty.Register(name, typeof(TProperty), typeof(TElement), metadata);

    }

    public static DependencyProperty RegisterAttached<TOwner, TTarget, TProperty>(string name, Action<TTarget, TProperty, TProperty> changeHandler = null) where TTarget : DependencyObject {
      PropertyMetadata metadata;
      if (changeHandler == null) {
        metadata = null;
      }
      else {
        metadata = GetTypePropertyMetadata(changeHandler);
      }

      return DependencyProperty.Register(name, typeof(TProperty), typeof(TOwner), metadata);
    }

  }
}
