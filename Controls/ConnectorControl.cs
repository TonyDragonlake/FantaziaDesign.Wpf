using FantaziaDesign.Core;
using FantaziaDesign.Wpf.Core;
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

namespace FantaziaDesign.Wpf.Controls
{
	[Flags]
	public enum RectChangeFlags : byte
	{
		NoChange,
		XChange = 1,
		YChange = 2,
		OriginChange = XChange | YChange,
		WidthChange = 4,
		HeightChange = 8,
		SizeChange = WidthChange | HeightChange,
		RectChange = OriginChange | SizeChange
	}

	public sealed class PenStyle
	{
		private bool isValid;
		private Vec4b styleFlags = new Vec4b();
		double miterLimit = 10.0;
		double dashOffset;
		IList<double> dashArray;

		public PenLineCap StartLineCap
		{
			get => (PenLineCap)styleFlags[0];
			set
			{
				styleFlags[0] = (byte)value;
				isValid = true;
			}
		}
		public PenLineCap EndLineCap
		{
			get => (PenLineCap)styleFlags[1];
			set
			{
				styleFlags[1] = (byte)value;
				isValid = true;
			}
		}
		public PenLineJoin LineJoin
		{
			get => (PenLineJoin)styleFlags[2];
			set
			{
				styleFlags[2] = (byte)value;
				isValid = true;
			}
		}
		public PenLineCap DashCap
		{
			get => (PenLineCap)styleFlags[3];
			set
			{
				styleFlags[3] = (byte)value;
				isValid = true;
			}
		}
		public int StyleFlags => styleFlags;

		public double MiterLimit
		{
			get => miterLimit;
			set
			{
				if (value < 1)
				{
					value = 1;
				}
				if (miterLimit != value)
				{
					miterLimit = value;
					isValid = true;
				}
			}
		}

		public double DashOffset
		{
			get => dashOffset;
			set
			{
				if (dashOffset != value)
				{
					dashOffset = value;
					isValid = true;
				}
			}
		}

		public IList<double> DashArray
		{
			get => dashArray;
			set
			{
				dashArray = value;
				isValid = true;
			}
		}

		public bool IsValid => isValid;

		public bool TryBuildPen(Brush brush, double thickness, bool ignoreDashStyle, out Pen pen)
		{
			bool isPenCollapsed = brush is null || double.IsNaN(thickness) || thickness == 0;
			if (isPenCollapsed)
			{
				pen = null;
				return false;
			}
			thickness = Math.Abs(thickness);
			pen = new Pen(brush, thickness);
			pen.StartLineCap = StartLineCap;
			pen.EndLineCap = EndLineCap;
			pen.LineJoin = LineJoin;
			pen.MiterLimit = MiterLimit;
			if (ignoreDashStyle)
			{
				return true;
			}
			pen.DashCap = DashCap;
			var dashOffset = DashOffset;
			if (dashArray != null || dashOffset != 0.0)
			{
				pen.DashStyle = new DashStyle(dashArray, dashOffset);
			}
			return true;
		}

		public void SetStyleFromShapeProperties(DependencyObject dependencyObject)
		{
			if (dependencyObject is null)
			{
				throw new ArgumentNullException(nameof(dependencyObject));
			}

			StartLineCap = (PenLineCap)dependencyObject.GetValue(Shape.StrokeStartLineCapProperty);
			EndLineCap = (PenLineCap)dependencyObject.GetValue(Shape.StrokeEndLineCapProperty);
			LineJoin = (PenLineJoin)dependencyObject.GetValue(Shape.StrokeLineJoinProperty);
			DashCap = (PenLineCap)dependencyObject.GetValue(Shape.StrokeDashCapProperty);
			MiterLimit = (double)dependencyObject.GetValue(Shape.StrokeMiterLimitProperty);
			DashOffset = (double)dependencyObject.GetValue(Shape.StrokeDashOffsetProperty);
			DashArray = ((DoubleCollection)dependencyObject.GetValue(Shape.StrokeDashArrayProperty))?.ToArray();
		}

