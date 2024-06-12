using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using FantaziaDesign.Core;
using FantaziaDesign.Wpf.Core;

namespace FantaziaDesign.Wpf.Controls
{
	public enum ColorSpectrumComponents
	{
		/// <summary>
		/// Hue is mapped to the X axis. Value is mapped to the Y axis.
		/// </summary>
		HueValue,
		/// <summary>
		/// Value is mapped to the X axis. Hue is mapped to the Y axis.
		/// </summary>
		ValueHue,
		/// <summary>
		/// Hue is mapped to the X axis. Saturation is mapped to the Y axis.
		/// </summary>
		HueSaturation,
		/// <summary>
		/// Saturation is mapped to the X axis. Hue is mapped to the Y axis.
		/// </summary>
		SaturationHue,
		/// <summary>
		/// Saturation is mapped to the X axis. Value is mapped to the Y axis.
		/// </summary>
		SaturationValue,
		/// <summary>
		/// Value is mapped to the X axis. Saturation is mapped to the Y axis.
		/// </summary>
		ValueSaturation,
	}

	public interface IColorSpectrumContext<T> : IWritableAhsvColorContext<T>
	{
		AhsvChannel XChannel { get; }
		AhsvChannel YChannel { get; }
		AhsvChannel ZChannel { get; }
		bool AffectRender { get; }
		bool AffectChildPosition { get; }
		void Render(DrawingContext drawingContext, Rect region);
		bool OnUpdateColorChannels(IChildPositionInfo<T> childPositionInfo);
		bool OnUpdateChildPosition(IChildPositionInfo<T> childPositionInfo);
		bool OnContainerChanged(IChildPositionInfo<T> childPositionInfo);
	}

	public abstract class ColorSpectrumContext : IColorSpectrumContext<float>
	{
		protected DependencyObject m_parent;
		protected readonly IAhsvColor<float> m_color;
		protected readonly AhsvChannel m_xChannel;
		protected readonly AhsvChannel m_yChannel;
		protected readonly AhsvChannel m_zChannel;
		protected int m_changedChannel;

		protected ColorSpectrumContext(DependencyObject parent, IAhsvColor<float> color, bool reverseAxis = false)
		{
			if (parent is null)
			{
				throw new ArgumentNullException(nameof(parent));
			}

			if (color is null)
			{
				throw new ArgumentNullException(nameof(color));
			}
			m_parent = parent;
			m_color = color;
			InitializeChannelsMapping(ref m_xChannel, ref m_yChannel, ref m_zChannel, reverseAxis);
		}

		protected abstract void InitializeChannelsMapping(ref AhsvChannel xChannel, ref AhsvChannel yChannel, ref AhsvChannel zChannel, bool reverseAxis);

		public AhsvChannel XChannel => m_xChannel;

		public AhsvChannel YChannel => m_yChannel;

		public AhsvChannel ZChannel => m_zChannel;

		public IAhsvColor<float> AhsvColor => m_color;

		public AhsvChannel ChangedAhsvChannel => (AhsvChannel)m_changedChannel;

		public abstract bool AffectRender { get; }
		public abstract bool AffectChildPosition { get; }

		public abstract void Render(DrawingContext drawingContext, Rect region);

		public virtual void ResetChangedChannel()
		{
			m_changedChannel = 0;
		}

		public virtual void SetChangedAhsvChannel(AhsvChannel channel)
		{
			m_changedChannel = (int)channel;
		}

		public virtual void AppendChangedAhsvChannel(AhsvChannel channel)
		{
			BitUtil.SetBit32Status(ref m_changedChannel, (int)channel, true);
		}

		public virtual void RemoveChangedAhsvChannel(AhsvChannel channel)
		{
			BitUtil.SetBit32Status(ref m_changedChannel, (int)channel, false);
		}

		public void SetParent(DependencyObject parent)
		{
			m_parent = parent;
		}

		protected static float GetChannelValue(IAhsvColor<float> colorSource, AhsvChannel channel)
		{
			if (colorSource != null)
			{
				switch (channel)
				{
					case AhsvChannel.Alpha:
						return colorSource.Alpha;
					case AhsvChannel.Hue:
						return colorSource.Hue;
					case AhsvChannel.Saturation:
						return colorSource.Saturation;
					case AhsvChannel.Value:
						return colorSource.Value;
					default:
						break;
				}
			}
			return 0;
		}

		protected static void SetChannelValue(IAhsvColor<float> colorSource, AhsvChannel channel, float channelValue)
		{
			if (colorSource != null)
			{
				switch (channel)
				{
					case AhsvChannel.Alpha:
						colorSource.Alpha = channelValue;
						return;
					case AhsvChannel.Hue:
						colorSource.Hue = channelValue;
						return;
					case AhsvChannel.Saturation:
						colorSource.Saturation = channelValue;
						return;
					case AhsvChannel.Value:
						colorSource.Value = channelValue;
						return;
					default:
						break;
				}
			}
		}

		public abstract bool OnUpdateColorChannels(IChildPositionInfo<float> childPositionInfo);
		public abstract bool OnUpdateChildPosition(IChildPositionInfo<float> childPositionInfo);
		public abstract bool OnContainerChanged(IChildPositionInfo<float> childPositionInfo);
	}

	public static class ColorSpectrumBrushes
	{
		public static readonly DrawingBrush UnitDrawingBrush = MakeFrozenUnitDrawingBrush();

