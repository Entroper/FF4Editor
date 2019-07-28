using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FF4MapEdit
{
    public static class GetAncestor
    {
        public static TAncestor GetAncestorOfType<TAncestor>(this DependencyObject reference)
            where TAncestor : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(reference);
            while (!(parent is TAncestor) && parent != null)
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return (TAncestor)parent;
        }
    }
}
