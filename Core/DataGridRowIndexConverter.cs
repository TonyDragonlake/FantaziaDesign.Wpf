using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace FantaziaDesign.Wpf.Core
{
	public class DataGridRowIndexConverter : IValueConverter
	{
		private int m_startIndex;

		public int StartIndex { get => m_startIndex; set => m_startIndex = value; }

		public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var row = value as DataGridRow;
			if (row is null)
			{
				return null;
			}
			return row.GetIndex() + m_startIndex;
		}

		public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}

	}

}