		private static DrawingBrush MakeFrozenUnitDrawingBrush()
		{
			var rect1 = new RectangleGeometry(new Rect(0, 0, 6, 5));
			var rect2 = new RectangleGeometry(new Rect(0, 6, 6, 5));
			var rect3 = new RectangleGeometry(new Rect(6, 0, 6, 5));
			var rect4 = new RectangleGeometry(new Rect(6, 6, 6, 5));
			var rect5 = new RectangleGeometry(new Rect(0, 5, 12, 1));
			var gCol1 = new GeometryCollection() { rect1, rect4 };
			var gCol2 = new GeometryCollection() { rect2, rect3 };
			var drawing = new DrawingGroup()
			{
				Children = new DrawingCollection {
					new GeometryDrawing(
						new SolidColorBrush(Color.FromRgb(208, 206, 199)),
						null,
						new GeometryGroup() { Children = gCol1 }),
					new GeometryDrawing(
						Brushes.White,
						null,
						new GeometryGroup() { Children = gCol2 }),
					new GeometryDrawing(
						new SolidColorBrush(Color.FromRgb(231, 231, 226)),
						null,
						rect5)
				}
			};
			var drawingBrush = new DrawingBrush()
			{
				Drawing = drawing,
				Viewport = new Rect(0, 0, 12, 11),
				ViewportUnits = BrushMappingMode.Absolute,
				Stretch = Stretch.None,
				TileMode = TileMode.Tile
			};
			drawingBrush.Freeze();
			return drawingBrush;
		}

		private static LinearGradientBrush MakeFrozenLinearGradientBrush(Orientation orientation, params GradientStop[] gradientStops)
		{
			var gsc = new GradientStopCollection(gradientStops);
			var brush = new LinearGradientBrush(
				gsc,
				new Point(),
				orientation == Orientation.Vertical ? new Point(0, 1) : new Point(1, 0));
			brush.Freeze();
			return brush;
		}

		public static readonly LinearGradientBrush HorizontalFullHueSpectrumGradientBrush =
			MakeFrozenLinearGradientBrush(Orientation.Horizontal,
				new GradientStop(Color.FromArgb(0xff, 0xff, 0x00, 0x00), 0),
				new GradientStop(Color.FromArgb(0xff, 0xff, 0xff, 0x00), 1.0 / 6.0),
				new GradientStop(Color.FromArgb(0xff, 0x00, 0xff, 0x00), 1.0 / 3.0),
				new GradientStop(Color.FromArgb(0xff, 0x00, 0xff, 0xff), 0.5),
				new GradientStop(Color.FromArgb(0xff, 0x00, 0x00, 0xff), 2.0 / 3.0),
				new GradientStop(Color.FromArgb(0xff, 0xff, 0x00, 0xff), 5.0 / 6.0),
				new GradientStop(Color.FromArgb(0xff, 0xff, 0x00, 0x00), 1)
				);

		public static readonly LinearGradientBrush VerticalFullHueSpectrumGradientBrush =
			MakeFrozenLinearGradientBrush(Orientation.Vertical,
				new GradientStop(Color.FromArgb(0xff, 0xff, 0x00, 0x00), 1),
				new GradientStop(Color.FromArgb(0xff, 0xff, 0xff, 0x00), 5.0 / 6.0),
				new GradientStop(Color.FromArgb(0xff, 0x00, 0xff, 0x00), 2.0 / 3.0),
				new GradientStop(Color.FromArgb(0xff, 0x00, 0xff, 0xff), 0.5),
				new GradientStop(Color.FromArgb(0xff, 0x00, 0x00, 0xff), 1.0 / 3.0),
				new GradientStop(Color.FromArgb(0xff, 0xff, 0x00, 0xff), 1.0 / 6.0),
				new GradientStop(Color.FromArgb(0xff, 0xff, 0x00, 0x00), 0)
				);

		internal static readonly GradientStop BlackBeginningGradientStop = new GradientStop(Colors.Black, 0);
		internal static readonly GradientStop BlackEndingGradientStop = new GradientStop(Colors.Black, 1);

		public static readonly LinearGradientBrush HorizontalSaturationGradientBrush =
			MakeFrozenLinearGradientBrush(Orientation.Horizontal, new GradientStop(Colors.White, 0), new GradientStop(Colors.Transparent, 1));
		public static readonly LinearGradientBrush VerticalValueGradientBrush =
			MakeFrozenLinearGradientBrush(Orientation.Vertical, new GradientStop(Color.FromArgb(0, 0, 0, 0), 0), BlackEndingGradientStop);

		public static readonly LinearGradientBrush VerticalSaturationGradientBrush =
			MakeFrozenLinearGradientBrush(Orientation.Vertical, new GradientStop(Colors.White, 1), new GradientStop(Colors.Transparent, 0));
		public static readonly LinearGradientBrush HorizontalValueGradientBrush =
			MakeFrozenLinearGradientBrush(Orientation.Horizontal, new GradientStop(Color.FromArgb(0, 0, 0, 0), 1), BlackBeginningGradientStop);

	}

	public sealed class RgbColorToColorIndicatorForegroundConverter : IValueConverter
	{
		private readonly RgbColorToGrayscaleValueConverter m_innerConverter = new RgbColorToGrayscaleValueConverter();

		public Brush BrushInDarkColor { get; set; } = Brushes.White;

