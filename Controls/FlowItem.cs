using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using FantaziaDesign.Core;
using FantaziaDesign.Model;
using FantaziaDesign.Wpf.Core;
using FantaziaDesign.Wpf.Input;

namespace FantaziaDesign.Wpf.Controls
{
	public enum FlowItemType
	{
		ContentHost,
		Connector
	}

	public interface IFlowItemModel : INotifyPropertyChanged, IPoint<double>, IContentTemplateHost
	{
		FlowItemType FlowItemType { get; }
	}

	public class FlowItem : ListBoxItem
	{
		private static readonly Type s_typeOfThis = typeof(FlowItem);

		private ConnectorDrawingLogic m_connectorLogic = new ConnectorDrawingLogic();

		private bool needUpdateTransform = false;

		private Thumb _DraggableThumb;
		private bool _isDragEventListened;
		private bool _isDraggable;
		private static Action<ListBoxItem, MouseButton> HandleMouseButtonDown_ListBoxItem;

		static FlowItem()
		{
			DefaultStyleKeyProperty.OverrideMetadata(s_typeOfThis, new FrameworkPropertyMetadata(s_typeOfThis));
			KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(s_typeOfThis, new FrameworkPropertyMetadata(KeyboardNavigationMode.Once));
			KeyboardNavigation.TabNavigationProperty.OverrideMetadata(s_typeOfThis, new FrameworkPropertyMetadata(KeyboardNavigationMode.Local));
			CommandHelper.RegisterCommandHandler(s_typeOfThis, ApplicationCommands.Delete, new ExecutedRoutedEventHandler(OnDeleteCommand));
		}

		private static void OnDeleteCommand(object sender, ExecutedRoutedEventArgs e)
		{
			if (sender is FlowItem item)
			{
				var parentView = item.ParentView;
				if (parentView != null)
				{
					parentView.TryRemoveItem(item);
				}
			}
		}

		public FlowItem()
		{
			LayoutUpdated += FlowItem_LayoutUpdated;
		}

		private void FlowItem_LayoutUpdated(object sender, EventArgs e)
		{
			if (needUpdateTransform)
			{
				this.UpdateAnchorItemsCoordinateTransformRelativeTo(ParentPanel);
				needUpdateTransform = false;
				SetCurrentValue(ShouldUpdateCoordinateTransformProperty, false);
			}
		}

		protected static readonly DependencyProperty ShouldUpdateCoordinateTransformProperty = 
			Anchorable.ShouldUpdateCoordinateTransformProperty.AddOwner(s_typeOfThis, new PropertyMetadata(false, OnShouldUpdateCoordinateTransformChanged));