		public void SetStyleFromPenProperties(DependencyObject dependencyObject)
		{
			if (dependencyObject is null)
			{
				throw new ArgumentNullException(nameof(dependencyObject));
			}

			StartLineCap = (PenLineCap)dependencyObject.GetValue(Pen.StartLineCapProperty);
			EndLineCap = (PenLineCap)dependencyObject.GetValue(Pen.EndLineCapProperty);
			LineJoin = (PenLineJoin)dependencyObject.GetValue(Pen.LineJoinProperty);
			DashCap = (PenLineCap)dependencyObject.GetValue(Pen.DashCapProperty);
			MiterLimit = (double)dependencyObject.GetValue(Pen.MiterLimitProperty);
			var ds = (DashStyle)dependencyObject.GetValue(Pen.DashStyleProperty);
			if (ds is null)
			{
				DashOffset = 0;
				DashArray = null;
			}
			else
			{
				DashOffset = ds.Offset;
				DashArray = ds.Dashes.ToArray();
			}
		}

		public void InvalidateStyle()
		{
			if (isValid)
			{
				styleFlags = 0;
				miterLimit = 10.0;
				dashOffset = 0;
				dashArray = null;
				isValid = false;
			}
		}
	}

	public class ConnectorDrawingLogic
	{
		private Point m_startPoint;
		private Point m_endPoint;
		private Rect m_logicBound;
		private Pen m_drawingPen;
		private Pen m_hitTestPen;
		private StreamGeometry geoCache = new StreamGeometry();
		private RectChangeFlags m_changeFlags;
		private PenStyle m_penStyle = new PenStyle();
		private Brush m_penBrush;
		private double m_drawingThickness;
		private double m_hitTestThickness;

		public Point StartPoint
		{
			get => m_startPoint;
			set => SetStartPoint(value);
		}

		public Point EndPoint
		{
			get => m_endPoint;
			set => SetEndPoint(value);
		}

		public RectChangeFlags LogicBoundChangeFlags => m_changeFlags;

		public bool IsLogicBoundValid => (!m_logicBound.IsEmpty) && (m_logicBound.Width * m_logicBound.Height) >= 0;

		public bool IsConnectorValid => m_startPoint != m_endPoint;

		public PenStyle PenStyle => m_penStyle;

		public Pen DrawingPen
		{
			get
			{
				if (m_drawingPen is null)
				{
					if (!m_penStyle.TryBuildPen(m_penBrush, m_drawingThickness, false, out m_drawingPen))
					{
						return null;
					}
				}
				return m_drawingPen;
			}
		}

		public Pen HitTestPen
		{
			get
			{
				if (m_hitTestPen is null)
				{
					if (!m_penStyle.TryBuildPen(m_penBrush, m_hitTestThickness, true, out m_hitTestPen))
					{
						return null;
					}
				}
				return m_hitTestPen;
			}
		}

		public bool IsDrawingPenInvalid => m_drawingPen is null;
		public bool IsHitTestPenInvalid => m_hitTestPen is null;

		public Brush PenBrush { get => m_penBrush; set => m_penBrush = value; }

		public double DrawingThickness
		{
			get => m_drawingThickness;
			set
			{
				if (m_drawingThickness != value)
				{
					var extend = m_hitTestThickness - m_drawingThickness;
					m_drawingThickness = value;
					if (extend < 4)
					{
						return;
					}
					m_hitTestThickness = m_drawingThickness + extend;
				}
			}
		}

		public double ExtendThickness
		{
			get => m_hitTestThickness - m_drawingThickness;
			set
			{
				if (value < 4)
				{
					value = 4;
				}
				m_hitTestThickness = m_drawingThickness + value;
			}
		}

		public double HitTestThickness => m_hitTestThickness;

		public Rect LogicBound => m_logicBound;

		public void SetStartPoint(Point point)
		{
			if (m_startPoint == point)
			{
				return;
			}
			m_startPoint.X = point.X;
			m_startPoint.Y = point.Y;
			UpdateLogicBound();
		}

		public void SetStartPoint(double x, double y)
		{
			if (m_startPoint.X == x && m_startPoint.Y == y)
			{
				return;
			}
			m_startPoint.X = x;
			m_startPoint.Y = y;
			UpdateLogicBound();
		}

		public void SetEndPoint(Point point)
		{
			if (m_endPoint == point)
			{
				return;
			}
			m_endPoint.X = point.X;
			m_endPoint.Y = point.Y;
			UpdateLogicBound();
		}

		public void SetEndPoint(double x, double y)
		{
			if (m_endPoint.X == x && m_endPoint.Y == y)
			{
				return;
			}
			m_endPoint.X = x;
			m_endPoint.Y = y;
			UpdateLogicBound();
		}

