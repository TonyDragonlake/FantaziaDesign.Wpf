using FantaziaDesign.Wpf.Core;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FantaziaDesign.Wpf.Controls
{
	public sealed class BoundaryDecorator : Decorator
	{
		private Thickness _boundaryThicknessCache;
		private StreamGeometry _boundaryGeometry;
		private StreamGeometry _borderGeometry;

		public Brush BoundaryBrush
		{
			get { return (Brush)GetValue(BoundaryBrushProperty); }
			set { SetValue(BoundaryBrushProperty, value); }
		}

		// Using a DependencyProperty as the backing store for BoundaryBrush.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty BoundaryBrushProperty =
			DependencyProperty.Register("BoundaryBrush", typeof(Brush), typeof(BoundaryDecorator), new FrameworkPropertyMetadata(null,FrameworkPropertyMetadataOptions.AffectsRender));

		public Brush BorderBrush
		{
			get { return (Brush)GetValue(BorderBrushProperty); }
			set { SetValue(BorderBrushProperty, value); }
		}

		// Using a DependencyProperty as the backing store for BorderBrush.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty BorderBrushProperty =
			DependencyProperty.Register("BorderBrush", typeof(Brush), typeof(BoundaryDecorator), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));


		public Thickness BorderThickness
		{
			get { return (Thickness)GetValue(BorderThicknessProperty); }
			set { SetValue(BorderThicknessProperty, value); }
		}

		// Using a DependencyProperty as the backing store for BorderThickness.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty BorderThicknessProperty =
			DependencyProperty.Register("BorderThickness", typeof(Thickness), typeof(BoundaryDecorator), new FrameworkPropertyMetadata(default(Thickness), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender), new ValidateValueCallback(BorderLikeControlUtil.IsThicknessValid));

		public Thickness BorderMargin
		{
			get { return (Thickness)GetValue(BorderMarginProperty); }
			set { SetValue(BorderMarginProperty, value); }
		}

		// Using a DependencyProperty as the backing store for BorderMargin.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty BorderMarginProperty =
			DependencyProperty.Register("BorderMargin", typeof(Thickness), typeof(BoundaryDecorator), new FrameworkPropertyMetadata(default(Thickness), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender), new ValidateValueCallback(BorderLikeControlUtil.IsThicknessValid));

		public Thickness BorderPadding
		{
			get { return (Thickness)GetValue(BorderPaddingProperty); }
			set { SetValue(BorderPaddingProperty, value); }
		}

		// Using a DependencyProperty as the backing store for BorderPadding.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty BorderPaddingProperty =
			DependencyProperty.Register("BorderPadding", typeof(Thickness), typeof(BoundaryDecorator), new FrameworkPropertyMetadata(default(Thickness), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender), new ValidateValueCallback(BorderLikeControlUtil.IsThicknessValid));

		public double RenderOpacity
		{
			get { return (double)GetValue(RenderOpacityProperty); }
			set { SetValue(RenderOpacityProperty, value); }
		}

		// Using a DependencyProperty as the backing store for RenderOpacity.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty RenderOpacityProperty =
			DependencyProperty.Register("RenderOpacity", typeof(double), typeof(BoundaryDecorator), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));


		public Thickness BoundaryThickness
		{
			get
			{
				return ControlMetrics.CombineThickness(BorderThickness, BorderMargin, BorderPadding);
			}
		}

		protected override Size MeasureOverride(Size constraint)
		{
			_boundaryThicknessCache = BoundaryThickness;
			UIElement child = Child;
			Size result = default(Size);
			if (child != null)
			{
				child.Measure(ControlMetrics.NewDeflateSize(constraint, _boundaryThicknessCache));
				result = ControlMetrics.NewInflateSize(child.DesiredSize, _boundaryThicknessCache);
			}
			else
			{
				ControlMetrics.InflateSize(ref result, _boundaryThicknessCache);
			}
			return result;
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			var drawingRect = new Rect(finalSize);
			var isBoundaryZero = ControlMetrics.IsThicknessZero(_boundaryThicknessCache);
			UIElement child = Child;
			if (child != null)
			{
				Rect finalRect = new Rect(finalSize);
				if (!isBoundaryZero)
				{
					ControlMetrics.DeflateRect(ref finalRect, _boundaryThicknessCache);
				}
				child.Arrange(finalRect);
			}
			if (isBoundaryZero)
			{
				_boundaryGeometry = null;
			}
			else
			{
				if (_boundaryGeometry is null)
				{
					_boundaryGeometry = new StreamGeometry();
				}
				using (var ctx = _boundaryGeometry.Open())
				{
					BorderLikeControlUtil.GenerateSimpleBoundaryGeometry(ctx, drawingRect, _boundaryThicknessCache);
				}
			}
			var borderThickness = BorderThickness;
			if (ControlMetrics.IsThicknessZero(borderThickness))
			{
				_borderGeometry = null;
			}
			else
			{
				if (_borderGeometry is null)
				{
					_borderGeometry = new StreamGeometry();
				}
				using (var ctx = _borderGeometry.Open())
				{
					ControlMetrics.DeflateRect(ref drawingRect, BorderMargin);
					BorderLikeControlUtil.GenerateSimpleBoundaryGeometry(ctx, drawingRect, borderThickness);
				}
			}
			return finalSize;
		}

		protected override void OnRender(DrawingContext dc)
		{
			var renderOpacity = RenderOpacity;
			if (renderOpacity > 1)
			{
				renderOpacity = 1;
			}
			if (renderOpacity < 0)
			{
				renderOpacity = 0;
			}
			if (renderOpacity == 0)
			{
				return;
			}
			var borderBrush = BorderBrush;
			var boundaryBrush = BoundaryBrush;
			var canDrawBorder = !(borderBrush is null || _borderGeometry is null);
			var canDrawBoundary = !(boundaryBrush is null || _boundaryGeometry is null);
			bool shouldApplyOpacity = (renderOpacity != 1) && (canDrawBorder || canDrawBoundary);
			if (shouldApplyOpacity)
			{
				dc.PushOpacity(renderOpacity);
			}
			if (canDrawBorder)
			{
				dc.DrawGeometry(borderBrush, null, _borderGeometry);
			}
			if (canDrawBoundary)
			{
				dc.DrawGeometry(boundaryBrush, null, _boundaryGeometry);
			}
			if (shouldApplyOpacity)
			{
				dc.Pop();
			}
		}

	}
}