		private static void OnShouldUpdateCoordinateTransformChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is FlowItem item)
			{
				item.needUpdateTransform = (bool)e.NewValue;
			}
		}

		public FlowItemsView ParentView => ItemsControl.ItemsControlFromItemContainer(this) as FlowItemsView;

		public Panel ParentPanel => ParentView?.GetItemsPanelFromItemsControl();

		public FlowItemType FlowItemType
		{
			get { return (FlowItemType)GetValue(FlowItemTypeProperty); }
			set { SetValue(FlowItemTypeProperty, value); }
		}

		// Using a DependencyProperty as the backing store for FlowItemType.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty FlowItemTypeProperty =
			DependencyProperty.Register("FlowItemType", typeof(FlowItemType), s_typeOfThis, new FrameworkPropertyMetadata(FlowItemType.ContentHost, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

		public bool IsDraggable
		{
			get { return (bool)GetValue(IsDraggableProperty); }
			set { SetValue(IsDraggableProperty, value); }
		}

		// Using a DependencyProperty as the backing store for IsDraggable.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IsDraggableProperty =
			Draggable.IsDraggableProperty.AddOwner(s_typeOfThis, new FrameworkPropertyMetadata(false, OnIsDraggableChanged));

		private static void OnIsDraggableChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
		{
			if (d is FlowItem item)
			{
				item._isDraggable = (bool)args.NewValue;
			}
		}

		public Point ConnectorStartPoint
		{
			get { return (Point)GetValue(ConnectorStartPointProperty); }
			set { SetValue(ConnectorStartPointProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ConnectorStartPoint.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ConnectorStartPointProperty =
			ConnectorControl.ConnectorStartPointProperty.AddOwner(s_typeOfThis, new PropertyMetadata(default(Point), OnConnectorPointsChanged));
			// DependencyProperty.Register("ConnectorStartPoint", typeof(Point), s_typeOfThis, new PropertyMetadata(default(Point), OnConnectorPointsChanged));

		public Point ConnectorEndPoint
		{
			get { return (Point)GetValue(ConnectorEndPointProperty); }
			set { SetValue(ConnectorEndPointProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ConnectorEndPoint.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ConnectorEndPointProperty =
			ConnectorControl.ConnectorEndPointProperty.AddOwner(s_typeOfThis, new PropertyMetadata(default(Point), OnConnectorPointsChanged));
			//DependencyProperty.Register("ConnectorEndPoint", typeof(Point), s_typeOfThis, new PropertyMetadata(default(Point), OnConnectorPointsChanged));

		public double StrokeThickness
		{
			get { return (double)GetValue(StrokeThicknessProperty); }
			set { SetValue(StrokeThicknessProperty, value); }
		}

		// Using a DependencyProperty as the backing store for StrokeThickness.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty StrokeThicknessProperty = 
			Shape.StrokeThicknessProperty.AddOwner(s_typeOfThis, new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnPensChanged)));

		public double ExtendThickness
		{
			get { return (double)GetValue(ExtendThicknessProperty); }
			set { SetValue(ExtendThicknessProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ExtendThickness.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ExtendThicknessProperty =
			ConnectorControl.ExtendThicknessProperty.AddOwner(s_typeOfThis, new PropertyMetadata(4.0, OnHitTestStrokeChanged, CoarceMinimumHitTestStroke));
			//DependencyProperty.Register("ExtendThickness", typeof(double), s_typeOfThis, new PropertyMetadata(4.0, OnHitTestStrokeChanged, CoarceMinimumHitTestStroke));

		public Brush Stroke
		{
			get { return (Brush)GetValue(StrokeProperty); }
			set { SetValue(StrokeProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Stroke.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty StrokeProperty = 
			Shape.StrokeProperty.AddOwner(s_typeOfThis, new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender, new PropertyChangedCallback(OnPensChanged)));

		public PenLineCap StrokeStartLineCap
		{
			get { return (PenLineCap)GetValue(StrokeStartLineCapProperty); }
			set { SetValue(StrokeStartLineCapProperty, value); }
		}

		// Using a DependencyProperty as the backing store for StrokeStartLineCap.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty StrokeStartLineCapProperty = 
			Shape.StrokeStartLineCapProperty.AddOwner(s_typeOfThis, new FrameworkPropertyMetadata(PenLineCap.Flat, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnPensChanged)));

		public PenLineCap StrokeEndLineCap
		{
			get { return (PenLineCap)GetValue(StrokeEndLineCapProperty); }
			set { SetValue(StrokeEndLineCapProperty, value); }
		}

		// Using a DependencyProperty as the backing store for StrokeEndLineCap.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty StrokeEndLineCapProperty =
			Shape.StrokeEndLineCapProperty.AddOwner(s_typeOfThis, new FrameworkPropertyMetadata(PenLineCap.Flat, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnPensChanged)));

		public PenLineCap StrokeDashCap
		{
			get { return (PenLineCap)GetValue(StrokeDashCapProperty); }
			set { SetValue(StrokeDashCapProperty, value); }
		}

		// Using a DependencyProperty as the backing store for StrokeDashCap.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty StrokeDashCapProperty =
			Shape.StrokeDashCapProperty.AddOwner(s_typeOfThis, new FrameworkPropertyMetadata(PenLineCap.Flat, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnDashStyleChanged)));

		public PenLineJoin StrokeLineJoin
		{
			get { return (PenLineJoin)GetValue(StrokeLineJoinProperty); }
			set { SetValue(StrokeLineJoinProperty, value); }
		}

		// Using a DependencyProperty as the backing store for StrokeLineJoin.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty StrokeLineJoinProperty =
			Shape.StrokeLineJoinProperty.AddOwner(s_typeOfThis, new FrameworkPropertyMetadata(PenLineJoin.Miter, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnPensChanged)));

		public double StrokeMiterLimit
		{
			get { return (double)GetValue(StrokeMiterLimitProperty); }
			set { SetValue(StrokeMiterLimitProperty, value); }
		}

		// Using a DependencyProperty as the backing store for StrokeMiterLimit.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty StrokeMiterLimitProperty =
			Shape.StrokeMiterLimitProperty.AddOwner(s_typeOfThis, new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnPensChanged)));

		public double StrokeDashOffset
		{
			get { return (double)GetValue(StrokeDashOffsetProperty); }
			set { SetValue(StrokeDashOffsetProperty, value); }
		}

		// Using a DependencyProperty as the backing store for StrokeDashOffset.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty StrokeDashOffsetProperty =
			Shape.StrokeDashOffsetProperty.AddOwner(s_typeOfThis, new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnDashStyleChanged)));

		public DoubleCollection StrokeDashArray
		{
			get { return (DoubleCollection)GetValue(StrokeDashArrayProperty); }
			set { SetValue(StrokeDashArrayProperty, value); }
		}

		// Using a DependencyProperty as the backing store for StrokeDashArray.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty StrokeDashArrayProperty =
			Shape.StrokeDashArrayProperty.AddOwner(s_typeOfThis, new FrameworkPropertyMetadata(new DoubleCollection(), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnDashStyleChanged)));

		public object Header
		{
			get { return (object)GetValue(HeaderProperty); }
			set { SetValue(HeaderProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Header.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty HeaderProperty =
			HeaderedContentControl.HeaderProperty.AddOwner(s_typeOfThis, new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnHeaderChanged)));


		public static readonly DependencyProperty HasHeaderProperty =
			HeaderedContentControl.HasHeaderProperty;

		public bool HasHeader
		{
			get
			{
				return (bool)GetValue(HasHeaderProperty);
			}
		}

		public DataTemplate HeaderTemplate
		{
			get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
			set { SetValue(HeaderTemplateProperty, value); }
		}

		// Using a DependencyProperty as the backing store for HeaderTemplate.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty HeaderTemplateProperty =
			HeaderedContentControl.HeaderTemplateProperty.AddOwner(s_typeOfThis, new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnHeaderTemplateChanged)));

		public DataTemplateSelector HeaderTemplateSelector
		{
			get { return (DataTemplateSelector)GetValue(HeaderTemplateSelectorProperty); }
			set { SetValue(HeaderTemplateSelectorProperty, value); }
		}

		// Using a DependencyProperty as the backing store for HeaderTemplateSelector.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty HeaderTemplateSelectorProperty =
			HeaderedContentControl.HeaderTemplateSelectorProperty.AddOwner(s_typeOfThis, new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnHeaderTemplateSelectorChanged)));

		public string HeaderStringFormat
		{
			get { return (string)GetValue(HeaderStringFormatProperty); }
			set { SetValue(HeaderStringFormatProperty, value); }
		}

		// Using a DependencyProperty as the backing store for HeaderStringFormat.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty HeaderStringFormatProperty =
			HeaderedContentControl.HeaderStringFormatProperty.AddOwner(s_typeOfThis, new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnHeaderStringFormatChanged)));


		private static void OnHeaderTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is FlowItem item)
			{
				item.OnHeaderTemplateChanged((DataTemplate)e.OldValue, (DataTemplate)e.NewValue);
			}
		}

		protected virtual void OnHeaderTemplateChanged(DataTemplate oldHeaderTemplate, DataTemplate newHeaderTemplate)
		{
			// Helper.CheckTemplateAndTemplateSelector("Header", HeaderedContentControl.HeaderTemplateProperty, HeaderedContentControl.HeaderTemplateSelectorProperty, this);
		}

		private static void OnHeaderTemplateSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is FlowItem item)
			{
				item.OnHeaderTemplateSelectorChanged((DataTemplateSelector)e.OldValue, (DataTemplateSelector)e.NewValue);
			}
		}

		protected virtual void OnHeaderTemplateSelectorChanged(DataTemplateSelector oldHeaderTemplateSelector, DataTemplateSelector newHeaderTemplateSelector)
		{
			// Helper.CheckTemplateAndTemplateSelector("Header", HeaderedContentControl.HeaderTemplateProperty, HeaderedContentControl.HeaderTemplateSelectorProperty, this);
		}

		private static void OnHeaderStringFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is FlowItem item)
			{
				item.OnHeaderStringFormatChanged((string)e.OldValue, (string)e.NewValue);
			}
		}

		protected virtual void OnHeaderStringFormatChanged(string oldHeaderStringFormat, string newHeaderStringFormat)
		{
		}

		private static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is FlowItem item)
			{
				item.SetValueUnsafe(HasHeaderProperty, e.NewValue != null);
				item.OnHeaderChanged(e.OldValue, e.NewValue);
			}
		}

		protected virtual void OnHeaderChanged(object oldHeader, object newHeader)
		{
			RemoveLogicalChild(oldHeader);
			AddLogicalChild(newHeader);
		}

		private static void OnPensChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is FlowItem item)
			{
				item.m_connectorLogic.InvalidateDrawingPen();
				item.m_connectorLogic.InvalidateHitTestPen();
			}
		}

		private static void OnHitTestStrokeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is FlowItem item)
			{
				item.m_connectorLogic.InvalidateHitTestPen();
			}
		}

		private static void OnDashStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is FlowItem item)
			{
				item.m_connectorLogic.InvalidateDrawingPen();
			}
		}

		private static void OnConnectorPointsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is FlowItem item)
			{
				item.InvalidateConnectorPoints();
			}
		}

		private static object CoarceMinimumHitTestStroke(DependencyObject d, object baseValue)
		{
			double val = (double)baseValue;
			if (val < 4)
			{
				return 4;
			}
			return baseValue;
		}

		public void InvalidateConnectorPoints()
		{
			bool isConnector = FlowItemType == FlowItemType.Connector;
			if (isConnector)
			{
				m_connectorLogic.SetConnectorPoints(ConnectorStartPoint, ConnectorEndPoint);
				var flags = m_connectorLogic.LogicBoundChangeFlags;
				if (MatchAnyFlags(flags, RectChangeFlags.XChange, RectChangeFlags.YChange))
				{
					MoveTo(m_connectorLogic.LogicBound.X, m_connectorLogic.LogicBound.Y);
				}
				if (MatchAnyFlags(flags, RectChangeFlags.WidthChange, RectChangeFlags.HeightChange))
				{
					InvalidateMeasure();
				}
				m_connectorLogic.ClearChangeFlags();
			}
		}

		private bool MatchAnyFlags(RectChangeFlags src, params RectChangeFlags[] flags)
		{
			if (flags is null || flags.Length == 0)
			{
				return false;
			}
			foreach (var item in flags)
			{
				if ((src & item) == item)
				{
					return true;
				}
			}
			return false;
		}

		public virtual void MoveTo(double xValue, double yValue, bool allowValueNegative = false)
		{
			if (!allowValueNegative)
			{
				if (xValue < 0)
				{
					xValue = 0;
				}
				if (yValue < 0)
				{
					yValue = 0;
				}
			}
			//System.Diagnostics.Debug.WriteLine($"{xValue},{yValue}");
			//SetCurrentValue(Canvas.LeftProperty, xValue);
			//SetCurrentValue(Canvas.TopProperty, yValue);
			SetCurrentValue(InfiniteCanvas.OffsetXProperty, xValue);
			SetCurrentValue(InfiniteCanvas.OffsetYProperty, yValue);
		}

		public virtual void MoveDelta(double deltaX, double deltaY)
		{
			//var x = (double)GetValue(Canvas.LeftProperty);
			//var y = (double)GetValue(Canvas.TopProperty);
			var x = (double)GetValue(InfiniteCanvas.OffsetXProperty);
			var y = (double)GetValue(InfiniteCanvas.OffsetYProperty);
			MoveTo(x + deltaX, y + deltaY);
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			EndListenDragEvent();
			if (this.GetTemplateChild("PART_DraggableThumb", out _DraggableThumb))
			{
				if (_isDraggable)
				{
					BeginListenDragEvent();
				}
			}

		}

		//private void OnIsDraggableChanged(bool isDraggable)
		//{
		//	_isDraggable = isDraggable && _DraggableThumb != null;
		//	if (isDraggable != _isDraggable)
		//	{

		//	}
		//}

		//private bool CoerceIsDraggable(bool isDraggable)
		//{
		//	if (isDraggable)
		//	{
		//		return _DraggableThumb != null;
		//	}
		//	return isDraggable;
		//}

		private void BeginListenDragEvent()
		{
			if (_DraggableThumb is null)
			{
				return;
			}
			if (_isDragEventListened)
			{
				return;
			}
			_DraggableThumb.DragDelta += OnDraggableThumbDragDelta;
			_isDragEventListened = true;

		}

		private void EndListenDragEvent()
		{
			if (_DraggableThumb is null)
			{
				return;
			}
			if (_isDragEventListened)
			{
				_DraggableThumb.DragDelta -= OnDraggableThumbDragDelta;
				_isDragEventListened = false;
			}
		}

		private void OnDraggableThumbDragDelta(object sender, DragDeltaEventArgs e)
		{
			if (_isDraggable)
			{
				HandleDragDeltaEvent(e);
			}
		}

		protected virtual void HandleDragDeltaEvent(DragDeltaEventArgs e)
		{
			MoveDelta(e.HorizontalChange, e.VerticalChange);
			e.Handled = true;
		}

		protected override Size MeasureOverride(Size constraint)
		{
			if (FlowItemType == FlowItemType.Connector)
			{
				return m_connectorLogic.LogicBound.Size;
			}
			//needUpdateTransform = true;
			return base.MeasureOverride(constraint);
		}

		protected override Size ArrangeOverride(Size arrangeBounds)
		{
			if (FlowItemType == FlowItemType.Connector)
			{
				return m_connectorLogic.LogicBound.Size;
			}
			needUpdateTransform = true;
			return base.ArrangeOverride(arrangeBounds);
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			if (FlowItemType == FlowItemType.Connector)
			{
				if (m_connectorLogic.IsDrawingPenInvalid)
				{
					m_connectorLogic.SetDrawingPenDataFromShapeProperties(this);
				}
				m_connectorLogic.RenderInLogicBound(drawingContext);
			}
			//else
			//{
			//	base.OnRender(drawingContext);
			//	needUpdateTransform = true;
			//}
		}

		protected override GeometryHitTestResult HitTestCore(GeometryHitTestParameters hitTestParameters)
		{
			if (FlowItemType == FlowItemType.Connector)
			{
				if (hitTestParameters is null)
				{
					throw new ArgumentNullException(nameof(hitTestParameters));
				}
				if (m_connectorLogic.IsHitTestPenInvalid)
				{
					m_connectorLogic.SetHitTestPenDataFromShapeProperties(this, ExtendThickness);
				}
				var detail = m_connectorLogic.ContainsHitGeometry(hitTestParameters.HitGeometry);
				if (detail != IntersectionDetail.Empty)
				{
					return new GeometryHitTestResult(this, detail);
				}
				return null;
			}
			return base.HitTestCore(hitTestParameters);
		}

		protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
		{
			if (FlowItemType == FlowItemType.Connector)
			{
				if (hitTestParameters is null)
				{
					throw new ArgumentNullException(nameof(hitTestParameters));
				}
				if (m_connectorLogic.IsHitTestPenInvalid)
				{
					m_connectorLogic.SetHitTestPenDataFromShapeProperties(this, ExtendThickness);
				}
				if (m_connectorLogic.ContainsHitPoint(hitTestParameters.HitPoint))
				{
					return new PointHitTestResult(this, hitTestParameters.HitPoint);
				}
				return null;
			}
			return base.HitTestCore(hitTestParameters);
		}

		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			//System.Diagnostics.Debug.WriteLine($"OnMouseLeftButtonDown : {e.Handled}");
			//base.OnMouseLeftButtonDown(e);
		}

		protected static void HandleMouseButtonDownInternal(ListBoxItem item, MouseButton mouseButton)
		{
			if (HandleMouseButtonDown_ListBoxItem is null)
			{
				HandleMouseButtonDown_ListBoxItem = 
					ReflectionUtil.BindMethodToDelegate<Action<ListBoxItem, MouseButton>>(
						typeof(ListBoxItem), 
						"HandleMouseButtonDown", 
						ReflectionUtil.NonPublicInstance,
						typeof(MouseButton)
						);
			}
			HandleMouseButtonDown_ListBoxItem(item, mouseButton);
		}

		protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			//System.Diagnostics.Debug.WriteLine($"OnPreviewMouseLeftButtonDown : {e.Handled}");

			HandleMouseButtonDownInternal(this, MouseButton.Left);
		}

		protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
		{
			//System.Diagnostics.Debug.WriteLine($"OnPreviewMouseRightButtonDown : {e.Handled}");
			HandleMouseButtonDownInternal(this, MouseButton.Right);
		}

	}
}