		public void SetConnectorPoints(Point start, Point end)
		{
			if (m_startPoint == start && m_endPoint == end)
			{
				return;
			}
			m_startPoint.X = start.X;
			m_startPoint.Y = start.Y;
			m_endPoint.X = end.X;
			m_endPoint.Y = end.Y;
			UpdateLogicBound();
		}

		private void UpdateLogicBound()
		{
			var spX = m_startPoint.X;
			var spY = m_startPoint.Y;
			var epX = m_endPoint.X;
			var epY = m_endPoint.Y;
			double minX, minY, maxX, maxY, dX, dY;
			if (spX < epX)
			{
				minX = spX;
				maxX = epX;
			}
			else
			{
				minX = epX;
				maxX = spX;
			}

			if (spY < epY)
			{
				minY = spY;
				maxY = epY;
			}
			else
			{
				minY = epY;
				maxY = spY;
			}

			dX = maxX - minX;
			dY = maxY - minY;
			if (m_logicBound.X != minX)
			{
				m_logicBound.X = minX;
				m_changeFlags |= RectChangeFlags.XChange;
			}
			if (m_logicBound.Y != minY)
			{
				m_logicBound.Y = minY;
				m_changeFlags |= RectChangeFlags.YChange;
			}
			if (m_logicBound.Width != dX)
			{
				m_logicBound.Width = dX;
				m_changeFlags |= RectChangeFlags.WidthChange;
			}
			if (m_logicBound.Height != dY)
			{
				m_logicBound.Height = dY;
				m_changeFlags |= RectChangeFlags.HeightChange;
			}
		}

		public void RenderInLogicBound(DrawingContext drawingContext)
		{
			var type = m_logicBound.GetRectType();
			if (type == RectType.InvisibleRect || type >= RectType.InfinityRect)
			{
				return;
			}
			var sp = StartPoint;
			var ep = EndPoint;
			var offsetX = -m_logicBound.X;
			var offsetY = -m_logicBound.Y;
			sp.Offset(offsetX, offsetY);
			ep.Offset(offsetX, offsetY);
			if (type == RectType.NormalRect)
			{
				//var sc = new Point(ep.X, sp.Y);
				//var ec = new Point(sp.X, ep.Y);
				var sc = new Point(sp.X + (ep.X - sp.X) * 0.45, sp.Y);
				var ec = new Point(ep.X + (sp.X - ep.X) * 0.45, ep.Y);
				using (var ctx = geoCache.Open())
				{
					ctx.BeginFigure(sp, true, false);
					ctx.BezierTo(sc, ec, ep, true, true);
				}
				drawingContext.DrawGeometry(null, DrawingPen, geoCache);
			}
			else
			{
				drawingContext.DrawLine(DrawingPen, sp, ep);
			}
		}

		public void Render(DrawingContext drawingContext)
		{
			if (IsConnectorValid)
			{
				var sp = StartPoint;
				var ep = EndPoint;
				if (sp.X == ep.X || sp.Y == ep.Y)
				{
					drawingContext.DrawLine(DrawingPen, sp, ep);
					return;
				}
				//var sc = new Point(ep.X, sp.Y);
				//var ec = new Point(sp.X, ep.Y);
				var sc = new Point(sp.X + (ep.X - sp.X) * 0.45, sp.Y);
				var ec = new Point(ep.X + (sp.X - ep.X) * 0.45, ep.Y);
				using (var ctx = geoCache.Open())
				{
					ctx.BeginFigure(sp, true, false);
					ctx.BezierTo(sc, ec, ep, true, true);
				}
				drawingContext.DrawGeometry(null, DrawingPen, geoCache);
			}
		}

		public void InvalidateDrawingPen()
		{
			m_drawingPen = null;
			m_penBrush = null;
			m_drawingThickness = 0;
			m_penStyle.InvalidateStyle();
		}

		public void InvalidateHitTestPen()
		{
			m_hitTestPen = null;
			m_penBrush = null;
			m_hitTestThickness = 0;
			m_penStyle.InvalidateStyle();
		}

		public void ClearChangeFlags()
		{
			m_changeFlags = RectChangeFlags.NoChange;
		}

		public IntersectionDetail ContainsHitGeometry(Geometry hitGeometry)
		{
			if (IsConnectorValid)
			{
				var pen = HitTestPen;
				if (pen is null)
				{
					return IntersectionDetail.Empty;
				}
				return geoCache.StrokeContainsWithDetail(pen, hitGeometry);
			}
			return IntersectionDetail.Empty;
		}

