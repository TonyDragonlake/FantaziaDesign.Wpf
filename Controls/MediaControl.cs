using System;
using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using FantaziaDesign.Media;
using FantaziaDesign.Wpf.Core;

namespace FantaziaDesign.Wpf.Controls
{
	[TemplatePart(Name = "PART_PlayPauseToggle", Type = typeof(ToggleButton))]
	[TemplatePart(Name = "PART_MediaProgress", Type = typeof(MediaProgressBar))]
	[TemplatePart(Name = "PART_VolumeToggle", Type = typeof(ToggleButton))]
	[TemplatePart(Name = "PART_VolumeProgress", Type = typeof(MediaProgressBar))]
	public class MediaControl : Control
	{
		static MediaControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(MediaControl), new FrameworkPropertyMetadata(typeof(MediaControl)));
			//DataContextProperty.OverrideMetadata(typeof(MediaControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnDataContextChanged)));

			//DataContextProperty.OverrideMetadata(typeof(MediaControl), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnDataContextChanged)));
			EventManager.RegisterClassHandler(typeof(MediaControl), Mouse.MouseDownEvent, new MouseButtonEventHandler(MediaControl.OnClassMouseLeftButtonDown), true);
		}

		private static void OnClassMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton != MouseButton.Left)
			{
				return;
			}
			MediaControl ui = sender as MediaControl;
			if (ui._allowSelfFocus && !ui.IsKeyboardFocusWithin)
			{
				ui.Focus();
				//e.Handled = ui.Focus() || e.Handled;
			}
		}

		ToggleButton _PlayPauseToggle;
		MediaProgressBar _MediaProgress;
		ToggleButton _VolumeToggle;
		MediaProgressBar _VolumeProgress;
		bool _allowSelfFocus;

		protected IMediaPlayerManager mediaPlayerManager;
		protected BitVector32 componentSlots = new BitVector32();

		protected static readonly int mask_PlayPauseToggle = 1;
		protected static readonly int mask_MediaProgress = 2;
		protected static readonly int mask_MediaProgressManager = 4;
		protected static readonly int mask_VolumeToggle = 8;
		protected static readonly int mask_VolumeProgress = 16;
		protected static readonly int mask_VolumeProgressManager = 32;
		protected static readonly int mask_MediaStateConverter = 64;
		protected static readonly int fullComponentsMaskValue = 63;
		protected static readonly int fullMaskValue = 127;

		private IValueConverterWrapper<MediaPlaybackState, bool?> mscTemplateWrapper = new ValueConverterWrapper<MediaPlaybackState, bool?>();
		//private IValueConverterWrapper<VolumeState, bool?> vscTemplateWrapper = new ValueConverterWrapper<VolumeState, bool?>();

		private IValueConverterWrapper _MediaStateConverterWrapper;
		//private IValueConverterWrapper _VolumeStateConverterWrapper;

		public IValueConverter MediaStateConverter { get => _MediaStateConverterWrapper; }

		//public IValueConverter VolumeStateConverter { get => _VolumeStateConverterWrapper;}

		public IMediaPlayerManager MediaPlayerManager => mediaPlayerManager;

		public string CurrentMediaName
		{
			get { return (string)GetValue(CurrentMediaNameProperty); }
			set { SetValue(CurrentMediaNameProperty, value); }
		}

		// Using a DependencyProperty as the backing store for CurrentMediaName.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty CurrentMediaNameProperty =
			DependencyProperty.Register("CurrentMediaName", typeof(string), typeof(MediaControl), new PropertyMetadata(string.Empty));

		public TimeSpan TotalTimeSpan
		{
			get { return (TimeSpan)GetValue(TotalTimeSpanProperty); }
			set { SetValue(TotalTimeSpanProperty, value); }
		}

		// Using a DependencyProperty as the backing store for TotalTimeSpan.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty TotalTimeSpanProperty =
			DependencyProperty.Register("TotalTimeSpan", typeof(TimeSpan), typeof(MediaControl), new PropertyMetadata(default(TimeSpan)));

		public TimeSpan CurrentTimeSpan
		{
			get { return (TimeSpan)GetValue(CurrentTimeSpanProperty); }
			set { SetValue(CurrentTimeSpanProperty, value); }
		}

		// Using a DependencyProperty as the backing store for CurrentTimeSpan.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty CurrentTimeSpanProperty =
			DependencyProperty.Register("CurrentTimeSpan", typeof(TimeSpan), typeof(MediaControl), new PropertyMetadata(default(TimeSpan)));

		public MediaPlaybackState MediaState
		{
			get { return (MediaPlaybackState)GetValue(MediaStateProperty); }
			set { SetValue(MediaStateProperty, value); }
		}

		// Using a DependencyProperty as the backing store for MediaState.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MediaStateProperty =
			DependencyProperty.Register("MediaState", typeof(MediaPlaybackState), typeof(MediaControl), new PropertyMetadata(MediaPlaybackState.Stopped));

		public double MediaVolume
		{
			get { return (double)GetValue(MediaVolumeProperty); }
			set { SetValue(MediaVolumeProperty, value); }
		}

		// Using a DependencyProperty as the backing store for MediaVolume.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MediaVolumeProperty =
			DependencyProperty.Register("MediaVolume", typeof(double), typeof(MediaControl), new PropertyMetadata(0.0));

		public VolumeState VolumeState
		{
			get { return (VolumeState)GetValue(VolumeStateProperty); }
			set { SetValue(VolumeStateProperty, value); }
		}

		// Using a DependencyProperty as the backing store for VolumeState.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty VolumeStateProperty =
			DependencyProperty.Register("VolumeState", typeof(VolumeState), typeof(MediaControl), new PropertyMetadata(VolumeState.Silent));



		public IMediaPlayerManager MediaControlContext
		{
			get { return (IMediaPlayerManager)GetValue(MediaControlContextProperty); }
			set { SetValue(MediaControlContextProperty, value); }
		}

		// Using a DependencyProperty as the backing store for MediaControlContext.  This enables animation, styling, bindtypeof(MediaControl)ing, etc...
		public static readonly DependencyProperty MediaControlContextProperty =
			DependencyProperty.Register("MediaControlContext", typeof(IMediaPlayerManager), typeof(MediaControl), new PropertyMetadata(null, OnMediaControlContextPropertyChanged));

		private static void OnMediaControlContextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is MediaControl mediaControl)
			{
				mediaControl.OnMediaControlContextPropertyChanged(
					e.OldValue as IMediaPlayerManager, 
					e.NewValue as IMediaPlayerManager
					);
			}
		}

		public bool AsUnitedControl
		{
			get { return (bool)GetValue(AsUnitedControlProperty); }
			set { SetValue(AsUnitedControlProperty, value); }
		}

		// Using a DependencyProperty as the backing store for AsUnitedControl.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty AsUnitedControlProperty =
			DependencyProperty.Register("AsUnitedControl", typeof(bool), typeof(MediaControl), new PropertyMetadata(true, OnAsUnitedControlChanged));

		private static void OnAsUnitedControlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			MediaControl mediaControl = d as MediaControl;
			if (mediaControl != null)
			{
				bool? valueWrapper = e.NewValue as bool?;
				if (valueWrapper.HasValue)
				{
					mediaControl.SwitchComponentsFocusableProperty(!valueWrapper.Value);
				}
			}
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			UnbindSwitchStatesToTemplateToggle();
			UnbindComponentsManagerToTemplateProgress();
			if (this.GetTemplateChild($"PART{nameof(_PlayPauseToggle)}", out _PlayPauseToggle))
			{
				componentSlots[mask_PlayPauseToggle] = true;
			}

			if (this.GetTemplateChild($"PART{nameof(_MediaProgress)}", out _MediaProgress))
			{
				componentSlots[mask_MediaProgress] = true;
			}

			if (this.GetTemplateChild($"PART{nameof(_VolumeToggle)}", out _VolumeToggle))
			{
				componentSlots[mask_VolumeToggle] = true;
			}

			if (this.GetTemplateChild($"PART{nameof(_VolumeProgress)}", out _VolumeProgress))
			{
				componentSlots[mask_VolumeProgress] = true;
			}

			SwitchComponentsFocusableProperty(!AsUnitedControl);
			InitConverters();
			BindComponentsManagerToTemplateProgress();
			BindSwitchStatesToTemplateToggle();
		}

		private void InitConverters()
		{
			//if (componentSlots[fullConvertersMaskValue])
			//{
			//	return;
			//}
			var resDict = Template.Resources;
			if (resDict.Count > 0)
			{
				foreach (var item in resDict)
				{
					var entry = item as DictionaryEntry?;
					var key = string.Empty;
					if (entry.HasValue)
					{
						key = entry.Value.Key as string;
					}
					//if (componentSlots[fullConvertersMaskValue])
					//{
					//	break;
					//}
					if (!string.IsNullOrWhiteSpace(key))
					{
						if (!componentSlots[mask_MediaStateConverter])
						{
							if (string.Equals(key, $"PART{nameof(_MediaStateConverterWrapper)}"))
							{
								_MediaStateConverterWrapper = resDict[key] as IValueConverterWrapper;
								componentSlots[mask_MediaStateConverter] = true;
								//continue;
								break;
							}
						}
						//if (!componentSlots[mask_VolumeStateConverter])
						//{
						//	if (string.Equals(key, $"PART{nameof(_VolumeStateConverterWrapper)}"))
						//	{
						//		_VolumeStateConverterWrapper = resDict[key] as IValueConverterWrapper;
						//		componentSlots[mask_VolumeStateConverter] = true;
						//		continue;
						//	}
						//}
					}
				}
			}
			if (!componentSlots[mask_MediaStateConverter])
			{
				_MediaStateConverterWrapper = new ValueConverterWrapper();
				componentSlots[mask_MediaStateConverter] = true;
			}
			//if (!componentSlots[mask_VolumeStateConverter])
			//{
			//	_VolumeStateConverterWrapper = new ValueConverterWrapper();
			//	componentSlots[mask_VolumeStateConverter] = true;
			//}
		}

		//private static void OnDataContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		//{
		//	(d as MediaControl)?.OnDataContextChanged(e);
		//}

		//protected virtual void OnDataContextChanged(DependencyPropertyChangedEventArgs e)
		//{
		//	bool isLastConnected = e.OldValue is IMediaPlayerManager;
		//	bool isCurrentConnected = e.NewValue is IMediaPlayerManager;

		//	if (isLastConnected || isCurrentConnected)
		//	{
		//		BindingManager(e.NewValue as IMediaPlayerManager);
		//	}
		//}

		protected virtual void OnMediaControlContextPropertyChanged(IMediaPlayerManager oldValue, IMediaPlayerManager newValue)
		{
			bool isLastConnected = oldValue is IMediaPlayerManager;
			bool isCurrentConnected = newValue is IMediaPlayerManager;

			if (isLastConnected || isCurrentConnected)
			{
				BindingManager(newValue);
			}
		}

		protected virtual void SwitchComponentsFocusableProperty(bool focusable)
		{
			if (_allowSelfFocus == focusable)
			{
				_PlayPauseToggle?.SetCurrentValue(FocusableProperty, focusable);
				_MediaProgress?.SetCurrentValue(FocusableProperty, focusable);
				_VolumeToggle?.SetCurrentValue(FocusableProperty, focusable);
				_VolumeProgress?.SetCurrentValue(FocusableProperty, focusable);
				_allowSelfFocus = !focusable;
			}
		}

		public void SetMediaStateConverterProvider(IValueConverter value)
		{
			_MediaStateConverterWrapper.ConverterProvider = value;
		}

		//public void SetVolumeStateConverterProvider(IValueConverter value)
		//{
		//	_VolumeStateConverterWrapper.ConverterProvider = value;
		//}

		protected void SetTemplateConvertersProviderFromManagerInternal()
		{
			if (mediaPlayerManager is null)
			{
				mscTemplateWrapper.ConverterProvider = null;
				//vscTemplateWrapper.ConverterProvider = null;
			}
			else
			{
				mscTemplateWrapper.ConverterProvider = mediaPlayerManager.MediaStateConverter;
				//vscTemplateWrapper.ConverterProvider = mediaPlayerManager.VolumeStateConverter;
			}
		}

		protected void WrapTemplateConvertersInternal()
		{
			if (_MediaStateConverterWrapper != null)
			{
				if (_MediaStateConverterWrapper.ConverterProvider is null)
				{
					_MediaStateConverterWrapper.ConverterProvider = mscTemplateWrapper;
				}
			}

			//if (_VolumeStateConverterWrapper != null)
			//{
			//	if (_VolumeStateConverterWrapper?.ConverterProvider is null)
			//	{
			//		_VolumeStateConverterWrapper.ConverterProvider = vscTemplateWrapper;
			//	}
			//}
		}

		public void ForceSetConverterFromManager()
		{
			if (mediaPlayerManager is null)
			{
				_MediaStateConverterWrapper.ConverterProvider = null;
				//_VolumeStateConverterWrapper.ConverterProvider = null;
				return;
			}
			mscTemplateWrapper.ConverterProvider = mediaPlayerManager.MediaStateConverter;
			//vscTemplateWrapper.ConverterProvider = mediaPlayerManager.VolumeStateConverter;
			_MediaStateConverterWrapper.ConverterProvider = mscTemplateWrapper;
			//_VolumeStateConverterWrapper.ConverterProvider = vscTemplateWrapper;
		}

		protected virtual void BindingManager(IMediaPlayerManager manager)
		{
			if (manager is null)
			{

				BindingOperations.ClearBinding(this, CurrentMediaNameProperty);
				BindingOperations.ClearBinding(this, MediaStateProperty);
				BindingOperations.ClearBinding(this, VolumeStateProperty);

				BindingOperations.ClearBinding(this, TotalTimeSpanProperty);
				BindingOperations.ClearBinding(this, CurrentTimeSpanProperty);
				BindingOperations.ClearBinding(this, MediaVolumeProperty);

				mediaPlayerManager = null;
				SetTemplateConvertersProviderFromManagerInternal();
				UnbindComponentsManagerToTemplateProgress();
				UnbindSwitchStatesToTemplateToggle();
			}
			else
			{
				mediaPlayerManager = manager;
				SetTemplateConvertersProviderFromManagerInternal();
				/* `TwoWay` bindingMode
				 * string CurrentMediaName { get; set; }
				 * MediaPlaybackState MediaPlaybackState { get; set; }
				 * VolumeState VolumeState { get; set; }
				 * `OneWay` bindingMode
				 * TimeSpan TotalTimeSpan { get; }
				 * TimeSpan CurrentTimeSpan { get; }
				 * double Volume { get; }
				 */

				BindingOperations.SetBinding(this, CurrentMediaNameProperty, new Binding($"{nameof(IMediaPlayerManager.CurrentMediaName)}") { Mode = BindingMode.TwoWay, Source = mediaPlayerManager });
				BindingOperations.SetBinding(this, MediaStateProperty, new Binding($"{nameof(IMediaPlayerManager.MediaPlaybackState)}") { Mode = BindingMode.TwoWay, Source = mediaPlayerManager });
				BindingOperations.SetBinding(this, VolumeStateProperty, new Binding($"{nameof(IMediaPlayerManager.VolumeState)}") { Mode = BindingMode.OneWay, Source = mediaPlayerManager });

				BindingOperations.SetBinding(this, TotalTimeSpanProperty, new Binding($"{nameof(IMediaPlayerManager.TotalTimeSpan)}") { Mode = BindingMode.OneWay, Source = mediaPlayerManager });
				BindingOperations.SetBinding(this, CurrentTimeSpanProperty, new Binding($"{nameof(IMediaPlayerManager.CurrentTimeSpan)}") { Mode = BindingMode.OneWay, Source = mediaPlayerManager });
				BindingOperations.SetBinding(this, MediaVolumeProperty, new Binding($"{nameof(IMediaPlayerManager.MediaVolume)}") { Mode = BindingMode.OneWay, Source = mediaPlayerManager });

				BindComponentsManagerToTemplateProgress();
				BindSwitchStatesToTemplateToggle();
			}
		}

		protected virtual void BindComponentsManagerToTemplateProgress()
		{
			if (mediaPlayerManager is null)
			{
				return;
			}

			var mpm = mediaPlayerManager.MediaProgressManager;
			if (!componentSlots[mask_MediaProgressManager] && mpm != null && _MediaProgress != null)
			{
				_MediaProgress.SetCurrentValue(DataContextProperty, mpm);
				componentSlots[mask_MediaProgressManager] = true;
			}

			var vpm = mediaPlayerManager.VolumeProgressManager;
			if (!componentSlots[mask_VolumeProgressManager] && vpm != null && _VolumeProgress != null)
			{
				_VolumeProgress.SetCurrentValue(DataContextProperty, vpm);
				componentSlots[mask_VolumeProgressManager] = true;
			}
		}

		protected virtual void UnbindComponentsManagerToTemplateProgress()
		{
			if (componentSlots[mask_MediaProgressManager])
			{
				componentSlots[mask_MediaProgressManager] = false;
				_MediaProgress.ClearValue(DataContextProperty);
			}
			if (componentSlots[mask_VolumeProgressManager])
			{
				componentSlots[mask_VolumeProgressManager] = false;
				_VolumeProgress.ClearValue(DataContextProperty);
			}
		}

		protected virtual void BindSwitchStatesToTemplateToggle()
		{
			if (mediaPlayerManager is null)
			{
				return;
			}

			if (_MediaStateConverterWrapper != null)
			{
				if (_MediaStateConverterWrapper.ConverterProvider is null)
				{
					_MediaStateConverterWrapper.ConverterProvider = mscTemplateWrapper;
				}
				if (_PlayPauseToggle != null)
				{
					BindingOperations.SetBinding(_PlayPauseToggle, ToggleButton.IsCheckedProperty, new Binding($"{nameof(IMediaPlayerManager.MediaPlaybackState)}") { Mode = BindingMode.TwoWay, Source = mediaPlayerManager, Converter = _MediaStateConverterWrapper });
				}
			}
			if (_VolumeToggle != null)
			{
				BindingOperations.SetBinding(_VolumeToggle, ToggleButton.IsCheckedProperty, new Binding($"{nameof(IMediaPlayerManager.IsVolumeSilent)}") { Mode = BindingMode.TwoWay, Source = mediaPlayerManager });
			}

			//if (_VolumeStateConverterWrapper != null)
			//{
			//	if (_VolumeStateConverterWrapper?.ConverterProvider is null)
			//	{
			//		_VolumeStateConverterWrapper.ConverterProvider = vscTemplateWrapper;
			//	}
			//}
		}

		protected virtual void UnbindSwitchStatesToTemplateToggle()
		{
			if (_PlayPauseToggle != null)
			{
				BindingOperations.ClearBinding(_PlayPauseToggle, ToggleButton.IsCheckedProperty);
			}
			if (_VolumeToggle != null)
			{
				BindingOperations.ClearBinding(_VolumeToggle, ToggleButton.IsCheckedProperty);
			}
		}
	}
}
