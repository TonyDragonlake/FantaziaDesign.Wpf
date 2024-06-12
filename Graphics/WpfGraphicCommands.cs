using FantaziaDesign.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace FantaziaDesign.Wpf.Graphics
{
	public class WpfGeomertySinkContext : VirtualGeomertySinkContext<Geometry>
	{
		private StreamGeometry _geometryInstance = new StreamGeometry();
		private StreamGeometryContext _context;

		public WpfGeomertySinkContext()
		{
			_geometryInstance = new StreamGeometry();
		}

		public override Geometry GeometryInstance => _geometryInstance;

		protected override void ArcTo(Float2Reader pointAndSize, float rotationAngle, bool isLargeArc, bool isClockwise, bool isStroked, bool isSmoothJoin)
		{
			if (_context is null)
			{
				return;
			}
			if (pointAndSize.Read(0,out float px, out float py) && pointAndSize.Read(1, out float sw, out float sh))
			{
				_context.ArcTo(new Point(px, py), new Size(sw, sh), rotationAngle, isLargeArc, isClockwise ? SweepDirection.Clockwise : SweepDirection.Counterclockwise, isStroked, isSmoothJoin);
			}
		}

		protected override void BeginFigure(Float2Reader startPoint, bool isFilled, bool isClosed)
		{
			if (_context is null)
			{
				return;
			}
			if (startPoint.Read(0, out float px, out float py))
			{
				_context.BeginFigure(new Point(px, py), isFilled, isClosed);
			}
		}

		protected override void BeginGeomerty()
		{
			_context = _geometryInstance.Open();
		}

		protected override void BezierTo(Float2Reader points, bool isStroked, bool isSmoothJoin)
		{
			if (_context is null)
			{
				return;
			}
			if (points.Read(0, out float p1x, out float p1y) && points.Read(1, out float p2x, out float p2y) && points.Read(2, out float p3x, out float p3y))
			{
				_context.BezierTo(new Point(p1x, p1y), new Point(p2x, p2y), new Point(p3x, p3y), isStroked, isSmoothJoin);
			}
		}

		protected override void EndFigure(bool isFilled, bool isClosed)
		{
		}

		protected override void EndGeomerty()
		{
			_context?.Close();
		}

		protected override void LineTo(Float2Reader point, bool isStroked, bool isSmoothJoin)
		{
			if (_context is null)
			{
				return;
			}
			if (point.Read(0, out float px, out float py))
			{
				_context.LineTo(new Point(px, py), isStroked, isSmoothJoin);
			}
		}

		protected override void PolyBezierTo(Float2Reader points, bool isStroked, bool isSmoothJoin)
		{
			if (_context is null)
			{
				return;
			}
			var length = points.ItemCount;
			Point[] pointCol = new Point[length];
			float px, py;
			for (int i = 0; i < length; i++)
			{
				if (points.Read(i, out px, out py))
				{
					pointCol[i] = new Point(px, py);
				}
				else
				{
					return;
				}
			}
			_context.PolyBezierTo(pointCol, isStroked, isSmoothJoin);
		}

		protected override void PolyLineTo(Float2Reader points, bool isStroked, bool isSmoothJoin)
		{
			if (_context is null)
			{
				return;
			}
			var length = points.ItemCount;
			Point[] pointCol = new Point[length];
			float px, py;
			for (int i = 0; i < length; i++)
			{
				if (points.Read(i, out px, out py))
				{
					pointCol[i] = new Point(px, py);
				}
				else
				{
					return;
				}
			}
			_context.PolyLineTo(pointCol, isStroked, isSmoothJoin);
		}

		protected override void PolyQuadraticBezierTo(Float2Reader points, bool isStroked, bool isSmoothJoin)
		{
			if (_context is null)
			{
				return;
			}
			var length = points.ItemCount;
			Point[] pointCol = new Point[length];
			float px, py;
			for (int i = 0; i < length; i++)
			{
				if (points.Read(i, out px, out py))
				{
					pointCol[i] = new Point(px, py);
				}
				else
				{
					return;
				}
			}
			_context.PolyQuadraticBezierTo(pointCol, isStroked, isSmoothJoin);
		}

		protected override void QuadraticBezierTo(Float2Reader points, bool isStroked, bool isSmoothJoin)
		{
			if (_context is null)
			{
				return;
			}
			if (points.Read(0, out float p1x, out float p1y) && points.Read(1, out float p2x, out float p2y))
			{
				_context.QuadraticBezierTo(new Point(p1x, p1y), new Point(p2x, p2y), isStroked, isSmoothJoin);
			}
		}
	}

}
