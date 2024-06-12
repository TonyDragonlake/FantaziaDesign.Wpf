using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FantaziaDesign.Core;

namespace FantaziaDesign.Wpf.Controls
{
	public interface IPixelsPresenterContext
	{
		BitmapSource PixelsSource { get; set; }
		int PixelMapRadius { get; set; }
		int PixelMapCenterX { get; set; }
		int PixelMapCenterY { get; set; }
		int PixelBoxRadius { get; set; }
		int PixelBorderThickness { get; set; }
		int PixelCount { get; }
		int PresenterSize { get ; }
		Color PixelMapCenterColor { get;}
		bool AffectGeometry { get; }
		bool AffectResource {get;}
		void PrepareGeometry();
		void PrepareResource();
		Color GetPixelMapColor(int index);
		void Render(DrawingContext drawingContext);
	}

	public interface IPixelsPresenter
	{
		IPixelsPresenterContext Context { get; }
	}

	public class PixelsPresenter : FrameworkElement, IPixelsPresenter
	{
		internal sealed class PixelColorResource
		{
			private int m_colorValue;
			private SolidColorBrush m_brush;
			private Pen m_pen;

			public PixelColorResource()
			{
			}

			public PixelColorResource(int colorValue)
			{
				SetColorValue(colorValue);
			}

			public SolidColorBrush Brush => m_brush is null ? Brushes.Transparent : m_brush;

			public bool IsNullBrush => m_brush is null;

			public int ColorValue => m_colorValue;

			public void SetColorValue(int value, bool solidOnly = false)
			{
				if (m_colorValue != value)
				{
					m_colorValue = value;
					ColorsUtility.ColorBytes(m_colorValue, out byte a, out byte r, out byte g, out byte b);
					Color color;
					if (solidOnly)
					{
						color = Color.FromRgb(r, g, b);
					}
					else
					{
						color = Color.FromArgb(a, r, g, b);
					}
					if (m_brush is null)
					{
						m_brush = new SolidColorBrush(color);
					}
					else
					{
						m_brush.Color = color;
					}
				}
			}

			public void SetColorValue(byte a, byte r, byte g, byte b, bool solidOnly = false)
			{
				if (solidOnly)
				{
					a = 255;
				}
				var val = ColorsUtility.ColorInt(a, r, g, b);
				if (m_colorValue != val)
				{
					m_colorValue = val;
					Color color;
					if (solidOnly)
					{
						color = Color.FromRgb(r, g, b);
					}
					else
					{
						color = Color.FromArgb(a, r, g, b);
					}
					if (m_brush is null)
					{
						m_brush = new SolidColorBrush(color);
					}
					else
					{
						m_brush.Color = color;
					}
				}
			}

			public Pen MakePen(int penStroke = 0)
			{
				if (m_pen is null)
				{
					m_pen = new Pen();
				}
				if (!(m_brush is null))
				{
					m_pen.Brush = m_brush;
				}
				if (penStroke > 0)
				{
					m_pen.Thickness = penStroke;
				}
				return m_pen;
			}
		}

		internal sealed class PixelsPresenterContext : IPixelsPresenterContext
		{
			private BitmapSource m_source;
			private int m_mapRadius;
			private int m_mapCenterX;
			private int m_mapCenterY;
			private int m_boxRadius;
			private int m_thickness;

			private List<Rect> m_cacheBoxes;
			private List<PixelColorResource> m_cacheResources;
			private int m_pixelCount;
			private int m_presenterSize;

			private Rect m_highlightBox;
			private PixelColorResource m_highlightResource;
			private int m_flag; // AffectGeometry = 0b01, AffectResource = 0b10

			public PixelsPresenterContext()
			{
				m_cacheBoxes = new List<Rect>();
				m_highlightBox = new Rect();
				m_cacheResources = new List<PixelColorResource>();
				m_highlightResource = new PixelColorResource();
			}

			public BitmapSource PixelsSource
			{
				get => m_source;
				set
				{
					if (m_source != value)
					{
						m_source = value;
						InvalidateResource();
					}
				}
			}

			public int PixelMapRadius 
			{ 
				get => m_mapRadius;
				set 
				{ 
					if (m_mapRadius != value)
					{
						m_mapRadius = value;
						var pixelBoxSize = m_boxRadius * 2 + 1;
						var pixelMapSize = m_mapRadius * 2 + 1;
						// pixelCount Changed -> invalidateCacheResource
						m_pixelCount = pixelMapSize * pixelMapSize;
						// presenterSize Changed -> affect measurement
						m_presenterSize = m_thickness + (m_thickness + pixelBoxSize) * pixelMapSize;
						InvalidateGeometry();
						InvalidateResource();
					}
				}
			}