		public Brush BrushInLightColor { get; set; } = Brushes.Black;

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var grayScale = (byte)m_innerConverter.Convert(value, null, null, null);
			if (grayScale < 128)
			{
				return BrushInDarkColor;
			}
			return BrushInLightColor;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class ColorSpectrum : FrameworkElement
	{
		internal sealed class SolidChannels : IColorChannels
		{
			private byte[] m_bgr = new byte[3];
			private float[] m_hsv = new float[3];
			private ArgbChannel m_changedArgbChannel;
			private AhsvChannel m_changedAhsvChannel;

			public byte A { get => 255; set { } }

			public byte R
			{
				get => m_bgr[2];
				set
				{
					if (value != R)
					{
						m_bgr[2] = value;
						m_changedArgbChannel |= ArgbChannel.Red;
						SyncToColorHsv();
					}
				}
			}

			public byte G
			{
				get => m_bgr[1];
				set
				{
					if (value != G)
					{
						m_bgr[1] = value;
						m_changedArgbChannel |= ArgbChannel.Green;
						SyncToColorHsv();
					}
				}
			}

			public byte B
			{
				get => m_bgr[0];
				set
				{
					if (value != B)
					{
						m_bgr[0] = value;
						m_changedArgbChannel |= ArgbChannel.Blue;
						SyncToColorHsv();
					}
				}
			}

			public float Alpha { get => 1; set { } }

			public float Hue
			{
				get => m_hsv[0];
				set
				{
					if (value != Hue)
					{
						m_hsv[0] = value;
						m_changedAhsvChannel |= AhsvChannel.Hue;
						SyncToColorRgb();
					}
				}
			}

			public float Saturation
			{
				get => m_hsv[1];
				set
				{
					if (value != Saturation)
					{
						m_hsv[1] = value;
						m_changedAhsvChannel |= AhsvChannel.Saturation;
						SyncToColorRgb();
					}
				}
			}

			public float Value
			{
				get => m_hsv[2];
				set
				{
					if (value != Value)
					{
						m_hsv[2] = value;
						m_changedAhsvChannel |= AhsvChannel.Value;
						SyncToColorRgb();
					}
				}
			}

			public ArgbChannel ChangedArgbChannel => m_changedArgbChannel;

			public AhsvChannel ChangedAhsvChannel => m_changedAhsvChannel;

			IArgbColor<byte> IArgbColorContext<byte>.ArgbColor => this;

			IAhsvColor<float> IAhsvColorContext<float>.AhsvColor => this;

			public void SetColorFromBytesArgb(byte r, byte g, byte b, byte a = 255)
			{
				if (r != R)
				{
					m_bgr[2] = r;
					m_changedArgbChannel |= ArgbChannel.Red;
				}
				if (g != G)
				{
					m_bgr[1] = g;
					m_changedArgbChannel |= ArgbChannel.Green;
				}
				if (b != B)
				{
					m_bgr[0] = b;
					m_changedArgbChannel |= ArgbChannel.Blue;
				}
				SyncToColorHsv();
			}

			public void SetColorFromFloatAhsv(float hf, float sf, float vf, float af = 1)
			{
				var ahsvChannel = AhsvChannel.None;
				if (m_hsv[0] != hf)
				{
					m_hsv[0] = hf;
					ahsvChannel |= AhsvChannel.Hue;
				}
				if (m_hsv[1] != sf)
				{
					m_hsv[1] = sf;
					ahsvChannel |= AhsvChannel.Saturation;
				}
				if (m_hsv[2] != vf)
				{
					m_hsv[2] = vf;
					ahsvChannel |= AhsvChannel.Value;
				}
				if (ahsvChannel != AhsvChannel.None)
				{
					m_changedAhsvChannel |= ahsvChannel;
					SyncToColorRgb();
				}
			}

			public void SetColorFromFloatArgb(float rf, float gf, float bf, float af = 1)
			{
				SetColorFromBytesArgb(ColorsUtility.ColorByte(rf), ColorsUtility.ColorByte(gf), ColorsUtility.ColorByte(bf));
			}

			public void SetColorFromInt32Argb(int colorInt)
			{
				ColorsUtility.ColorBytes(colorInt, out byte _, out byte r, out byte g, out byte b);
				SetColorFromBytesArgb(r, g, b);
			}

			public void ZeroColor()
			{
				Array.Clear(m_bgr, 0, m_bgr.Length);
				Array.Clear(m_hsv, 0, m_hsv.Length);
				m_changedArgbChannel = ArgbChannel.All;
				m_changedAhsvChannel = AhsvChannel.All;
			}

			private void SyncToColorRgb()
			{
				ColorsUtility.ConvertHsvF3ToRgbB3(Hue, Saturation, Value, out byte r, out byte g, out byte b);
				if (r != R)
				{
					m_bgr[2] = r;
					m_changedArgbChannel |= ArgbChannel.Red;
				}
				if (g != G)
				{
					m_bgr[1] = g;
					m_changedArgbChannel |= ArgbChannel.Green;
				}
				if (b != B)
				{
					m_bgr[0] = b;
					m_changedArgbChannel |= ArgbChannel.Blue;
				}
			}

			private void SyncToColorHsv()
			{
				ColorsUtility.ConvertRgbB3ToHsvF3(R, G, B, out float hf, out float sf, out float vf);
				if (Hue != hf)
				{
					m_hsv[0] = hf;
					m_changedAhsvChannel |= AhsvChannel.Hue;
				}
				if (Saturation != sf)
				{
					m_hsv[1] = sf;
					m_changedAhsvChannel |= AhsvChannel.Saturation;
				}
				if (Value != vf)
				{
					m_hsv[2] = vf;
					m_changedAhsvChannel |= AhsvChannel.Value;
				}
			}

			public void ResetChangedChannel()
			{
				m_changedArgbChannel = ArgbChannel.None;
				m_changedAhsvChannel = AhsvChannel.None;
			}
		}

		internal sealed class IndicatorPositionInfo : IChildPositionInfo<float>
		{
			private float[] m_info = new float[4];

			public float this[int index] { get => m_info[index]; set => m_info[index] = value; }

			public float ContainerWidth { get => m_info[0]; set => m_info[0] = value; }
			public float ContainerHeight { get => m_info[1]; set => m_info[1] = value; }
			public float ChildPositionX { get => m_info[2]; set => m_info[2] = value; }
			public float ChildPositionY { get => m_info[3]; set => m_info[3] = value; }

			public int Count => m_info.Length;

			public IEnumerator<float> GetEnumerator()
			{
				return ((IList<float>)m_info).GetEnumerator();
			}

			public void SetChildPosition(float positionX, float positionY)
			{
				m_info[2] = positionX;
				m_info[3] = positionY;
			}

			public void SetContainerSize(float width, float height)
			{
				m_info[0] = width;
				m_info[1] = height;
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return m_info.GetEnumerator();
			}
		}

		internal abstract class HsvColorSpectrumContext : ColorSpectrumContext
		{
			protected HsvColorSpectrumContext(DependencyObject parent, IAhsvColor<float> color, bool reverseAxis = false) 
				: base(parent, color, reverseAxis)
			{
			}

			public override bool OnUpdateColorChannels(IChildPositionInfo<float> childPositionInfo)
			{
				if (childPositionInfo is null)
				{
					return false;
				}
				var xChannel = childPositionInfo.ChildPositionX / childPositionInfo.ContainerWidth;
				var yChannel = 1 - childPositionInfo.ChildPositionY / childPositionInfo.ContainerHeight;
				SetChannelValue(m_color, m_xChannel, xChannel);
				SetChannelValue(m_color, m_yChannel, yChannel);
				ColorSpectrum.SetHsvColorChannelsProperty(m_parent, m_xChannel, xChannel);
				ColorSpectrum.SetHsvColorChannelsProperty(m_parent, m_yChannel, yChannel);
				return true;
			}

			public override bool OnUpdateChildPosition(IChildPositionInfo<float> childPositionInfo)
			{
				if (childPositionInfo is null)
				{
					return false;
				}
				if (AffectChildPosition)
				{
					var xChannelVal = GetChannelValue(m_color, m_xChannel);
					var yChannelVal = GetChannelValue(m_color, m_yChannel);
					childPositionInfo.ChildPositionX = xChannelVal * childPositionInfo.ContainerWidth;
					childPositionInfo.ChildPositionY = (1 - yChannelVal) * childPositionInfo.ContainerHeight;
					RemoveChangedAhsvChannel(m_xChannel);
					RemoveChangedAhsvChannel(m_yChannel);
					return true;
				}
				return false;
			}

			public override bool OnContainerChanged(IChildPositionInfo<float> childPositionInfo)
			{
				if (childPositionInfo is null)
				{
					return false;
				}
				var xChannelVal = GetChannelValue(m_color, m_xChannel);
				var yChannelVal = GetChannelValue(m_color, m_yChannel);
				childPositionInfo.ChildPositionX = xChannelVal * childPositionInfo.ContainerWidth;
				childPositionInfo.ChildPositionY = (1 - yChannelVal) * childPositionInfo.ContainerHeight;
				return true;
			}
		}

		internal sealed class HueValueContext : HsvColorSpectrumContext
		{
			private LinearGradientBrush m_saturationGradientBrush;

			public HueValueContext(DependencyObject parent, IAhsvColor<float> color, bool reverseAxis = false) : base(parent, color, reverseAxis)
			{
			}

			public override bool AffectRender => BitUtil.GetBit32Status(ref m_changedChannel, (int)AhsvChannel.Saturation);

			public override bool AffectChildPosition =>
				BitUtil.GetBit32Status(ref m_changedChannel, (int)AhsvChannel.Hue)
				|| BitUtil.GetBit32Status(ref m_changedChannel, (int)AhsvChannel.Value);

			public LinearGradientBrush SaturationGradientBrush
			{
				get 
				{
					var isNullBrush = m_saturationGradientBrush is null;
					var containsSaturationMask = AffectRender;
					if (isNullBrush || containsSaturationMask)
					{
						var alpha = ColorsUtility.ColorByte(1 - m_color.Saturation);
						var whiteValue = Color.FromArgb(alpha, 255, 255, 255);
						if (isNullBrush)
						{
							m_saturationGradientBrush = new LinearGradientBrush();
						}
						m_saturationGradientBrush.GradientStops.Clear();
						if (m_yChannel == AhsvChannel.Value)
						{
							// Vertical
							var gradientStop = new GradientStop(whiteValue, 0);
							m_saturationGradientBrush.GradientStops.Add(gradientStop);
							m_saturationGradientBrush.GradientStops.Add(ColorSpectrumBrushes.BlackEndingGradientStop);
							m_saturationGradientBrush.EndPoint = new Point(0, 1);
						}
						else
						{
							// Horizontal
							var gradientStop = new GradientStop(whiteValue, 1);
							m_saturationGradientBrush.GradientStops.Add(ColorSpectrumBrushes.BlackBeginningGradientStop);
							m_saturationGradientBrush.GradientStops.Add(gradientStop);
							m_saturationGradientBrush.EndPoint = new Point(1, 0);
						}

						if (containsSaturationMask)
						{
							RemoveChangedAhsvChannel(AhsvChannel.Saturation);
						}
					}
					return m_saturationGradientBrush;
				}
			}

			public override void Render(DrawingContext drawingContext, Rect region)
			{
				if (m_xChannel == AhsvChannel.Hue)
				{
					drawingContext.DrawRectangle(ColorSpectrumBrushes.HorizontalFullHueSpectrumGradientBrush, null, region);
				}
				else
				{
					drawingContext.DrawRectangle(ColorSpectrumBrushes.VerticalFullHueSpectrumGradientBrush, null, region);
				}
				drawingContext.DrawRectangle(SaturationGradientBrush, null, region);
			}

			protected override void InitializeChannelsMapping(ref AhsvChannel xChannel, ref AhsvChannel yChannel, ref AhsvChannel zChannel, bool reverseAxis)
			{
				if (reverseAxis)
				{
					xChannel = AhsvChannel.Value;
					yChannel = AhsvChannel.Hue;
				}
				else
				{
					xChannel = AhsvChannel.Hue;
					yChannel = AhsvChannel.Value;
				}
				zChannel = AhsvChannel.Saturation;
			}
		}

		internal sealed class HueSaturationContext : HsvColorSpectrumContext
		{
			private SolidColorBrush m_valueSolidColorBrush;

			public HueSaturationContext(DependencyObject parent, IAhsvColor<float> color, bool reverseAxis = false) : base(parent, color, reverseAxis)
			{
			}

			public override bool AffectRender => BitUtil.GetBit32Status(ref m_changedChannel, (int)AhsvChannel.Value);

			public override bool AffectChildPosition =>
				BitUtil.GetBit32Status(ref m_changedChannel, (int)AhsvChannel.Hue)
				|| BitUtil.GetBit32Status(ref m_changedChannel, (int)AhsvChannel.Saturation);

			public SolidColorBrush ValueSolidColorBrush
			{ 
				get 
				{
					var isNullBrush = m_valueSolidColorBrush is null;
					var containsValueMask = AffectRender;
					if (isNullBrush || containsValueMask)
					{
						var alpha = ColorsUtility.ColorByte(1-m_color.Value);
						var blackValue = Color.FromArgb(alpha, 0, 0, 0);
						if (isNullBrush)
						{
							m_valueSolidColorBrush = new SolidColorBrush(blackValue);
						}
						else
						{
							m_valueSolidColorBrush.Color = blackValue;
						}
						if (containsValueMask)
						{
							RemoveChangedAhsvChannel(AhsvChannel.Value);
						}
					}
					return m_valueSolidColorBrush; 
				}
			}

			public override void Render(DrawingContext drawingContext, Rect region)
			{
				if (m_xChannel == AhsvChannel.Hue)
				{
					drawingContext.DrawRectangle(ColorSpectrumBrushes.HorizontalFullHueSpectrumGradientBrush, null, region);
					drawingContext.DrawRectangle(ColorSpectrumBrushes.VerticalSaturationGradientBrush, null, region);

				}
				else
				{
					drawingContext.DrawRectangle(ColorSpectrumBrushes.VerticalFullHueSpectrumGradientBrush, null, region);
					drawingContext.DrawRectangle(ColorSpectrumBrushes.HorizontalSaturationGradientBrush, null, region);

				}
				drawingContext.DrawRectangle(ValueSolidColorBrush, null, region);
			}

			protected override void InitializeChannelsMapping(ref AhsvChannel xChannel, ref AhsvChannel yChannel, ref AhsvChannel zChannel, bool reverseAxis)
			{
				if (reverseAxis)
				{
					xChannel = AhsvChannel.Saturation;
					yChannel = AhsvChannel.Hue;
				}
				else
				{
					xChannel = AhsvChannel.Hue;
					yChannel = AhsvChannel.Saturation;
				}
				zChannel = AhsvChannel.Value;
			}
		}

		internal sealed class SaturationValueContext : HsvColorSpectrumContext
		{
			private SolidColorBrush m_hueSolidColorBrush;

			public SaturationValueContext(DependencyObject parent, IAhsvColor<float> color, bool reverseAxis = false) : base(parent, color, reverseAxis)
			{
			}

			public SolidColorBrush HueSolidColorBrush
			{
				get
				{
					var isNullBrush = m_hueSolidColorBrush is null;
					var containsHueMask = AffectRender;
					if (isNullBrush || containsHueMask)
					{
						ColorsUtility.ConvertAhsvF4ToArgbB4(1, m_color.Hue, 1, 1, out byte _, out byte r, out byte g, out byte b);
						var color = Color.FromRgb(r, g, b);
						if (isNullBrush)
						{
							m_hueSolidColorBrush = new SolidColorBrush(color);
						}
						else
						{
							m_hueSolidColorBrush.Color = color;
						}
						if (containsHueMask)
						{
							RemoveChangedAhsvChannel(AhsvChannel.Hue);
						}
					}
					return m_hueSolidColorBrush;
				}
			}

			public override bool AffectRender => BitUtil.GetBit32Status(ref m_changedChannel, (int)AhsvChannel.Hue);

			public override bool AffectChildPosition => 
				BitUtil.GetBit32Status(ref m_changedChannel, (int)AhsvChannel.Saturation)
				|| BitUtil.GetBit32Status(ref m_changedChannel, (int)AhsvChannel.Value);

			public override void Render(DrawingContext drawingContext, Rect region)
			{
				drawingContext.DrawRectangle(HueSolidColorBrush, null, region);
				if (m_xChannel == AhsvChannel.Saturation)
				{
					drawingContext.DrawRectangle(ColorSpectrumBrushes.HorizontalSaturationGradientBrush, null, region);
					drawingContext.DrawRectangle(ColorSpectrumBrushes.VerticalValueGradientBrush, null, region);
				}
				else
				{
					drawingContext.DrawRectangle(ColorSpectrumBrushes.VerticalSaturationGradientBrush, null, region);
					drawingContext.DrawRectangle(ColorSpectrumBrushes.HorizontalValueGradientBrush, null, region);
				}

			}

			protected override void InitializeChannelsMapping(ref AhsvChannel xChannel, ref AhsvChannel yChannel, ref AhsvChannel zChannel, bool reverseAxis)
			{
				if (reverseAxis)
				{
					xChannel = AhsvChannel.Value;
					yChannel = AhsvChannel.Saturation;
				}
				else
				{
					xChannel = AhsvChannel.Saturation;
					yChannel = AhsvChannel.Value;
				}
				zChannel = AhsvChannel.Hue;
			}
		}

		public const double DefaultIndicatorSize = 16;
		public const ColorSpectrumComponents DefaultComponents = ColorSpectrumComponents.SaturationValue;

		private bool m_isDragging;
		private ContentControl m_indicator;
		private Size m_indicatorBound;
		private Point m_originPoint;
		private int m_components;
		private SolidChannels m_channels;
		private IColorSpectrumContext<float>[] m_contexts;
		private bool m_suppressChildPositionChange;
		private bool m_suppressHsvChannelChange;
		private bool m_suppressRgbChannelChange;

		private IChildPositionInfo<float> m_cacheInfo;

		private IColorSpectrumContext<float> CurrentContext => m_contexts[m_components];

		private IChildPositionInfo<float> GetChildPositionInfo(float x = 0, float y = 0)
		{
			m_cacheInfo.SetContainerSize((float)ActualWidth, (float)ActualHeight);
			m_cacheInfo.SetChildPosition(x, y);
			return m_cacheInfo;
		}

		public IColorChannels Channels { get => m_channels; }

		protected override int VisualChildrenCount => m_indicator is null ? 0 : 1;

		public double IndicatorHeight
		{
			get { return (double)GetValue(IndicatorHeightProperty); }
			set { SetValue(IndicatorHeightProperty, value); }
		}

		// Using a DependencyProperty as the backing store for IndicatorHeight.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IndicatorHeightProperty =
			DependencyProperty.Register("IndicatorHeight", typeof(double), typeof(ColorSpectrum), new FrameworkPropertyMetadata(DefaultIndicatorSize, FrameworkPropertyMetadataOptions.AffectsMeasure));

		public double IndicatorWidth
		{
			get { return (double)GetValue(IndicatorWidthProperty); }
			set { SetValue(IndicatorWidthProperty, value); }
		}

		// Using a DependencyProperty as the backing store for IndicatorWidth.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IndicatorWidthProperty =
			DependencyProperty.Register("IndicatorWidth", typeof(double), typeof(ColorSpectrum), new FrameworkPropertyMetadata(DefaultIndicatorSize, FrameworkPropertyMetadataOptions.AffectsMeasure));

		public ColorSpectrumComponents Components
		{
			get { return (ColorSpectrumComponents)GetValue(ComponentsProperty); }
			set { SetValue(ComponentsProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Components.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ComponentsProperty =
			DependencyProperty.Register("Components", typeof(ColorSpectrumComponents), typeof(ColorSpectrum), new FrameworkPropertyMetadata(DefaultComponents, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnComponentsPropertyChanged)));

		public DataTemplate IndicatorTemplate
		{
			get { return (DataTemplate)GetValue(IndicatorTemplateProperty); }
			set { SetValue(IndicatorTemplateProperty, value); }
		}

		// Using a DependencyProperty as the backing store for IndicatorTemplate.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IndicatorTemplateProperty =
			DependencyProperty.Register("IndicatorTemplate", typeof(DataTemplate), typeof(ColorSpectrum), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnIndicatorTemplateChanged)));

		public DataTemplateSelector IndicatorTemplateSelector
		{
			get { return (DataTemplateSelector)GetValue(IndicatorTemplateSelectorProperty); }
			set { SetValue(IndicatorTemplateSelectorProperty, value); }
		}

		// Using a DependencyProperty as the backing store for IndicatorTemplateSelector.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IndicatorTemplateSelectorProperty =
			DependencyProperty.Register("IndicatorTemplateSelector", typeof(DataTemplateSelector), typeof(ColorSpectrum), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnIndicatorTemplateSelectorChanged)));

