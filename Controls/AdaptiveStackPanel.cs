using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FantaziaDesign.Wpf.Controls
{
	public static class AdaptiveLayout
	{
		public static double GetMinAdaptiveWidth(DependencyObject obj)
		{
			return (double)obj.GetValue(MinAdaptiveWidthProperty);
		}

		public static void SetMinAdaptiveWidth(DependencyObject obj, double value)
		{
			obj.SetValue(MinAdaptiveWidthProperty, value);
		}

		// Using a DependencyProperty as the backing store for MinAdaptiveWidth.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MinAdaptiveWidthProperty =
			DependencyProperty.RegisterAttached("MinAdaptiveWidth", typeof(double), typeof(AdaptiveLayout), new FrameworkPropertyMetadata(double.NaN,new PropertyChangedCallback(OnAdaptiveLayoutChanged)));

		public static double GetMaxAdaptiveWidth(DependencyObject obj)
		{
			return (double)obj.GetValue(MaxAdaptiveWidthProperty);
		}

		public static void SetMaxAdaptiveWidth(DependencyObject obj, double value)
		{
			obj.SetValue(MaxAdaptiveWidthProperty, value);
		}

		// Using a DependencyProperty as the backing store for MaxAdaptiveWidth.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MaxAdaptiveWidthProperty =
			DependencyProperty.RegisterAttached("MaxAdaptiveWidth", typeof(double), typeof(AdaptiveLayout), new FrameworkPropertyMetadata(double.NaN, new PropertyChangedCallback(OnAdaptiveLayoutChanged)));

		public static double GetMinAdaptiveHeight(DependencyObject obj)
		{
			return (double)obj.GetValue(MinAdaptiveHeightProperty);
		}

		public static void SetMinAdaptiveHeight(DependencyObject obj, double value)
		{
			obj.SetValue(MinAdaptiveHeightProperty, value);
		}

		// Using a DependencyProperty as the backing store for MinAdaptiveHeight.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MinAdaptiveHeightProperty =
			DependencyProperty.RegisterAttached("MinAdaptiveHeight", typeof(double), typeof(AdaptiveLayout), new FrameworkPropertyMetadata(double.NaN, new PropertyChangedCallback(OnAdaptiveLayoutChanged)));

		public static double GetMaxAdaptiveHeight(DependencyObject obj)
		{
			return (double)obj.GetValue(MaxAdaptiveHeightProperty);
		}

		public static void SetMaxAdaptiveHeight(DependencyObject obj, double value)
		{
			obj.SetValue(MaxAdaptiveHeightProperty, value);
		}

		// Using a DependencyProperty as the backing store for MaxAdaptiveHeight.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MaxAdaptiveHeightProperty =
			DependencyProperty.RegisterAttached("MaxAdaptiveHeight", typeof(double), typeof(AdaptiveLayout), new FrameworkPropertyMetadata(double.NaN, new PropertyChangedCallback(OnAdaptiveLayoutChanged)));

		private static void OnAdaptiveLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var panel = VisualTreeHelper.GetParent(d) as Panel;
			if (panel is null)
			{
				return;
			}
			if (panel.IsMeasureValid)
			{
				panel.InvalidateMeasure();
			}
		}
	}

	public class AdaptiveStackPanel : ScrollablePanel
	{
		public Orientation Orientation
		{
			get { return (Orientation)GetValue(OrientationProperty); }
			set { SetValue(OrientationProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Orientation.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty OrientationProperty =
			StackPanel.OrientationProperty.AddOwner(typeof(AdaptiveStackPanel), new FrameworkPropertyMetadata(Orientation.Vertical, FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(OnOrientationChanged)));
		
		private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			(d as AdaptiveStackPanel).OnOrientationChanged();
		}

		protected virtual void OnOrientationChanged()
		{
			InvalidateMeasure();
			if (CanScroll)
			{
				ScrollData.ClearLayout();
			}
		}

		public override void LineDown()
		{
			throw new NotImplementedException();
		}

		public override void LineLeft()
		{
			throw new NotImplementedException();
		}

		public override void LineRight()
		{
			throw new NotImplementedException();
		}

		public override void LineUp()
		{
			throw new NotImplementedException();
		}

		public override void MouseWheelDown()
		{
			throw new NotImplementedException();
		}

		public override void MouseWheelLeft()
		{
			throw new NotImplementedException();
		}

		public override void MouseWheelRight()
		{
			throw new NotImplementedException();
		}

		public override void MouseWheelUp()
		{
			throw new NotImplementedException();
		}

		public override void PageDown()
		{
			throw new NotImplementedException();
		}

		public override void PageLeft()
		{
			throw new NotImplementedException();
		}

		public override void PageRight()
		{
			throw new NotImplementedException();
		}

		public override void PageUp()
		{
			throw new NotImplementedException();
		}

		protected override Size ArrangeChildren(Size arrangeSize, Vector computedOffset)
		{
			throw new NotImplementedException();
		}

		protected override Size MeasureChildren(Size constraint, Size childrenConstraint)
		{
			throw new NotImplementedException();

			foreach (UIElement item in InternalChildren)
			{

			}
		}
	}
}