			public int PixelMapCenterX
			{
				get => m_mapCenterX;
				set
				{
					if (m_mapCenterX != value)
					{
						m_mapCenterX = value;
						InvalidateResource();
					}
				}
			}

			public int PixelMapCenterY
			{
				get => m_mapCenterY;
				set
				{
					if (m_mapCenterY != value)
					{
						m_mapCenterY = value;
						InvalidateResource();
					}
				}
			}

			public int PixelBoxRadius 
			{ 
				get => m_boxRadius;
				set 
				{
					if (m_boxRadius != value)
					{
						m_boxRadius = value;
						var pixelBoxSize = m_boxRadius * 2 + 1;
						var pixelMapSize = m_mapRadius * 2 + 1;
						// presenterSize Changed -> affect measurement
						m_presenterSize = m_thickness + (m_thickness + pixelBoxSize) * pixelMapSize;
						InvalidateGeometry();
					}
				}
			}

			public int PixelBorderThickness 
			{
				get => m_thickness;
				set 
				{ 
					if (m_thickness != value)
					{
						m_thickness = value;
						var pixelBoxSize = m_boxRadius * 2 + 1;
						var pixelMapSize = m_mapRadius * 2 + 1;
						// presenterSize Changed -> affect measurement
						m_presenterSize = m_thickness + (m_thickness + pixelBoxSize) * pixelMapSize;
						InvalidateGeometry();
					}
				}
			}

			public int PixelCount { get => m_pixelCount; }

			public int PresenterSize { get => m_presenterSize; }

			public bool AffectGeometry => BitUtil.GetBit32Status(ref m_flag, 1);

			public bool AffectResource => BitUtil.GetBit32Status(ref m_flag, 2);

			public Color PixelMapCenterColor => GetPixelMapColor(m_pixelCount / 2);

			private void InvalidateGeometry()
			{
				m_flag |= 1;
			}

			private void InvalidateResource()
			{
				m_flag |= 2;
			}

			public void PrepareGeometry()
			{
				if (!AffectGeometry)
				{
					return;
				}
				var pixelBoxSize = m_boxRadius * 2 + 1;
				var pixelMapSize = m_mapRadius * 2 + 1;
				int x;
				int y = 0;
				int gBox = 0;
				int countBoxes = m_cacheBoxes.Count;
				for (int j = 0; j < pixelMapSize; j++)
				{
					x = 0;
					y += m_thickness;
					for (int i = 0; i < pixelMapSize; i++)
					{
						x += m_thickness;
						var rect = new Rect(x, y, pixelBoxSize, pixelBoxSize);
						if (gBox < countBoxes)
						{
							m_cacheBoxes[gBox] = rect;
						}
						else
						{
							m_cacheBoxes.Add(rect);
						}
						x += pixelBoxSize;
						gBox++;
					}
					y += pixelBoxSize;
				}
				var centerBox = m_cacheBoxes[m_pixelCount / 2];
				var cbx = centerBox.X;
				var cby = centerBox.Y;
				var cbw = centerBox.Width;
				var cbh = centerBox.Height;
				cbx -= m_thickness;
				cby -= m_thickness;
				cbw += 2 * m_thickness;
				cbh += 2 * m_thickness;
				m_highlightBox.X = cbx;
				m_highlightBox.Y = cby;
				m_highlightBox.Width = cbw;
				m_highlightBox.Height = cbh;
			}

			public void PrepareResource()
			{
				if (!AffectResource)
				{
					return;
				}
				if (m_source is null)
				{
					m_cacheResources.Clear();
					return;
				}
				var pixelMapSize = m_mapRadius * 2 + 1;
				var srcWidth = m_source.PixelWidth;
				var srcHeight = m_source.PixelHeight;
				var mapl = m_mapCenterX - m_mapRadius;
				var mapt = m_mapCenterY - m_mapRadius;
				var mapr = pixelMapSize + mapl;
				var mapb = pixelMapSize + mapt;
				var clipl = mapl < 0 ? 0 : mapl;
				var clipt = mapt < 0 ? 0 : mapt;
				var clipr = mapr > srcWidth ? srcWidth : mapr;
				var clipb = mapb > srcHeight ? srcHeight : mapb;
				var clipw = clipr - clipl;
				var cliph = clipb - clipt;
				if (clipw <= 0 || cliph <= 0)
				{
					m_cacheResources.Clear();
					return;
				}
				var rect = new Int32Rect(clipl, clipt, clipw, cliph);
				clipl -= mapl;
				clipt -= mapt;
				clipr -= mapl;
				clipb -= mapt;
				var stride = (rect.Width * m_source.Format.BitsPerPixel + 7) / 8;
				var data = new int[rect.Height * stride / 4];
				m_source.CopyPixels(rect, data, stride, 0);
				int gRes = 0;
				int gData = 0;
				int colorInt;
				int countRes = m_cacheResources.Count;
				for (int j = 0; j < pixelMapSize; j++)
				{
					for (int i = 0; i < pixelMapSize; i++)
					{
						colorInt = 0;
						if (clipl <= i && i < clipr && clipt <= j && j < clipb)
						{
							// colorful
							colorInt = data[gData];
							gData++;
						}
						if (gRes < countRes)
						{
							m_cacheResources[gRes].SetColorValue(colorInt);
						}
						else
						{
							var res = new PixelColorResource(colorInt);
							m_cacheResources.Add(res);
						}
						gRes++;
					}
				}
				var centerColor = m_cacheResources[m_pixelCount / 2].Brush.Color;
				var grayScale = ColorsUtility.ConvertRgbB3ToGrayscaleByte(centerColor.R, centerColor.G, centerColor.B);
				if (grayScale < 128)
				{
					m_highlightResource.SetColorValue(255, 255, 255, 255);
				}
				else
				{
					m_highlightResource.SetColorValue(255, 0, 0, 0);
				}
			}