		public string IndicatorStringFormat
		{
			get { return (string)GetValue(IndicatorStringFormatProperty); }
			set { SetValue(IndicatorStringFormatProperty, value); }
		}

		// Using a DependencyProperty as the backing store for IndicatorStringFormat.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IndicatorStringFormatProperty =
			DependencyProperty.Register("IndicatorStringFormat", typeof(string), typeof(ColorSpectrum), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnIndicatorStringFormatChanged)));

		public double Hue
		{
			get { return (double)GetValue(HueProperty); }
			set { SetValue(HueProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Hue.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty HueProperty =
			DependencyProperty.Register("Hue", typeof(double), typeof(ColorSpectrum), new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnHsvColorChannelsChanged),new CoerceValueCallback(OnCoerceHsvColorChannels)));

		public double Saturation
		{
			get { return (double)GetValue(SaturationProperty); }
			set { SetValue(SaturationProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Saturation.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty SaturationProperty =
			DependencyProperty.Register("Saturation", typeof(double), typeof(ColorSpectrum), new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnHsvColorChannelsChanged)));

		public double Value
		{
			get { return (double)GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register("Value", typeof(double), typeof(ColorSpectrum), new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnHsvColorChannelsChanged)));

		public byte Red
		{
			get { return (byte)GetValue(RedProperty); }
			set { SetValue(RedProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Red.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty RedProperty =
			DependencyProperty.Register("Red", typeof(byte), typeof(ColorSpectrum), new FrameworkPropertyMetadata(default(byte), new PropertyChangedCallback(OnRgbColorChannelsChanged)));

		public byte Green
		{
			get { return (byte)GetValue(GreenProperty); }
			set { SetValue(GreenProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Green.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty GreenProperty =
			DependencyProperty.Register("Green", typeof(byte), typeof(ColorSpectrum), new FrameworkPropertyMetadata(default(byte), new PropertyChangedCallback(OnRgbColorChannelsChanged)));

		public byte Blue
		{
			get { return (byte)GetValue(BlueProperty); }
			set { SetValue(BlueProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Blue.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty BlueProperty =
			DependencyProperty.Register("Blue", typeof(byte), typeof(ColorSpectrum), new FrameworkPropertyMetadata(default(byte), new PropertyChangedCallback(OnRgbColorChannelsChanged)));

		public Color RgbColor
		{
			get { return (Color)GetValue(RgbColorProperty); }
			protected internal set { SetValue(RgbColorProperty, value); }
		}

		// Using a DependencyProperty as the backing store for RgbColor.  This enables animation, styling, binding, etc...
		protected internal static readonly DependencyPropertyKey RgbColorPropertyKey =
			DependencyProperty.RegisterReadOnly("RgbColor", typeof(Color), typeof(ColorSpectrum), new PropertyMetadata(default(Color), new PropertyChangedCallback(OnRgbColorPropertyChanged)));

		public static DependencyProperty RgbColorProperty = RgbColorPropertyKey.DependencyProperty;

		public event RoutedPropertyChangedEventHandler<Color> RgbColorChanged
		{
			add { AddHandler(RgbColorChangedEvent, value); }
			remove { RemoveHandler(RgbColorChangedEvent, value); }
		}

		public static readonly RoutedEvent RgbColorChangedEvent =
			EventManager.RegisterRoutedEvent("RgbColorChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<Color>), typeof(ColorSpectrum));


		private static void OnComponentsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is ColorSpectrum colorSpectrum)
			{
				var oldContext = colorSpectrum.CurrentContext;
				colorSpectrum.m_components = (int)(ColorSpectrumComponents)e.NewValue;
				var newContext = colorSpectrum.CurrentContext;
				newContext.SetChangedAhsvChannel(oldContext.ChangedAhsvChannel);
				var info = colorSpectrum.GetChildPositionInfo();
				if (newContext.OnContainerChanged(info))
				{
					colorSpectrum.UpdateIndicatorTransformCore(info.ChildPositionX, info.ChildPositionY);
				}
			}
		}

		private static void OnIndicatorTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is ColorSpectrum colorSpectrum)
			{
				colorSpectrum.m_indicator?.SetValue(ContentControl.ContentTemplateProperty, e.NewValue);
			}
		}

		private static void OnIndicatorTemplateSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is ColorSpectrum colorSpectrum)
			{
				colorSpectrum.m_indicator?.SetValue(ContentControl.ContentTemplateSelectorProperty, e.NewValue);
			}
		}

		private static void OnIndicatorStringFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is ColorSpectrum colorSpectrum)
			{
				colorSpectrum.m_indicator?.SetValue(ContentControl.ContentStringFormatProperty, e.NewValue);
			}
		}

		private static object OnCoerceHsvColorChannels(DependencyObject d, object baseValue)
		{
			var channel = Convert.ToDouble(baseValue);
			if (channel < 0)
			{
				channel = 0;
			}
			if (channel > 1)
			{
				channel = 1;
			}
			return channel;
		}

		private static void OnHsvColorChannelsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is ColorSpectrum colorSpectrum)
			{
				var channels = colorSpectrum.m_channels;
				var gIndex = e.Property.GlobalIndex;
				if (gIndex == HueProperty.GlobalIndex)
				{
					channels.Hue = Convert.ToSingle(e.NewValue);
				}
				else if (gIndex == SaturationProperty.GlobalIndex)
				{
					channels.Saturation = Convert.ToSingle(e.NewValue);
				}
				else if (gIndex == ValueProperty.GlobalIndex)
				{
					channels.Value = Convert.ToSingle(e.NewValue);
				}
				else
				{
					return;
				}
				colorSpectrum.OnUpdateIndicatorPositionFromHsv();
				colorSpectrum.OnUpdateRgbChannelsFromHsv();
				channels.ResetChangedChannel();
			}
		}

		private static void OnRgbColorChannelsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is ColorSpectrum colorSpectrum)
			{
				var channels = colorSpectrum.m_channels;
				var gIndex = e.Property.GlobalIndex;
				if (gIndex == RedProperty.GlobalIndex)
				{
					channels.R = Convert.ToByte(e.NewValue);
				}
				else if (gIndex == GreenProperty.GlobalIndex)
				{
					channels.G = Convert.ToByte(e.NewValue);
				}
				else if (gIndex == BlueProperty.GlobalIndex)
				{
					channels.B = Convert.ToByte(e.NewValue);
				}
				else
				{
					return;
				}
				colorSpectrum.OnUpdateHsvChannelsFromRgb();
				channels.ResetChangedChannel();
			}
		}

		private static void OnRgbColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is ColorSpectrum colorSpectrum)
			{
				var oldColor = e.OldValue as Color?;
				var newColor = e.NewValue as Color?;
				colorSpectrum.RaiseEvent(
					new RoutedPropertyChangedEventArgs<Color>(
						oldColor.GetValueOrDefault(), 
						newColor.GetValueOrDefault(), 
						RgbColorChangedEvent)
					);
			}
		}

		private void OnUpdateHsvChannelsFromIndicatorPos(double x, double y)
		{
			m_suppressChildPositionChange = true;
			var context = CurrentContext;
			context?.OnUpdateColorChannels(GetChildPositionInfo((float)x, (float)y));
			m_channels.ResetChangedChannel();
			m_suppressChildPositionChange = false;
		}

		private void OnUpdateIndicatorPositionFromHsv()
		{
			if (!m_suppressChildPositionChange)
			{
				var context = CurrentContext;
				if (context != null)
				{
					var changedAhsvChannel = m_channels.ChangedAhsvChannel;
					context.AppendChangedAhsvChannel(changedAhsvChannel);
					if (context.OnUpdateChildPosition(GetChildPositionInfo()))
					{
						UpdateIndicatorTransformCore(m_cacheInfo.ChildPositionX, m_cacheInfo.ChildPositionY);
					}
					if (context.AffectRender)
					{
						InvalidateVisual();
					}
				}
			}
		}

		private void OnUpdateRgbChannelsFromHsv()
		{
			if (!m_suppressRgbChannelChange)
			{
				m_suppressHsvChannelChange = true;
				// update RGB channels
				var changedArgbChannel = m_channels.ChangedArgbChannel;
				if (changedArgbChannel > ArgbChannel.None)
				{
					var changedChannel = (int)changedArgbChannel;
					if (BitUtil.GetBit32Status(ref changedChannel, (int)ArgbChannel.Red))
					{
						SetRgbColorChannelsProperty(this, ArgbChannel.Red, m_channels.R);
					}
					if (BitUtil.GetBit32Status(ref changedChannel, (int)ArgbChannel.Green))
					{
						SetRgbColorChannelsProperty(this, ArgbChannel.Green, m_channels.G);
					}
					if (BitUtil.GetBit32Status(ref changedChannel, (int)ArgbChannel.Blue))
					{
						SetRgbColorChannelsProperty(this, ArgbChannel.Blue, m_channels.B);
					}
					OnUpdateRgbColorProperty(Color.FromRgb(m_channels.R, m_channels.G, m_channels.B));
				}
				m_suppressHsvChannelChange = false;
			}
		}

		private void OnUpdateHsvChannelsFromRgb()
		{
			if (!m_suppressHsvChannelChange)
			{
				m_suppressRgbChannelChange = true;
				var changedAhsvChannel = m_channels.ChangedAhsvChannel;
				if (changedAhsvChannel > AhsvChannel.None)
				{
					var changedChannel = (int)changedAhsvChannel;
					if (BitUtil.GetBit32Status(ref changedChannel, (int)AhsvChannel.Hue))
					{
						SetHsvColorChannelsProperty(this, AhsvChannel.Hue, m_channels.Hue);
					}
					if (BitUtil.GetBit32Status(ref changedChannel, (int)AhsvChannel.Saturation))
					{
						SetHsvColorChannelsProperty(this, AhsvChannel.Saturation, m_channels.Saturation);
					}
					if (BitUtil.GetBit32Status(ref changedChannel, (int)AhsvChannel.Value))
					{
						SetHsvColorChannelsProperty(this, AhsvChannel.Value, m_channels.Value);
					}
					OnUpdateRgbColorProperty(Color.FromRgb(m_channels.R, m_channels.G, m_channels.B));
				}
				m_suppressRgbChannelChange = false;
			}
		}

		private void OnUpdateRgbColorProperty(Color newColor)
		{
			SetValue(RgbColorPropertyKey, newColor);
		}

		private static void SetHsvColorChannelsProperty(DependencyObject d, AhsvChannel channel, double channelValue)
		{
			switch (channel)
			{
				case AhsvChannel.Hue:
					{
						d.SetCurrentValue(HueProperty, channelValue);
					}
					break;
				case AhsvChannel.Saturation:
					{
						d.SetCurrentValue(SaturationProperty, channelValue);
					}
					break;
				case AhsvChannel.Value:
					{
						d.SetCurrentValue(ValueProperty, channelValue);
					}
					break;
				default:
					break;
			}
		}

		private static void SetRgbColorChannelsProperty(DependencyObject d, ArgbChannel channel, byte channelValue)
		{
			switch (channel)
			{
				case ArgbChannel.Red:
					{
						d.SetCurrentValue(RedProperty, channelValue);
					}
					break;
				case ArgbChannel.Green:
					{
						d.SetCurrentValue(GreenProperty, channelValue);
					}
					break;
				case ArgbChannel.Blue:
					{
						d.SetCurrentValue(BlueProperty, channelValue);
					}
					break;
				default:
					break;
			}
		}

		public ColorSpectrum()
		{
			m_channels = new SolidChannels();
			m_contexts = new IColorSpectrumContext<float>[6];
			m_contexts[0] = new HueValueContext(this, m_channels);
			m_contexts[1] = new HueValueContext(this, m_channels,true);
			m_contexts[2] = new HueSaturationContext(this, m_channels);
			m_contexts[3] = new HueSaturationContext(this, m_channels, true);
			m_contexts[4] = new SaturationValueContext(this, m_channels);
			m_contexts[5] = new SaturationValueContext(this, m_channels, true);
			m_components = (int)ColorSpectrumComponents.SaturationValue;
			m_cacheInfo = new IndicatorPositionInfo();
			PrepareIndicator();
		}

		protected override Visual GetVisualChild(int index)
		{
			return m_indicator;
		}

		private void PrepareIndicator()
		{
			if (m_indicator is null)
			{
				m_indicator = new ContentControl();
				var template = IndicatorTemplate;
				if (template != null)
				{
					m_indicator.SetValue(ContentControl.ContentTemplateProperty, template);
				}
				var templateSelector = IndicatorTemplateSelector;
				if (templateSelector != null)
				{
					m_indicator.SetValue(ContentControl.ContentTemplateSelectorProperty, templateSelector);
				}
				var stringFormat = IndicatorStringFormat;
				if (stringFormat != null)
				{
					m_indicator.SetValue(ContentControl.ContentStringFormatProperty, stringFormat);
				}
				m_indicator.RenderTransform = new TranslateTransform();
				m_indicator.RenderTransformOrigin = new Point(0.5, 0.5);
				AddVisualChild(m_indicator);
			}
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			var region = new Rect(RenderSize);
			var guidelineSet = new GuidelineSet(new double[] { region.X, region.Width }, new double[] { region.Y, region.Height });
			drawingContext.PushGuidelineSet(guidelineSet);
			var index = m_components;
			var renderer = m_contexts[index];
			renderer?.Render(drawingContext, region);
			drawingContext.Pop();
		}

		private static void CoerceIndicatorSize(ref double value)
		{
			if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0)
			{
				value = DefaultIndicatorSize;
			}
		}

		private static void CoerceMousePoint(ref double x, ref double y, double w, double h)
		{
			if (x < 0)
			{
				x = 0;
			}
			if (x > w)
			{
				x = w;
			}
			if (y < 0)
			{
				y = 0;
			}
			if (y > h)
			{
				y = h;
			}
		}

		protected override Size MeasureOverride(Size availableSize)
		{
			var w = IndicatorWidth;
			var h = IndicatorHeight;
			CoerceIndicatorSize(ref w);
			CoerceIndicatorSize(ref h);
			m_indicatorBound = new Size(w, h);
			m_originPoint = new Point(-w / 2, -h / 2);
			m_indicator?.Measure(m_indicatorBound);
			return m_indicatorBound;
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			m_indicator?.Arrange(new Rect(m_originPoint, m_indicatorBound));
			return finalSize;
		}

		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			BeginDragging(e);
			base.OnMouseLeftButtonDown(e);
		}

		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			EndDragging();
			base.OnMouseLeftButtonUp(e);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			OnDragging(e);
		}

		private void BeginDragging(MouseButtonEventArgs e)
		{
			if (!m_isDragging)
			{
				CaptureMouse();
				var p = e.GetPosition(this);
				var x = p.X;
				var y = p.Y;
				var w = ActualWidth;
				var h = ActualHeight;
				CoerceMousePoint(ref x, ref y, w, h);
				UpdateIndicatorTransformCore(x, y);
				OnUpdateHsvChannelsFromIndicatorPos(x, y);
				m_isDragging = true;
			}
		}

		private void OnDragging(MouseEventArgs e)
		{
			if (m_isDragging && e.MouseDevice.LeftButton == MouseButtonState.Pressed)
			{
				var p = e.GetPosition(this);
				var x = p.X;
				var y = p.Y;
				var w = ActualWidth;
				var h = ActualHeight;
				CoerceMousePoint(ref x, ref y, w, h);
				UpdateIndicatorTransformCore(x, y);
				OnUpdateHsvChannelsFromIndicatorPos(x, y);
			}
		}

		private void EndDragging()
		{
			if (IsMouseCaptured)
			{
				ReleaseMouseCapture();
			}
			if (m_isDragging)
			{
				m_isDragging = false;
			}
		}

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			base.OnRenderSizeChanged(sizeInfo);
			var context = CurrentContext;
			if (context != null && context.OnContainerChanged(GetChildPositionInfo()))
			{
				UpdateIndicatorTransformCore(m_cacheInfo.ChildPositionX, m_cacheInfo.ChildPositionY);
			}
		}

		private void UpdateIndicatorTransformCore(double x, double y)
		{
			var translate = m_indicator.RenderTransform as TranslateTransform;
			if (translate is null)
			{
				translate = new TranslateTransform();
				m_indicator.RenderTransform = translate;
			}
			translate.X = x;
			translate.Y = y;
		}

	}
}
