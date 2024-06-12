using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FantaziaDesign.Wpf.Core
{
    public static class Draggable
    {
		public static bool GetIsDraggable(DependencyObject obj)
		{
			return (bool)obj.GetValue(IsDraggableProperty);
		}

		public static void SetIsDraggable(DependencyObject obj, bool value)
		{
			obj.SetValue(IsDraggableProperty, value);
		}

		// Using a DependencyProperty as the backing store for IsDraggable.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IsDraggableProperty =
			DependencyProperty.RegisterAttached("IsDraggable", typeof(bool), typeof(Draggable), new FrameworkPropertyMetadata(false));
	}
}