			public void Render(DrawingContext drawingContext)
			{
				var minCount = Math.Min(m_cacheResources.Count, m_cacheBoxes.Count);
				if (minCount > m_pixelCount)
				{
					minCount = m_pixelCount;
				}
				int centerPixel = m_pixelCount / 2;
				Brush centerBrush = null;
				for (int i = 0; i < minCount; i++)
				{
					var brush = m_cacheResources[i].Brush;
					if (i == centerPixel)
					{
						centerBrush = brush;
					}
					else if (brush.Color.A != 0)
					{
						var rect = m_cacheBoxes[i];
						drawingContext.DrawRectangle(brush, null, rect);
					}
				}
				drawingContext.DrawRectangle(centerBrush, m_highlightResource.MakePen(2), m_highlightBox);
				m_flag = 0;
			}

			public Color GetPixelMapColor(int index)
			{
				if (0 <= index && index < m_cacheResources.Count)
				{
					var targetRes = m_cacheResources[index];
					if (!targetRes.IsNullBrush)
					{
						return targetRes.Brush.Color;
					}
				}
				return Colors.Transparent;
			}
		}

		private PixelsPresenterContext m_context;

		public IPixelsPresenterContext Context => m_context;

		public BitmapSource PixelsSource
		{
			get { return (BitmapSource)GetValue(PixelsSourceProperty); }
			set { SetValue(PixelsSourceProperty, value); }
		}

