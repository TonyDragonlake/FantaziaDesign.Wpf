using FantaziaDesign.Core;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FantaziaDesign.Wpf.Core
{
	public class BooleanToVisualVisibilityConverter : IValueConverter
	{
		public Visibility TrueTarget { get; set; } = Visibility.Visible;

		public Visibility FalseTarget { get; set; } = Visibility.Collapsed;

		public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool boolean)
			{
				return boolean ? TrueTarget : FalseTarget;
			}
			var nullableBoolean = value as bool?;
			return nullableBoolean.GetValueOrDefault() ? TrueTarget : FalseTarget;
		}

		public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is Visibility visibility)
			{
				return visibility == TrueTarget;
			}
			return false;
		}
	}

	public class ScalableValueConverter : IValueConverter
	{
		public double ScaleValue { get; set; } = 1;

		public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var dVal = value as double?;
			if (dVal.HasValue)
			{
				dVal *= ScaleValue;
				return dVal;
			}

			var convertible = value as IConvertible;
			if (convertible != null)
			{
				var val = convertible.ToDouble(culture);
				val *= ScaleValue;
				if (targetType is null)
				{
					targetType = value.GetType();
				}
				return ((IConvertible)val).ToType(targetType, culture);
			}
			return value;
		}

		public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var dVal = value as double?;
			if (dVal.HasValue)
			{
				dVal /= ScaleValue;
				return dVal;
			}

			var convertible = value as IConvertible;
			if (convertible != null)
			{
				var val = convertible.ToDouble(culture);
				val /= ScaleValue;
				if (targetType is null)
				{
					targetType = value.GetType();
				}
				return ((IConvertible)val).ToType(targetType, culture);
			}
			return value;
		}
	}


	public class ColorByteToColorFloatConverter : IValueConverter
	{
		public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is byte val)
			{
				return (double)ColorsUtility.ColorFloat(val);
			}
			return value;
		}

		public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is double val)
			{
				return ColorsUtility.ColorByte((float)val);
			}
			return value;
		}
	}

}
