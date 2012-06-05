using System;

namespace PixelLab.Common
{
    public interface IDynamicValue<T>
    {
        T Value { get; }
        event EventHandler ValueChanged;
    }
}