		public bool ContainsHitPoint(Point hitPoint)
		{
			if (IsConnectorValid)
			{
				var pen = HitTestPen;
				if (pen is null)
				{
					return false;
				}
				return geoCache.StrokeContains(pen, hitPoint);
			}
			return false;
		}

		public void SetDrawingPenDataFromShapeProperties(DependencyObject dependencyObject)
		{
			if (m_penBrush is null)
			{
				m_penBrush = (Brush)dependencyObject.GetValue(Shape.StrokeProperty);
			}
			if (m_drawingThickness == 0)
			{
				m_drawingThickness = (double)dependencyObject.GetValue(Shape.StrokeThicknessProperty);
			}
			if (!m_penStyle.IsValid)
			{
				m_penStyle.SetStyleFromShapeProperties(dependencyObject);
			}
		}

		public void SetDrawingPenDataFromPenProperties(DependencyObject dependencyObject)
		{
			if (m_penBrush is null)
			{
				m_penBrush = (Brush)dependencyObject.GetValue(Pen.BrushProperty);
			}
			if (m_drawingThickness == 0)
			{
				m_drawingThickness = (double)dependencyObject.GetValue(Pen.ThicknessProperty);
			}
			if (m_penStyle.IsValid)
			{
				m_penStyle.SetStyleFromShapeProperties(dependencyObject);
			}
		}

		public void SetExtendThicknessIfInvalid(double thickness)
		{
			if (m_hitTestThickness == 0 && thickness >= 4)
			{
				m_hitTestThickness = m_drawingThickness + thickness;
			}
		}

		public void SetHitTestPenDataFromShapeProperties(DependencyObject dependencyObject, double extendThickness)
		{
			if (m_penBrush is null)
			{
				m_penBrush = (Brush)dependencyObject.GetValue(Shape.StrokeProperty);
			}
			if (m_drawingThickness == 0)
			{
				m_drawingThickness = (double)dependencyObject.GetValue(Shape.StrokeThicknessProperty);
			}
			if (m_hitTestThickness == 0 && extendThickness >= 4)
			{
				m_hitTestThickness = m_drawingThickness + extendThickness;
			}
			if (m_penStyle.IsValid)
			{
				m_penStyle.SetStyleFromShapeProperties(dependencyObject);
			}
		}

		public void SetHitTestPenDataFromPenProperties(DependencyObject dependencyObject, double extendThickness)
		{
			if (m_penBrush is null)
			{
				m_penBrush = (Brush)dependencyObject.GetValue(Pen.BrushProperty);
			}
			if (m_hitTestThickness == 0)
			{
				m_hitTestThickness = (double)dependencyObject.GetValue(Pen.ThicknessProperty);
			}
			if (m_hitTestThickness == 0 && extendThickness >= 4)
			{
				m_hitTestThickness = m_drawingThickness + extendThickness;
			}
			if (m_penStyle.IsValid)
			{
				m_penStyle.SetStyleFromShapeProperties(dependencyObject);
			}
		}
	}

	public class ConnectorControl : Control
	{
		private static readonly Type s_typeOfThis = typeof(ConnectorControl);

		private bool isPointChangeSuspended = false;
		private ConnectorDrawingLogic m_connectorLogic = new ConnectorDrawingLogic();

		static ConnectorControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(s_typeOfThis, new FrameworkPropertyMetadata(s_typeOfThis));
		}

