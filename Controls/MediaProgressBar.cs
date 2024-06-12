using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Data;
using FantaziaDesign.Core;
using FantaziaDesign.Wpf.Core;
using FantaziaDesign.Media;
using Future = FantaziaDesign.Media.Future;

namespace FantaziaDesign.Wpf.Controls
{
	[TemplatePart(Name = "PART_Indicator", Type = typeof(FrameworkElement))]
	[TemplatePart(Name = "PART_BackgroundBar", Type = typeof(FrameworkElement))]
	[TemplatePart(Name = "PART_ForegroundBar", Type = typeof(FrameworkElement))]
	[TemplatePart(Name = "PART_DraggingTranslation", Type = typeof(TranslateTransform))]
	public class MediaProgressBar : RangeBase
	{
		public sealed class BindingsProvider : IBindingOperationsProvider
		{
			internal BindingsProvider()
			{
			}

			public void ClearBindingOperations(DependencyObject targetObject)
			{
				if (targetObject is null)
				{
					return;
				}
				BindingOperations.ClearBinding(targetObject, MinimumProperty);
				BindingOperations.ClearBinding(targetObject, MaximumProperty);
				BindingOperations.ClearBinding(targetObject, LargeChangeProperty);
				BindingOperations.ClearBinding(targetObject, SmallChangeProperty);
				BindingOperations.ClearBinding(targetObject, ValueProperty);
			}

			public void SetBindingOperations(DependencyObject targetObject, object model)
			{
				var manager = model as Future.IMediaProgressManager;
				if (manager is null)
				{
					return;
				}
				/* All IMediaProgressManager props are `OneWay` bindingMode (Model effects view)
				 * 
				 * double Minimum { get; set; }
				 * double Maximum { get; set; }
				 * double LargeInterval { get; set; }
				 * double SmallInterval { get; set; }
				 * double ProgressValue { get; }
				 */
				BindingOperations.SetBinding(targetObject, MinimumProperty, new Binding($"{nameof(Future.IMediaProgressManager.Minimum)}") { Mode = BindingMode.OneWay, Source = manager });
				BindingOperations.SetBinding(targetObject, MaximumProperty, new Binding($"{nameof(Future.IMediaProgressManager.Maximum)}") { Mode = BindingMode.OneWay, Source = manager });
				BindingOperations.SetBinding(targetObject, LargeChangeProperty, new Binding($"{nameof(Future.IMediaProgressManager.LargeInterval)}") { Mode = BindingMode.OneWay, Source = manager });
				BindingOperations.SetBinding(targetObject, SmallChangeProperty, new Binding($"{nameof(Future.IMediaProgressManager.SmallInterval)}") { Mode = BindingMode.OneWay, Source = manager });
				BindingOperations.SetBinding(targetObject, ValueProperty, new Binding($"{nameof(Future.IMediaProgressManager.ProgressValue)}") { Mode = BindingMode.OneWay, Source = manager });

			}
		}

		protected static IBindingOperationsProvider DefaultBindingsProvider => new BindingsProvider();

		static MediaProgressBar()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(MediaProgressBar), new FrameworkPropertyMetadata(typeof(MediaProgressBar)));
			//DataContextProperty.OverrideMetadata(typeof(MediaProgressBar), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnDataContextChanged)));

