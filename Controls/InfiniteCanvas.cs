using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FantaziaDesign.Wpf.Controls
{
	public class InfiniteCanvas : Panel
	{
		private Rect _childrenUnionRect = new Rect();

		[TypeConverter("System.Windows.LengthConverter, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, Custom=null")]
		[AttachedPropertyBrowsableForChildren]
		public static double GetOffsetX(DependencyObject obj)
		{
			return (double)obj.GetValue(OffsetXProperty);
		}

		public static void SetOffsetX(DependencyObject obj, double value)
		{
			obj.SetValue(OffsetXProperty, value);
		}

		// Using a DependencyProperty as the backing store for OffsetX.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty OffsetXProperty =
			//Canvas.LeftProperty.AddOwner(typeof(InfiniteCanvas), new FrameworkPropertyMetadata(double.NaN, new PropertyChangedCallback(OnPlacementChanged)));
			DependencyProperty.RegisterAttached("OffsetX", typeof(double), typeof(InfiniteCanvas), new FrameworkPropertyMetadata(double.NaN, new PropertyChangedCallback(OnPlacementChanged)), new ValidateValueCallback(IsDoubleFiniteOrNaN));

		[TypeConverter("System.Windows.LengthConverter, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, Custom=null")]
		[AttachedPropertyBrowsableForChildren]
		public static double GetOffsetY(DependencyObject obj)
		{
			return (double)obj.GetValue(OffsetYProperty);
		}

		public static void SetOffsetY(DependencyObject obj, double value)
		{
			obj.SetValue(OffsetYProperty, value);
		}

		// Using a DependencyProperty as the backing store for OffsetY.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty OffsetYProperty =
			//Canvas.TopProperty.AddOwner(typeof(InfiniteCanvas), new FrameworkPropertyMetadata(double.NaN, new PropertyChangedCallback(OnPlacementChanged)));
			DependencyProperty.RegisterAttached("OffsetY", typeof(double), typeof(InfiniteCanvas), new FrameworkPropertyMetadata(double.NaN, new PropertyChangedCallback(OnPlacementChanged)), new ValidateValueCallback(IsDoubleFiniteOrNaN));

		private static void OnPlacementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			UIElement child = d as UIElement;
			if (child != null)
			{
				var canvas = VisualTreeHelper.GetParent(child) as InfiniteCanvas;
				if (canvas != null)
				{
					canvas.OnChildrenPlacementChanged(child, e.Property, (double)e.OldValue, (double)e.NewValue);
				}
			}
		}

		protected virtual void OnChildrenPlacementChanged(UIElement child, DependencyProperty property, double oldValue, double newValue)
		{
			InvalidateMeasure();
			//InvalidateArrange();

			//System.Diagnostics.Debug.WriteLine($"Child : {child}, Property : {property}, oldValue : {oldValue}, newValue : {newValue}");

		}

		internal static bool IsDoubleFiniteOrNaN(object o)
		{
			return !double.IsInfinity((double)o);
		}

		private void ClearUnion()
		{
			_childrenUnionRect.X = 0;
			_childrenUnionRect.Y = 0;
			_childrenUnionRect.Width = 0;
			_childrenUnionRect.Height = 0;
		}

		protected override Size MeasureOverride(Size constraint)
		{
			ClearUnion();
			Size availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
			foreach (var item in InternalChildren)
			{
				UIElement uielement = (UIElement)item;
				if (uielement != null)
				{
					uielement.Measure(availableSize);
					double x = 0.0;
					double y = 0.0;
					double offsetX = GetOffsetX(uielement);
					if (!double.IsNaN(offsetX))
					{
						x = offsetX;
					}

					double offsetY = GetOffsetY(uielement);
					if (!double.IsNaN(offsetY))
					{
						y = offsetY;
					}

					_childrenUnionRect.Union(new Rect(new Point(x, y), uielement.DesiredSize));

					//uielement.Arrange(new Rect(new Point(x, y), uielement.DesiredSize));
				}
			}
			//System.Diagnostics.Debug.WriteLine($"MeasureOverride : {_childrenUnionRect}");
			return _childrenUnionRect.Size;

			// return base.MeasureOverride(constraint);
		}

		protected override Size ArrangeOverride(Size arrangeSize)
		{
			Rect rect = new Rect(arrangeSize);

			foreach (object obj in InternalChildren)
			{
				UIElement uielement = (UIElement)obj;
				if (uielement != null)
				{
					double x = 0.0;
					double y = 0.0;
					double offsetX = GetOffsetX(uielement);
					if (!double.IsNaN(offsetX))
					{
						x = offsetX;
					}

					double offsetY = GetOffsetY(uielement);
					if (!double.IsNaN(offsetY))
					{
						y = offsetY;
					}
					uielement.Arrange(new Rect(new Point(x, y), uielement.DesiredSize));
				}
			}

			rect.Union(_childrenUnionRect);
			//System.Diagnostics.Debug.WriteLine($"ArrangeOverride : {rect}");
			return rect.Size;
			//return arrangeSize;
		}
	}
}
