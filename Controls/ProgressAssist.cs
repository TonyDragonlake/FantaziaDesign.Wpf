using System;
using System.Windows;
using System.Windows.Media;

namespace FantaziaDesign.Wpf.Controls
{
	//public static class ProgressAssist
	//{
	//	public static double GetMinimumValue(DependencyObject obj)
	//	{
	//		return (double)obj.GetValue(MinimumValueProperty);
	//	}

	//	public static void SetMinimumValue(DependencyObject obj, double value)
	//	{
	//		obj.SetValue(MinimumValueProperty, value);
	//	}

	//	// Using a DependencyProperty as the backing store for MinimunValue.  This enables animation, styling, binding, etc...
	//	public static readonly DependencyProperty MinimumValueProperty =
	//		DependencyProperty.RegisterAttached("MinimunValue", typeof(double), typeof(ProgressAssist), new PropertyMetadata(0.0,OnMinimumValueChanged));

	//	private static void OnMinimumValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	//	{
	//		UpdateCurrentValue(d, (double)e.NewValue, GetMaximumValue(d), GetCurrentPercentage(d));
	//	}

	//	public static double GetMaximumValue(DependencyObject obj)
	//	{
	//		return (double)obj.GetValue(MaximumValueProperty);
	//	}

	//	public static void SetMaximumValue(DependencyObject obj, double value)
	//	{
	//		obj.SetValue(MaximumValueProperty, value);
	//	}

	//	// Using a DependencyProperty as the backing store for MaximunValue.  This enables animation, styling, binding, etc...
	//	public static readonly DependencyProperty MaximumValueProperty =
	//		DependencyProperty.RegisterAttached("MaximunValue", typeof(double), typeof(ProgressAssist), new PropertyMetadata(0.0, OnMaximumValueChanged));

	//	private static void OnMaximumValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	//	{
	//		UpdateCurrentValue(d, GetMinimumValue(d), (double)e.NewValue, GetCurrentPercentage(d));
	//	}

	//	public static double GetRangeOffset(DependencyObject obj)
	//	{
	//		return (double)obj.GetValue(RangeOffsetProperty);
	//	}

	//	public static void SetRangeOffset(DependencyObject obj, double value)
	//	{
	//		obj.SetValue(RangeOffsetProperty, value);
	//	}

	//	// Using a DependencyProperty as the backing store for RangeOffset.  This enables animation, styling, binding, etc...
	//	public static readonly DependencyProperty RangeOffsetProperty =
	//		DependencyProperty.RegisterAttached("RangeOffset", typeof(double), typeof(ProgressAssist), new PropertyMetadata(0.0, OnRangeOffsetChanged));

	//	private static void OnRangeOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	//	{
	//		UpdateCurrentValue(d, GetMinimumValue(d), GetMaximumValue(d), GetCurrentPercentage(d));
	//	}

	//	public static double GetCurrentProgress(DependencyObject obj)
	//	{
	//		return (double)obj.GetValue(CurrentProgressProperty);
	//	}

	//	public static void SetCurrentProgress(DependencyObject obj, double value)
	//	{
	//		obj.SetValue(CurrentProgressProperty, value);
	//	}

	//	// Using a DependencyProperty as the backing store for CurrentProgress.  This enables animation, styling, binding, etc...
	//	public static readonly DependencyProperty CurrentProgressProperty =
	//		DependencyProperty.RegisterAttached("CurrentProgress", typeof(double), typeof(ProgressAssist), new PropertyMetadata(0.0));

	//	public static double GetCurrentPercentage(DependencyObject obj)
	//	{
	//		return (double)obj.GetValue(CurrentPercentageProperty);
	//	}

	//	public static void SetCurrentPercentage(DependencyObject obj, double value)
	//	{
	//		obj.SetValue(CurrentPercentageProperty, value);
	//	}

	//	// Using a DependencyProperty as the backing store for CurrentPercentage.  This enables animation, styling, binding, etc...
	//	public static readonly DependencyProperty CurrentPercentageProperty =
	//		DependencyProperty.RegisterAttached("CurrentPercentage", typeof(double), typeof(ProgressAssist), new PropertyMetadata(0.0,OnCurrentPercentageChanged));