		public Point ConnectorStartPoint
		{
			get { return (Point)GetValue(ConnectorStartPointProperty); }
			set { SetValue(ConnectorStartPointProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ConnectorStartPoint.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ConnectorStartPointProperty =
			DependencyProperty.Register("ConnectorStartPoint", typeof(Point), s_typeOfThis, new PropertyMetadata(default(Point), OnConnectorPointsChanged));

		public Point ConnectorEndPoint
		{
			get { return (Point)GetValue(ConnectorEndPointProperty); }
			set { SetValue(ConnectorEndPointProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ConnectorEndPoint.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ConnectorEndPointProperty =
			DependencyProperty.Register("ConnectorEndPoint", typeof(Point), s_typeOfThis, new PropertyMetadata(default(Point), OnConnectorPointsChanged));

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
			DependencyProperty.Register("ExtendThickness", typeof(double), s_typeOfThis, new PropertyMetadata(4.0, OnHitTestStrokeChanged, CoarceMinimumHitTestStroke));

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

		public bool IsConnectorPointsChangeSuspended { get => isPointChangeSuspended;  }

		// Using a DependencyProperty as the backing store for StrokeDashArray.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty StrokeDashArrayProperty =
			Shape.StrokeDashArrayProperty.AddOwner(s_typeOfThis, new FrameworkPropertyMetadata(new DoubleCollection(), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnDashStyleChanged)));

		private static void OnPensChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is ConnectorControl connector)
			{
				connector.m_connectorLogic.InvalidateDrawingPen();
				connector.m_connectorLogic.InvalidateHitTestPen();
			}
		}

		private static void OnHitTestStrokeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is ConnectorControl connector)
			{
				connector.m_connectorLogic.InvalidateHitTestPen();
			}
		}

		private static void OnDashStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is ConnectorControl connector)
			{
				connector.m_connectorLogic.InvalidateDrawingPen();
			}
		}

		private static void OnConnectorPointsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is ConnectorControl connector)
			{
				connector.InvalidateConnectorPoints();
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
			if (isPointChangeSuspended)
			{
				return;
			}
			m_connectorLogic.SetConnectorPoints(ConnectorStartPoint, ConnectorEndPoint);
			var flags = m_connectorLogic.LogicBoundChangeFlags;
			if (flags != RectChangeFlags.NoChange)
			{
				if (IsArrangeValid || IsMeasureValid)
				{
					InvalidateVisual();
				}
			}
			m_connectorLogic.ClearChangeFlags();
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			if (m_connectorLogic.IsDrawingPenInvalid)
			{
				m_connectorLogic.SetDrawingPenDataFromShapeProperties(this);
			}
			m_connectorLogic.Render(drawingContext);
		}

		protected override GeometryHitTestResult HitTestCore(GeometryHitTestParameters hitTestParameters)
		{
			if (hitTestParameters is null)
			{
				return null;
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

		protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
		{
			if (hitTestParameters is null)
			{
				return null;
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

		public void SuspendConnectorPointsChange()
		{
			if (isPointChangeSuspended)
			{
				return;
			}
			isPointChangeSuspended = true;
		}

		public void ResumeConnectorPointsChange()
		{
			if (isPointChangeSuspended)
			{
				isPointChangeSuspended = false;
				InvalidateConnectorPoints();
			}
		}
	}

	public class ConnectorControlAdorner : FrameworkElementAdorner<ConnectorControl>
	{
		public ConnectorControlAdorner(UIElement adornedElement) : base(adornedElement)
		{
		}

		protected override void OnAttachAdornerChild(UIElement adornedElement)
		{
			m_child = new ConnectorControl();
		}
	}

	public class ConnectorControlAdornerController : AdornerController<ConnectorControlAdorner>
	{
		private ConnectorControlAdorner m_adorner;

		public override ConnectorControlAdorner CurrentAdorner { get => m_adorner; protected set => m_adorner = value; }

		protected override void ActiveAdorner()
		{
			if (m_adorner is null)
			{
				Predicate<ConnectorControlAdorner> matchCondition = MatchFirst;
				Func<FrameworkElement, ConnectorControlAdorner> creator = Creator;
				if (TryGetMatchedAdornerOrNewOverride(out m_adorner, matchCondition, creator))
				{
					m_adorner.Visibility = Visibility.Visible;
				}
				else
				{
					m_adorner = null;
				}
			}
			else
			{
				m_adorner.Visibility = Visibility.Visible;
			}
		}

		private static bool MatchFirst(ConnectorControlAdorner adorner)
		{
			return true;
		}

		private static ConnectorControlAdorner Creator(FrameworkElement element)
		{
			return new ConnectorControlAdorner(element);
		}

		protected override void DeactiveAdorner(bool removeAction = false)
		{
			if (removeAction)
			{
				if (m_adorner is null)
				{
					return;
				}
				m_adornerLayer.Remove(m_adorner);
				m_adorner = null;
			}
			else
			{
				if (m_adorner is null)
				{
					Predicate<ConnectorControlAdorner> matchCondition = MatchFirst;
					Func<FrameworkElement, ConnectorControlAdorner> creator = Creator;
					if (TryGetMatchedAdornerOrNewOverride(out m_adorner, matchCondition, creator))
					{
						m_adorner.Visibility = Visibility.Collapsed;
					}
					else
					{
						m_adorner = null;
					}
				}
				else
				{
					m_adorner.Visibility = Visibility.Collapsed;
				}
			}
		}
	}

}
