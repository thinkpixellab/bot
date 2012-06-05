using System.Windows;

namespace PixelLab.Common
{
    public interface ICommandProxy
    {
        object GetCommandOwner(DependencyObject source);
    }
}