			DataContextProperty.OverrideMetadata(typeof(MediaProgressBar), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnDataContextChanged)));
			MaximumProperty.OverrideMetadata(typeof(MediaProgressBar), new FrameworkPropertyMetadata(100.0));
			EventManager.RegisterClassHandler(typeof(MediaProgressBar), Mouse.MouseDownEvent, new MouseButtonEventHandler(MediaProgressBar.OnClassMouseLeftButtonDown), true);
		}

		#region Fields

		private bool _isFrameLocked;
		//private bool _isOnApplyInitialValue;
		protected Rect m_backRect;
		protected Size m_indicatorSize;
		protected Orientation m_orientation;
		protected Future.IMediaProgressManager m_progressManager;
		protected Future.IMediaProgressEntry<double> m_progressEntry;

		protected FrameworkElement m_indicator;
		protected FrameworkElement m_backgroundBar;
		protected FrameworkElement m_foregroundBar;
		protected TranslateTransform m_draggingTranslation;

		protected double m_modelValue;
		protected double m_viewValue;
		protected IRangeValueConverter<double> m_modelValueConverter = new DoubleRangeValueConverter() { MaxValue = 100 };
		protected IRangeValueConverter<double> m_viewValueConverter = new DoubleRangeValueConverter() { MaxValue = 100 };
		protected IBindingOperationsProvider m_bindingsProvider = DefaultBindingsProvider;

		#endregion

		#region Props

		public double MinValue { get => m_modelValueConverter.MinValue; }
		public double MaxValue { get => m_modelValueConverter.MaxValue; }
		public Future.IMediaProgressManager MediaProgressManager { get => m_progressManager; private set => m_progressManager = value; }

		#endregion

		#region PropDps

		public bool IsDragging
		{
			get { return (bool)GetValue(IsDraggingProperty); }
			set { SetValue(IsDraggingProperty, value); }
		}

		// Using a DependencyProperty as the backing store for IsDragging.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IsDraggingProperty =
			DependencyProperty.Register("IsDragging", typeof(bool), typeof(MediaProgressBar), new PropertyMetadata(false));

		public Brush UnprocessedBrush
		{
			get { return (Brush)GetValue(UnprocessedBrushProperty); }
			set { SetValue(UnprocessedBrushProperty, value); }
		}

		// Using a DependencyProperty as the backing store for UnprocessedBrush.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty UnprocessedBrushProperty =
			DependencyProperty.Register("UnprocessedBrush", typeof(Brush), typeof(MediaProgressBar), new FrameworkPropertyMetadata(Brushes.LightGray, FrameworkPropertyMetadataOptions.AffectsRender));

		public Brush ProcessedBrush
		{
			get { return (Brush)GetValue(ProcessedBrushProperty); }
			set { SetValue(ProcessedBrushProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ProcessedBrush.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ProcessedBrushProperty =
			DependencyProperty.Register("ProcessedBrush", typeof(Brush), typeof(MediaProgressBar), new FrameworkPropertyMetadata(Brushes.SkyBlue, FrameworkPropertyMetadataOptions.AffectsRender));

		public Orientation Orientation
		{
			get { return (Orientation)GetValue(OrientationProperty); }
			set { SetValue(OrientationProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Orientation.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty OrientationProperty =
			DependencyProperty.Register("Orientation", typeof(Orientation), typeof(MediaProgressBar), new FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsArrange, new PropertyChangedCallback(MediaProgressBar.OnOrientationChanged)));

		public IBindingOperationsProvider BindingOperationsProvider
		{
			get { return (IBindingOperationsProvider)GetValue(BindingOperationsProviderProperty); }
			set { SetValue(BindingOperationsProviderProperty, value); }
		}

		// Using a DependencyProperty as the backing store for MediaProgressBindingsProvider.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty BindingOperationsProviderProperty =
			Bindable.BindingOperationsProviderProperty.AddOwner(typeof(MediaProgressBar), new FrameworkPropertyMetadata(DefaultBindingsProvider, new PropertyChangedCallback(MediaProgressBar.OnMediaProgressBindingsProviderChanged)));

		#endregion

		#region Events

		public event EventHandler DraggingCompleted
		{
			add { AddHandler(DraggingCompletedEvent, value); }
			remove { RemoveHandler(DraggingCompletedEvent, value); }
		}

		public static readonly RoutedEvent DraggingCompletedEvent =
			EventManager.RegisterRoutedEvent("DraggingCompleted", RoutingStrategy.Bubble, typeof(EventHandler), typeof(MediaProgressBar));

		#endregion

		#region PropDpCallbacks

		private static void OnClassMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton != MouseButton.Left)
			{
				return;
			}
			MediaProgressBar ui = sender as MediaProgressBar;
			if (!ui.IsKeyboardFocusWithin)
			{
				ui.Focus();
				//e.Handled = ui.Focus() || e.Handled;
			}
		}

		private static void OnDataContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var mediaProgressBar = d as MediaProgressBar;
			if (mediaProgressBar != null)
			{
				mediaProgressBar.OnDataContextChanged(e);
			}
		}

		protected virtual void OnDataContextChanged(DependencyPropertyChangedEventArgs e)
		{
			bool isLastConnected = e.OldValue is Future.IMediaProgressManager;
			bool isCurrentConnected = e.NewValue is Future.IMediaProgressManager;

			if (isLastConnected || isCurrentConnected)
			{
				BindingManager(e.NewValue as Future.IMediaProgressManager);
			}
		}

		private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is MediaProgressBar mediaProgressBar)
			{
				mediaProgressBar.OnOrientationChanged();
			}
		}

		protected virtual void OnOrientationChanged()
		{
		}

		private static void OnMediaProgressBindingsProviderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is MediaProgressBar mediaProgressBar)
			{
				var oldValue = e.OldValue as IBindingOperationsProvider;
				var newValue = e.NewValue as IBindingOperationsProvider;
				if (newValue is null)
				{
					if (oldValue != null)
					{
						oldValue.ClearBindingOperations(mediaProgressBar);
					}
					mediaProgressBar.m_bindingsProvider = null;
				}
				else
				{
					mediaProgressBar.m_bindingsProvider = newValue;
				}
				mediaProgressBar.BindingManager(mediaProgressBar.m_progressManager);
			}
		}


		#endregion

		#region Methods

		private void BeginDragging(MouseButtonEventArgs e)
		{
			if (!IsDragging)
			{
				bool isOutsideIndicator = m_indicator != null && !m_indicator.IsMouseOver;
				base.CaptureMouse();
				IsDragging = true;
				m_progressEntry?.EnterExclusiveMode();
				if (isOutsideIndicator)
				{
					var point = e.GetPosition(this);
					SetDisplayValueFromPoint(ref point);
				}
			}
			else
			{
				base.ReleaseMouseCapture();
				IsDragging = false;
				m_progressEntry?.ExitExclusiveMode();
			}
		}

		private void OnDraggingBehavior(MouseEventArgs e)
		{
			if (IsDragging)
			{
				var point = e.GetPosition(this);
				SetDisplayValueFromPoint(ref point);
			}
			else
			{
				EndDragging(e);
			}
		}

		private void EndDragging(MouseEventArgs e)
		{
			if (IsMouseCaptured && IsDragging)
			{
				m_progressEntry?.ExitExclusiveMode();
				var point = e.GetPosition(this);
				SetDisplayValueFromPoint(ref point);
				m_progressEntry?.SynchronizeToContext();
				IsDragging = false;
				//this.TrySetUIState(DraggingState, 0);
				ReleaseMouseCapture();
				RaiseEvent(new RoutedEventArgs(MediaProgressBar.DraggingCompletedEvent, this));
			}
		}

		private void ApplyDisplayFromValue(double value)
		{
			if (UpdateViewRange())
			{
				m_modelValueConverter.AdaptValueInRange(ref value);
				m_modelValue = value;
				var percentage = m_modelValueConverter.ConvertTo(m_modelValue);
				m_viewValue = m_viewValueConverter.ConvertBack(percentage);
				UpdateFrame();
			}
		}

		private void SetDisplayValueFromPoint(ref Point point)
		{
			if (UpdateViewRange())
			{
				UpdateViewRangeValueFromPoint(ref point);
				var percentage = m_viewValueConverter.ConvertTo(m_viewValue);
				m_modelValue = m_modelValueConverter.ConvertBack(percentage);
				m_progressEntry?.SetProgressValue(m_modelValue);
			}
		}

		public void LockFrame()
		{
			_isFrameLocked = true;
		}

		public void UnlockFrame()
		{
			_isFrameLocked = false;
		}

		public void UpdateFrame()
		{
			if (!_isFrameLocked)
			{
				ApplyDisplayValueToView();
			}
		}

		protected virtual void UpdateViewRangeValueFromPoint(ref Point point)
		{
			var isHorizonal = Orientation == Orientation.Horizontal;
			m_viewValue = isHorizonal ? point.X - (m_indicatorSize.Width / 2) : point.Y - (m_indicatorSize.Height / 2);
		}

		protected virtual bool UpdateViewRange()
		{
			var isHorizonal = Orientation == Orientation.Horizontal;
			m_viewValueConverter.SetRange(0, isHorizonal ? m_backRect.Width : m_backRect.Height);
			return m_viewValueConverter.RangeValue > 0;
		}

		protected virtual void ApplyDisplayValueToView()
		{
			if (m_draggingTranslation != null && m_foregroundBar != null)
			{
				var loc = m_viewValue;
				if (m_orientation == Orientation.Horizontal)
				{
					m_draggingTranslation.X = loc;
					m_foregroundBar.Width = loc;
				}
				else
				{
					m_draggingTranslation.Y = loc;
					m_foregroundBar.Height = loc;
				}
			}
		}

		protected virtual void BindingManager(Future.IMediaProgressManager manager)
		{
			if (m_bindingsProvider is null)
			{
				return;
			}
			LockFrame();
			if (m_progressEntry != null)
			{
				m_progressEntry.Dispose();
				m_progressManager = null;
			}
			if (manager is null)
			{
				m_bindingsProvider.ClearBindingOperations(this);
			}
			else
			{
				m_progressManager = manager;
				m_progressEntry = m_progressManager.GetExclusiveProgressEntry();
				m_bindingsProvider.SetBindingOperations(this, m_progressManager);
			}
			UnlockFrame();
			UpdateFrame();
		}

		#endregion

		#region OverrideMethods

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			if (this.GetTemplateChild("PART_Indicator", out m_indicator))
			{
				m_indicatorSize = m_indicator.RenderSize;
			}

			if (this.GetTemplateChild("PART_BackgroundBar", out m_backgroundBar))
			{
				m_backRect = m_backgroundBar.GetRelativeRenderRect(this);
			}

			this.GetTemplateChild("PART_ForegroundBar", out m_foregroundBar);
			this.GetTemplateChild("PART_DraggingTranslation", out m_draggingTranslation);
			m_orientation = Orientation;
			m_modelValueConverter.SetRange(Minimum, Maximum);
			//_isOnApplyInitialValue = true;
		}

		protected override Size ArrangeOverride(Size arrangeBounds)
		{
			var size = base.ArrangeOverride(arrangeBounds);

			var rect = m_backgroundBar.GetRelativeRenderRect(this);
			var asize = m_indicator.GetActualSize();

			var backRectChanged = m_backRect != rect;
			var indicatorSizeChanged = m_indicatorSize != asize;
			var isOrientationChanged = m_orientation != Orientation;

			if (backRectChanged)
			{
				m_backRect = rect;
			}

			if (indicatorSizeChanged)
			{
				m_indicatorSize = asize;
			}

			if (isOrientationChanged)
			{
				m_orientation = Orientation;
			}

			if (backRectChanged || indicatorSizeChanged || isOrientationChanged)
			{
				ApplyDisplayFromValue(m_modelValue);
			}
			return size;
		}

		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			BeginDragging(e);
			base.OnMouseLeftButtonDown(e);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			OnDraggingBehavior(e);
		}

		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			EndDragging(e);
			base.OnMouseLeftButtonUp(e);
		}

		protected override void OnMinimumChanged(double oldMinimum, double newMinimum)
		{
			m_modelValueConverter.TrySetMinValue(newMinimum);
			ApplyDisplayFromValue(m_modelValue);
			base.OnMinimumChanged(oldMinimum, newMinimum);
		}

		protected override void OnMaximumChanged(double oldMaximum, double newMaximum)
		{
			m_modelValueConverter.TrySetMaxValue(newMaximum);
			ApplyDisplayFromValue(m_modelValue);
			base.OnMaximumChanged(oldMaximum, newMaximum);
		}

		protected override void OnValueChanged(double oldValue, double newValue)
		{
			ApplyDisplayFromValue(newValue);
			base.OnValueChanged(oldValue, newValue);
		}

		#endregion
	}
}