		// Using a DependencyProperty as the backing store for PixelsSource.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty PixelsSourceProperty =
			DependencyProperty.Register("PixelsSource", typeof(BitmapSource), typeof(PixelsPresenter), 
				new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnPixelsPresenterContextChanged)));

		public int PixelMapRadius
		{
			get { return (int)GetValue(PixelMapRadiusProperty); }
			set { SetValue(PixelMapRadiusProperty, value); }
		}

		// Using a DependencyProperty as the backing store for PixelMapRadius.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty PixelMapRadiusProperty =
			DependencyProperty.Register("PixelMapRadius", typeof(int), typeof(PixelsPresenter), 
				new FrameworkPropertyMetadata(4, FrameworkPropertyMetadataOptions.AffectsMeasure|FrameworkPropertyMetadataOptions.AffectsRender,
					new PropertyChangedCallback(OnPixelsPresenterContextChanged), new CoerceValueCallback(OnCoercePixelMapRadius)));

		private static object OnCoercePixelMapRadius(DependencyObject d, object baseValue)
		{
			int val = Convert.ToInt32(baseValue);
			if (val < 1)
			{
				val = 1;
			}
			if (val > 25)
			{
				val = 25;
			}
			return val;
		}

		public int PixelMapCenterX
		{
			get { return (int)GetValue(PixelMapCenterXProperty); }
			set { SetValue(PixelMapCenterXProperty, value); }
		}

		// Using a DependencyProperty as the backing store for PixelMapCenterX.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty PixelMapCenterXProperty =
			DependencyProperty.Register("PixelMapCenterX", typeof(int), typeof(PixelsPresenter), 
				new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender, 
					new PropertyChangedCallback(OnPixelsPresenterContextChanged)));

		public int PixelMapCenterY
		{
			get { return (int)GetValue(PixelMapCenterYProperty); }
			set { SetValue(PixelMapCenterYProperty, value); }
		}

		// Using a DependencyProperty as the backing store for PixelMapCenterY.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty PixelMapCenterYProperty =
			DependencyProperty.Register("PixelMapCenterY", typeof(int), typeof(PixelsPresenter), 
				new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender, 
					new PropertyChangedCallback(OnPixelsPresenterContextChanged)));

		public int PixelBoxRadius
		{
			get { return (int)GetValue(PixelBoxRadiusProperty); }
			set { SetValue(PixelBoxRadiusProperty, value); }
		}

		// Using a DependencyProperty as the backing store for PixelBoxRadius.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty PixelBoxRadiusProperty =
			DependencyProperty.Register("PixelBoxRadius", typeof(int), typeof(PixelsPresenter), 
				new FrameworkPropertyMetadata(4, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, 
					new PropertyChangedCallback(OnPixelsPresenterContextChanged), new CoerceValueCallback(OnCoercePixelBoxRadius)));

		private static object OnCoercePixelBoxRadius(DependencyObject d, object baseValue)
		{
			int val = Convert.ToInt32(baseValue);
			if (val < 1)
			{
				val = 1;
			}
			if (val > 10)
			{
				val = 10;
			}
			return val;
		}

		public int PixelBorderThickness
		{
			get { return (int)GetValue(PixelBorderThicknessProperty); }
			set { SetValue(PixelBorderThicknessProperty, value); }
		}

		// Using a DependencyProperty as the backing store for PixelBorderThickness.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty PixelBorderThicknessProperty =
			DependencyProperty.Register("PixelBorderThickness", typeof(int), typeof(PixelsPresenter), 
				new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, 
					new PropertyChangedCallback(OnPixelsPresenterContextChanged), new CoerceValueCallback(OnCoercePixelBorderThickness)));

		public Brush PixelBorderBrush
		{
			get { return (Brush)GetValue(PixelBorderBrushProperty); }
			set { SetValue(PixelBorderBrushProperty, value); }
		}

		// Using a DependencyProperty as the backing store for PixelBorderBrush.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty PixelBorderBrushProperty =
			DependencyProperty.Register("PixelBorderBrush", typeof(Brush), typeof(PixelsPresenter), 
				new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

		public PixelsPresenter()
		{
			m_context = new PixelsPresenterContext();
			m_context.PixelMapRadius = PixelMapRadius;
			m_context.PixelBoxRadius = PixelBoxRadius;
			m_context.PixelBorderThickness = PixelBorderThickness;
		}

		private static object OnCoercePixelBorderThickness(DependencyObject d, object baseValue)
		{
			int val = Convert.ToInt32(baseValue);
			if (val < 0)
			{
				val = 0;
			}
			return val;
		}

		private static void OnPixelsPresenterContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var presenter = d as IPixelsPresenter; 
			if (presenter != null)
			{
				var context = presenter.Context;
				if (context != null)
				{
					/*
					BitmapSource PixelsSource { get; set; }
					int PixelMapRadius { get; set; }
					int PixelMapCenterX { get; set; }
					int PixelMapCenterY { get; set; }
					int PixelBoxRadius { get; set; }
					int PixelBorderThickness { get; set; }
					*/

					var index = e.Property.GlobalIndex;
					if (index == PixelsSourceProperty.GlobalIndex)
					{
						context.PixelsSource = e.NewValue as BitmapSource;
					}
					else
					{
						var nullableValue = e.NewValue as int?;
						if (nullableValue.HasValue)
						{
							var value = nullableValue.Value;
							if (index == PixelMapRadiusProperty.GlobalIndex)
							{
								context.PixelMapRadius = value;
							}
							else if (index == PixelMapCenterXProperty.GlobalIndex)
							{
								context.PixelMapCenterX = value;
							}
							else if (index == PixelMapCenterYProperty.GlobalIndex)
							{
								context.PixelMapCenterY = value;
							}
							else if (index == PixelBoxRadiusProperty.GlobalIndex)
							{
								context.PixelBoxRadius = value;
							}
							else if (index == PixelBorderThicknessProperty.GlobalIndex)
							{
								context.PixelBorderThickness = value;
							}
						}
					}
				}
			}
		}

		protected override Size MeasureOverride(Size availableSize)
		{
			if (m_context != null)
			{
				m_context.PrepareGeometry();
				var size = m_context.PresenterSize;
				return new Size(size, size);
			}
			return availableSize;
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			if (m_context != null)
			{
				var size = m_context.PresenterSize;
				return new Size(size, size);
			}
			return finalSize;
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			var bgBrush = PixelBorderBrush;
			if (bgBrush is null)
			{
				bgBrush = Brushes.Transparent;
			}
			if (m_context != null)
			{
				m_context.PrepareResource();
				var size = m_context.PresenterSize;
				drawingContext.DrawRectangle(bgBrush, null, new Rect(new Size(size, size)));
				m_context.Render(drawingContext);
			}
			else
			{
				drawingContext.DrawRectangle(bgBrush, null, new Rect(RenderSize));
			}
		}
	}
}
