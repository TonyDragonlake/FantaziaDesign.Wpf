using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;

namespace FantaziaDesign.Wpf.Core
{
	public class EqualityToVisualVisibilityConverter<TSource> : BooleanToVisualVisibilityConverter
	{
		public TSource EqualitySource { get; set; }

		public TSource DefaultSource { get; set; }

		public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is TSource source)
			{
				var equals = EqualityComparer<TSource>.Default.Equals(source, EqualitySource);
				return equals ? TrueTarget : FalseTarget;
			}
			return FalseTarget;
		}

		public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is Visibility visibility)
			{
				if (visibility == TrueTarget)
				{
					return EqualitySource;
				}
			}
			return DefaultSource;
		}
	}

}
