using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FantaziaDesign.Core;
using FantaziaDesign.Wpf.Core;

namespace FantaziaDesign.Wpf.Controls
{
	[Flags]
	public enum TransformFlags : byte
	{
		None,
		Translational,
		Rotational,
		Skewable = 4,
		Scalable = 8
	}

	//"PART_TopLeftAnchor"
	//"PART_CenterLeftAnchor"
	//"PART_BottomLeftAnchor"
	//"PART_TopMiddleAnchor"
	//"PART_RotateAnchor"
	//"PART_BottomMiddleAnchor"
	//"PART_TopRightAnchor"
	//"PART_CenterRightAnchor"
	//"PART_BottomRightAnchor"
	//"PART_DraggingAnchor"

	[TemplatePart(Name = "PART_DraggingAnchor", Type = typeof(FrameworkElement))]
	[TemplatePart(Name = "PART_RotateAnchor", Type = typeof(FrameworkElement))]
	[TemplatePart(Name = "PART_TopLeftAnchor", Type = typeof(FrameworkElement))]
	[TemplatePart(Name = "PART_CenterLeftAnchor", Type = typeof(FrameworkElement))]
	[TemplatePart(Name = "PART_BottomLeftAnchor", Type = typeof(FrameworkElement))]
	[TemplatePart(Name = "PART_TopMiddleAnchor", Type = typeof(FrameworkElement))]
	[TemplatePart(Name = "PART_BottomMiddleAnchor", Type = typeof(FrameworkElement))]
	[TemplatePart(Name = "PART_TopRightAnchor", Type = typeof(FrameworkElement))]
	[TemplatePart(Name = "PART_CenterRightAnchor", Type = typeof(FrameworkElement))]
	[TemplatePart(Name = "PART_BottomRightAnchor", Type = typeof(FrameworkElement))]
	[TemplatePart(Name = "PART_ContentPresenter", Type = typeof(ContentPresenter))]
	public class BoundaryFrame : ContentControl
	{
		ContentPresenter _ContentPresenter;
		FrameworkElement _TopLeftAnchor;
		FrameworkElement _CenterLeftAnchor;
		FrameworkElement _BottomLeftAnchor;
		FrameworkElement _TopMiddleAnchor;
		FrameworkElement _RotateAnchor;
		FrameworkElement _BottomMiddleAnchor;
		FrameworkElement _TopRightAnchor;
		FrameworkElement _CenterRightAnchor;
		FrameworkElement _BottomRightAnchor;
		FrameworkElement _DraggingAnchor;