	//	private static void OnCurrentPercentageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	//	{
	//		//if (GetRangeInfoSource(d) == RangeInfoSource.PropertyHost)
	//		//{
	//		//	minValue = GetMinimunValue(d);
	//		//	maxValue = GetMaximunValue(d);
	//		//}
	//		//else
	//		//{
	//		//	minValue = GetMinimunValue(GetTargetVisual(d));
	//		//	maxValue = GetMaximunValue(GetTargetVisual(d));
	//		//}
	//		UpdateCurrentValue(d, GetMinimumValue(d), GetMaximumValue(d), (double)e.NewValue);
	//	}

	//	private static void UpdateCurrentValue(DependencyObject d, double minValue, double maxValue, double percentage)
	//	{
	//		bool isPercentile = minValue == 0 && maxValue == 100;
	//		double current = isPercentile ? percentage : minValue + percentage / 100 * (maxValue - minValue);
	//		double offset = GetRangeOffset(d);
	//		if (offset != 0)
	//		{
	//			current += offset;
	//		}
	//		d.SetValue(CurrentProgressProperty, current);
	//	}
	//}

	public class ProgressElement : ClippedContentControl
	{
		private double _minimum, _maximum, _offset, _percent;

		public double MinimumValue
		{
			get { return (double)GetValue(MinimumValueProperty); }
			set { SetValue(MinimumValueProperty, value); }
		}

		// Using a DependencyProperty as the backing store for MinimumValue.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MinimumValueProperty =
			DependencyProperty.Register("MinimumValue", typeof(double), typeof(ProgressElement), new PropertyMetadata(0.0, OnMinimumValueChanged));

		public double MaximumValue
		{
			get { return (double)GetValue(MaximumValueProperty); }
			set { SetValue(MaximumValueProperty, value); }
		}

		// Using a DependencyProperty as the backing store for MaximumValue.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MaximumValueProperty =
			DependencyProperty.Register("MaximumValue", typeof(double), typeof(ProgressElement), new PropertyMetadata(0.0,OnMaximumValueChanged));

		public double RangeOffset
		{
			get { return (double)GetValue(RangeOffsetProperty); }
			set { SetValue(RangeOffsetProperty, value); }
		}

		// Using a DependencyProperty as the backing store for RangeOffset.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty RangeOffsetProperty =
			DependencyProperty.Register("RangeOffset", typeof(double), typeof(ProgressElement), new PropertyMetadata(0.0,OnRangeOffsetChanged));

		public double CurrentProgress
		{
			get { return (double)GetValue(CurrentProgressProperty); }
			set { SetValue(CurrentProgressProperty, value); }
		}

		// Using a DependencyProperty as the backing store for CurrentProgress.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty CurrentProgressProperty =
			DependencyProperty.Register("CurrentProgress", typeof(double), typeof(ProgressElement), new PropertyMetadata(0.0));

		public double CurrentPercentage
		{
			get { return (double)GetValue(CurrentPercentageProperty); }
			set { SetValue(CurrentPercentageProperty, value); }
		}

		// Using a DependencyProperty as the backing store for CurrentPercentage.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty CurrentPercentageProperty =
			DependencyProperty.Register("CurrentPercentage", typeof(double), typeof(ProgressElement), new PropertyMetadata(0.0, OnCurrentPercentageChanged));

		private static void OnMinimumValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is ProgressElement agent)
			{
				agent._minimum = (double)e.NewValue;
				agent.UpdateCurrentProgress();
			}
		}

		private static void OnMaximumValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is ProgressElement agent)
			{
				agent._maximum = (double)e.NewValue;
				agent.UpdateCurrentProgress();
			}
		}

		private static void OnRangeOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is ProgressElement agent)
			{
				agent._offset = (double)e.NewValue;
				agent.UpdateCurrentProgress();
			}
		}

		private static void OnCurrentPercentageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is ProgressElement agent)
			{
				agent._percent = (double)e.NewValue;
				agent.UpdateCurrentProgress();
			}
		}

		private void UpdateCurrentProgress()
		{
			bool isPercentile = _minimum == 0 && _maximum == 100;
			double current = isPercentile ? _percent : _minimum + _percent / 100 * (_maximum - _minimum);
			//System.Diagnostics.Debug.WriteLine("UpdateCurrentProgress");
			if (_offset != 0)
			{
				current += _offset;
			}
			SetCurrentValue(CurrentProgressProperty, current);
		}
	}
}
