using System;
using System.Globalization;
using System.Collections.Generic;
using System.Windows.Data;
using System.Windows.Media;
using FantaziaDesign.Core;

namespace FantaziaDesign.Wpf.Core
{
	public static class GrayscaleColors
	{
		private static readonly Dictionary<byte, Color> s_grayScaleColors = new Dictionary<byte, Color>(256);

		private static readonly Dictionary<byte, SolidColorBrush> s_grayScaleColorBrushes = new Dictionary<byte, SolidColorBrush>(256);

		public static Color GetGrayscaleColor(byte grayscale)
		{
			if (!s_grayScaleColors.TryGetValue(grayscale, out Color result))
			{
				result = Color.FromRgb(grayscale, grayscale, grayscale);
				s_grayScaleColors.Add(grayscale, result);
			}
			return result;
		}

		public static SolidColorBrush GetGrayscaleColorBrush(byte grayscale)
		{
			if (!s_grayScaleColorBrushes.TryGetValue(grayscale, out SolidColorBrush result))
			{
				result = new SolidColorBrush(Color.FromRgb(grayscale, grayscale, grayscale));
				result.Freeze();
				s_grayScaleColorBrushes.Add(grayscale, result);
			}
			return result;
		}
	}

	public class RgbColorToGrayscaleValueConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			byte grayScale = 0;
			if (value is Color color)
			{
				grayScale = ColorsUtility.ConvertRgbB3ToGrayscaleByte(color.R, color.G, color.B);
			}
			else if (value is SolidColorBrush colorBrush)
			{
				var colorBrushColor = colorBrush.Color;
				grayScale = ColorsUtility.ConvertRgbB3ToGrayscaleByte(colorBrushColor.R, colorBrushColor.G, colorBrushColor.B);
			}
			if (targetType != null)
			{
				if (targetType == typeof(Color))
				{
					return GrayscaleColors.GetGrayscaleColor(grayScale);
				}
				if (targetType.IsAssignableFrom(typeof(SolidColorBrush)))
				{
					return GrayscaleColors.GetGrayscaleColorBrush(grayScale);
				}
				//if (targetType == typeof(CompatibleByte))
				//{
				//	return new CompatibleByte(grayScale);
				//}
				return ((IConvertible)grayScale).ToType(targetType, null);
			}
			return grayScale;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

}
