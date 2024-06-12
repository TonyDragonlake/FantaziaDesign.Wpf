using System;
using System.Windows;
using System.Windows.Media;

namespace FantaziaDesign.Wpf.Controls
{
	public sealed class SizePlaceholder : FrameworkElement
	{
		private UIElement m_target;
		private Size m_sizeCache;
		private bool isFrameworkElement;
		internal bool HasTarget => m_target != null;

		public UIElement UIElementTarget
		{
			get { return (UIElement)GetValue(UIElementTargetProperty); }
			set { SetValue(UIElementTargetProperty, value); }
		}

		// Using a DependencyProperty as the backing store for UIElementTarget.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty UIElementTargetProperty =
			DependencyProperty.Register("UIElementTarget", typeof(FrameworkElement), typeof(SizePlaceholder), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, OnTargetChanged));

		private static void OnTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
		{
			if (d is SizePlaceholder placeholder)
			{
				placeholder.SetOrReplaceTarget(args.NewValue as UIElement);
			}
		}

		internal void SetOrReplaceTarget(UIElement target)
		{
			if (HasTarget)
			{
				DetachLayoutListener();
			}
			if (target is null)
			{
				isFrameworkElement = false;
				m_target = null;
				m_sizeCache = default(Size);
			}
			else
			{
				m_target = target;
				isFrameworkElement = m_target is FrameworkElement;
				AttachLayoutListener();
				m_sizeCache = m_target.RenderSize;
			}
			if (IsMeasureValid)
			{
				InvalidateMeasure();
			}
		}

		private void AttachLayoutListener()
		{
			if (isFrameworkElement)
			{
				var frameworkElement = (FrameworkElement)m_target;
				frameworkElement.SizeChanged += FrameworkElementTargetSizeChanged;
			}
			else
			{
				m_target.LayoutUpdated += UIElementTargetLayoutUpdated;
			}
		}

		private void DetachLayoutListener()
		{
			if (isFrameworkElement)
			{
				var frameworkElement = (FrameworkElement)m_target;
				frameworkElement.SizeChanged -= FrameworkElementTargetSizeChanged;
			}
			else
			{
				m_target.LayoutUpdated -= UIElementTargetLayoutUpdated;
			}
		}

		private void UIElementTargetLayoutUpdated(object sender, EventArgs e)
		{
			m_sizeCache = m_target.RenderSize;
		}

		private void FrameworkElementTargetSizeChanged(object sender, SizeChangedEventArgs e)
		{
			var newSize = e.NewSize;
			if (m_sizeCache != newSize)
			{
				m_sizeCache = newSize;
				if (IsMeasureValid)
				{
					InvalidateMeasure();
				}
			}
		}

		protected override Size MeasureOverride(Size availableSize)
		{
			if (HasTarget)
			{
				return m_sizeCache;
			}
			return base.MeasureOverride(availableSize);
		}
	}
}
