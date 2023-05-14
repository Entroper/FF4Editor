using System.Windows;
using System.Windows.Media;

namespace FF4Editor;

public static class GetAncestor
{
    public static TAncestor? GetAncestorOfType<TAncestor>(this DependencyObject reference)
        where TAncestor : DependencyObject
    {
        var parent = VisualTreeHelper.GetParent(reference);
        while (!(parent is TAncestor) && parent != null)
        {
            parent = VisualTreeHelper.GetParent(parent);
        }

        return (TAncestor?)parent;
    }
}