		static BoundaryFrame()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(BoundaryFrame), new FrameworkPropertyMetadata(typeof(BoundaryFrame)));
		}

		TransformFlags m_transformFlags;
		//private double _offsetX;
		//private double _offsetY;

		public Vector BoundaryFrameOffset => VisualOffset;

		public Visual LogicalChildVisual
		{
			get
			{
				if (_ContentPresenter != null && VisualTreeHelper.GetChildrenCount(_ContentPresenter) > 0)
				{
					return VisualTreeHelper.GetChild(_ContentPresenter, 0) as Visual;
				}
				return null;
			}
		}

		// Never Allow Scale or Skew
		public virtual bool AllowScale { get => false; set { } }
		public virtual bool AllowSkew { get => false; set { } }

		public virtual bool AllowRotate { get => GetFlags(TransformFlags.Rotational); set => SetFlags(TransformFlags.Rotational, value); }
		public virtual bool AllowTranslate { get => GetFlags(TransformFlags.Translational); set => SetFlags(TransformFlags.Translational, value); }


		public TransformFlags TransformFlags
		{
			get { return (TransformFlags)GetValue(TransformFlagsProperty); }
			set { SetValue(TransformFlagsProperty, value); }
		}

		// Using a DependencyProperty as the backing store for TransformFlags.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty TransformFlagsProperty =
			DependencyProperty.Register("TransformFlags", typeof(TransformFlags), typeof(BoundaryFrame), new PropertyMetadata(TransformFlags.None, OnTransformFlagsPropertyChanged));

		private static void OnTransformFlagsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is BoundaryFrame frame)
			{
				frame.m_transformFlags = (TransformFlags)e.NewValue;
			}
		}

		public double RenderOpacity
		{
			get { return (double)GetValue(RenderOpacityProperty); }
			set { SetValue(RenderOpacityProperty, value); }
		}

		// Using a DependencyProperty as the backing store for RenderOpacity.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty RenderOpacityProperty =
			DependencyProperty.Register("RenderOpacity", typeof(double), typeof(BoundaryFrame), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));

		//public Visual Visual
		//{
		//	get { return (Visual)GetValue(VisualProperty); }
		//	set { SetValue(VisualProperty, value); }
		//}

		//// Using a DependencyProperty as the backing store for Visual.  This enables animation, styling, binding, etc...
		////public static readonly DependencyProperty VisualProperty =
		////	DependencyProperty.Register("Visual", typeof(Visual), typeof(BoundaryFrame), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange, OnVisualPropertyChanged, OnCoerceVisualPropertyValue));

		//public static readonly DependencyProperty VisualProperty =
		//	DependencyProperty.Register("Visual", typeof(Visual), typeof(BoundaryFrame), new PropertyMetadata(null, OnVisualPropertyChanged, OnCoerceVisualPropertyValue));


		//private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		//{
		//	if (d is BoundaryFrame frame)
		//	{
		//		frame.OnVisualHostChanged((Visual)e.NewValue);
		//	}
		//}

		//private static object OnCoerceVisualPropertyValue(DependencyObject d, object value)
		//{
		//	Visual @this = d as Visual;
		//	Visual @other = value as Visual;
		//	if (@this is null)
		//	{
		//		return null;
		//	}
		//	if (@other is null)
		//	{
		//		return null;
		//	}
		//	if (@this == @other)
		//	{
		//		return null;
		//	}
		//	return value;
		//}

		//private void OnVisualHostChanged(Visual newValue)
		//{
		//	if (newValue is null)
		//	{
		//		rect.ZeroRect();
		//	}
		//	var r = VisualTreeHelper.GetDescendantBounds(newValue);
		//	rect.SetFromPointAndSize((float)r.X, (float)r.Y, (float)r.Width, (float)r.Height);
		//	VisualBoundarySize = r.Size;
		//}

		//public Size VisualBoundarySize
		//{
		//	get { return (Size)GetValue(VisualBoundarySizeProperty); }
		//	internal set { SetValue(VisualBoundarySizeProperty, value); }
		//}

		//// Using a DependencyProperty as the backing store for VisualBoundaryRect.  This enables animation, styling, binding, etc...
		//public static readonly DependencyProperty VisualBoundarySizeProperty =
		//	DependencyProperty.Register("VisualBoundarySize", typeof(Size), typeof(BoundaryFrame), new PropertyMetadata(new Size()));

		//public virtual bool AllowScale { get => GetFlags(TransformFlags.Scalable); set => SetFlags(TransformFlags.Scalable, value); }
		//public virtual bool AllowSkew { get => GetFlags(TransformFlags.Skewable); set => SetFlags(TransformFlags.Skewable, value); }


		protected void SetFlags(TransformFlags flags, bool value)
		{
			var newValue = m_transformFlags;
			if (value)
			{
				newValue |= flags;
			}
			else
			{
				newValue &= ~flags;
			}
			if (newValue != m_transformFlags)
			{
				SetValue(TransformFlagsProperty, newValue);
			}
		}

		protected bool GetFlags(TransformFlags flags)
		{
			return (m_transformFlags & flags) == flags;
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			if (!GetTemplateChildInternal($"PART{nameof(_ContentPresenter)}", out _ContentPresenter))
			{
				_ContentPresenter = null;
				throw new ArgumentOutOfRangeException("Cannot find Major ContentPresenter, Name = PART_ContentPresenter");
			}

			if (GetTemplateChildInternal($"PART{nameof(_DraggingAnchor)}", out _DraggingAnchor))
			{

			}
			if (GetTemplateChildInternal($"PART{nameof(_RotateAnchor)}", out _RotateAnchor))
			{

			}
			if (GetTemplateChildInternal($"PART{nameof(_TopLeftAnchor)}", out _TopLeftAnchor))
			{

			}
			if (GetTemplateChildInternal($"PART{nameof(_CenterLeftAnchor)}", out _CenterLeftAnchor))
			{

			}
			if (GetTemplateChildInternal($"PART{nameof(_BottomLeftAnchor)}", out _BottomLeftAnchor))
			{

			}
			if (GetTemplateChildInternal($"PART{nameof(_TopMiddleAnchor)}", out _TopMiddleAnchor))
			{

			}
			if (GetTemplateChildInternal($"PART{nameof(_BottomMiddleAnchor)}", out _BottomMiddleAnchor))
			{

			}
			if (GetTemplateChildInternal($"PART{nameof(_TopRightAnchor)}", out _TopRightAnchor))
			{

			}
			if (GetTemplateChildInternal($"PART{nameof(_CenterRightAnchor)}", out _CenterRightAnchor))
			{

			}
			if (GetTemplateChildInternal($"PART{nameof(_BottomRightAnchor)}", out _BottomRightAnchor))
			{

			}
		}

		private bool GetTemplateChildInternal<T>(string name, out T control) where T : DependencyObject
		{
			control = GetTemplateChild(name) as T;
			if (control != null)
			{
				return true;
			}
			return false;
		}


		public bool MoveBoundaryFrame(Vector deltaOffset)
		{
			if (IsArrangeValid)
			{
				VisualOffset += deltaOffset;
				return true;
			}
			return false;
		}

		protected override Size MeasureOverride(Size constraint)
		{
			var size = base.MeasureOverride(constraint);
			return size;
		}

		protected override Size ArrangeOverride(Size arrangeBounds)
		{
			if (VisualChildrenCount > 0)
			{
				var arrangeRect = new Rect(arrangeBounds);
				UIElement child = (UIElement)GetVisualChild(0);
				if (child != null)
				{
					child.Arrange(arrangeRect);
				}
			}
			return arrangeBounds;
		}
	}
}
