using System;
using System.Globalization;
using System.ComponentModel;
using FantaziaDesign.Core;
using FantaziaDesign.Media;
using FantaziaDesign.Events;

namespace FantaziaDesign.Wpf.Media
{
	public class MediaPlayerManager : IMediaPlayerManager, IUnique<MediaPlayerManager>
	{
		private TimeSpan _totalTimeSpan;
		private TimeSpan _currentTimeSpan;
		private MediaProgressToTimeTicksConverter _mpTottConverter = new MediaProgressToTimeTicksConverter();

		protected ValueBox<double> mediaProgressCache;
		protected ValueBox<double> volumeProgressCache;
		protected ValueBox mediaPlaybackStateCache;
		protected ValueBox volumeStateCache;

		protected MediaPlayerContext mpContext;
		protected IMediaProgressManager mediaProgressManager;
		protected IMediaProgressManager volumeProgressManager;

		protected IValueConverter<MediaPlaybackState, bool?> mediaStateConverter;
		protected IValueConverter<VolumeState, bool?> volumeStateConverter;
		private Guid _guid;

		public MediaPlayerManager()
		{
			_guid = Guid.NewGuid();
			InitializeComponents();
			InitializeManagementSettings();
		}

		protected virtual void InitializeComponents()
		{
			//mpContext = new MediaPlayerContext(
			//	MediaProperty.MediaStreamId
			//	| MediaProperty.MediaProgress
			//	| MediaProperty.MediaState
			//	| MediaProperty.VolumeState
			//);
			mpContext = new MediaPlayerContext();
			mediaProgressManager = new MediaProgressManager();
			volumeProgressManager = new MediaProgressManager();
			mediaProgressCache = new ValueBox<double>(0);
			volumeProgressCache = new ValueBox<double>(0);
			mediaPlaybackStateCache = new ValueBox(MediaPlaybackState.Stopped);
			volumeStateCache = new ValueBox(VolumeState.Silent);
			mediaStateConverter = new MediaPlaybackStateToBooleanConverter();
			volumeStateConverter = new VolumeStateToBooleanConverter();
		}

		protected virtual void InitializeManagementSettings()
		{
			mediaProgressManager.ProviderId = MediaProperties.MediaProgress;
			volumeProgressManager.ProviderId = MediaProperties.MediaVolume;
			mediaProgressManager.AttachValueContext(mpContext);
			volumeProgressManager.AttachValueContext(mpContext);
			MediaPlayerContextEventManager.AddHandler(mpContext, OnMediaPlayerContextChanged);
			mediaProgressCache.SetTypeCodeAndPropertyCode(ValueBoxSource.InternalSource, ValueBoxTarget.AllTarget, MediaProperties.MediaProgress);
			volumeProgressCache.SetTypeCodeAndPropertyCode(ValueBoxSource.InternalSource, ValueBoxTarget.AllTarget, MediaProperties.MediaVolume);
			mediaPlaybackStateCache.SetTypeCodeAndPropertyCode(ValueBoxSource.InternalSource, ValueBoxTarget.AllTarget, MediaProperties.MediaState);
			volumeStateCache.SetTypeCodeAndPropertyCode(ValueBoxSource.InternalSource, ValueBoxTarget.AllTarget, MediaProperties.VolumeState);
		}

		protected virtual void OnMediaStreamChangedCallback(Guid guid)
		{
			long totalTicks = 0;
			if (guid != Guid.Empty)
			{
				if (!(mpContext.MediaStreamInfo is null))
				{
					var converter = mpContext.MediaStreamInfo.PositionToTimeTicksConverter;
					var totalLength = mpContext.MediaStreamInfo.MediaLength;
					totalTicks = converter.ConvertTo(totalLength);
				}
			}
			SetTotalTimeTicksInternal(totalTicks);
			RaisePropertyChangedEvent(nameof(CurrentMediaName));
		}

		protected virtual void OnMediaProgressChangedCallback(double progress)
		{
			var ticks = _mpTottConverter.ConvertTo(progress);
			SetCurrentTimeTicksInternal(ticks);
			mediaProgressCache.Value = progress;
			mediaProgressCache.TargetTypeCode = ValueBoxTarget.ViewTarget;
			mediaProgressManager.SetValue(mediaProgressCache);
		}

		protected virtual void OnMediaStateChangedCallback(MediaStateChangedReason reason)
		{
			if (reason == MediaStateChangedReason.NormalStopped)
			{
				SetCurrentTimeTicksInternal(_totalTimeSpan.Ticks);
			}
			RaisePropertyChangedEvent(nameof(MediaPlaybackState));
		}

		protected virtual void OnMediaVolumeChangedCallback(double volume)
		{
			RaisePropertyChangedEvent(nameof(MediaVolume));
		}

		protected virtual void OnVolumeStateChangedCallback(VolumeState state)
		{
			RaisePropertyChangedEvent(nameof(VolumeState));
			RaisePropertyChangedEvent(nameof(IsVolumeSilent));
		}

		protected virtual void OnMediaPlayerContextChanged(object sender, ValueBoxEventArgs args)
		{
			var val = args.InnerValue;
			int propId = args.TargetPropertyCode;
			// MediaProperty.MediaStreamId | MediaProperty.MediaProgress | MediaProperty.MediaState | MediaProperty.VolumeState
			switch (propId)
			{
				case MediaProperties.MediaStreamId:
					{
						if (val is Guid guid)
						{
							OnMediaStreamChangedCallback(guid);
						}
					}
					break;
				case MediaProperties.MediaProgress:
					{
						if (val is double progress)
						{
							OnMediaProgressChangedCallback(progress);
						}
					}
					break;
				case MediaProperties.MediaState:
					{
						//if (val is MediaPlaybackState state)
						if (val is MediaPlaybackState)
						{
							var stateArgs = args as MediaPlaybackStateEventArgs;
							if (stateArgs != null)
							{
								OnMediaStateChangedCallback(stateArgs.Reason);
							}
						}
					}
					break;
				case MediaProperties.MediaVolume:
					{
						if (val is double volume)
						{
							OnMediaVolumeChangedCallback(volume);
						}
					}
					break;
				case MediaProperties.VolumeState:
					{
						if (val is VolumeState state)
						{
							OnVolumeStateChangedCallback(state);
						}
					}
					break;
				default:
					break;
			}
		}

		public Guid Guid => _guid;

		public string CurrentMediaName
		{
			get
			{
				if (mpContext is null || mpContext.MediaStreamInfo is null)
				{
					return string.Empty;
				}
				return mpContext.MediaStreamInfo.MediaName;
			}
			set
			{
				if (mpContext is null || mpContext.MediaStreamInfo is null)
				{
					return;
				}
				if (mpContext.MediaStreamInfo.Rename(value))
				{
					RaisePropertyChangedEvent(nameof(CurrentMediaName));
				}
			}
		}

		public TimeSpan TotalTimeSpan
		{
			get => _totalTimeSpan;
		}

		public TimeSpan CurrentTimeSpan
		{
			get => _currentTimeSpan;
		}

		public MediaPlaybackState MediaPlaybackState
		{
			get
			{
				var state = mpContext.MediaState;
				//System.Diagnostics.Debug.WriteLine($"MediaManager : get MediaPlaybackState {state}");
				return state;
			}
			set
			{
				//System.Diagnostics.Debug.WriteLine($"MediaManager : set MediaPlaybackState {value}");

				SetMediaPlaybackState(value);
				//RaisePropertyChangedEvent(nameof(MediaPlaybackState));

				//if (SetMediaPlaybackState(value))
				//{
				//	RaisePropertyChangedEvent(nameof(MediaPlaybackState));
				//}
			}
		}

		public bool SetMediaPlaybackState(MediaPlaybackState value)
		{
			mediaPlaybackStateCache.Value = value;
			return mpContext.SetValue(mediaPlaybackStateCache);
		}

		public IMediaProgressManager MediaProgressManager => mediaProgressManager;

		public double MediaVolume
		{
			get => mpContext.MediaVolume;
		}

		public VolumeState VolumeState
		{
			get
			{
				//var res = mpContext.VolumeState;
				//System.Diagnostics.Debug.WriteLine($"MediaManager : get VolumeState {res}");

				return mpContext.VolumeState;
			}
			//set
			//{
			//	System.Diagnostics.Debug.WriteLine($"MediaManager : set VolumeState {value}");

			//	SetVolumeState(value);
			//}
		}

		public IMediaProgressManager VolumeProgressManager => volumeProgressManager;

		public MediaPlayerContext Context => mpContext;

		public IValueConverter<MediaPlaybackState, bool?> MediaStateConverter => mediaStateConverter;

		public IValueConverter<VolumeState, bool?> VolumeStateConverter => volumeStateConverter;

		public bool IsVolumeSilent
		{
			get => mpContext.IsMediaVolumeSilent;
			set => SetVolumeState(value ? VolumeState.Silent : VolumeState.Middle);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void RaisePropertyChangedEvent(string propName)
		{
			if (string.IsNullOrWhiteSpace(propName))
			{
				throw new ArgumentException($"{nameof(propName)} is null or WhiteSpace", nameof(propName));
			}
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
		}

		public void SetCurrentProgress(double currentProgress)
		{
			if (mediaProgressManager is null)
			{
				return;
			}
			mediaProgressCache.Value = currentProgress;
			mediaProgressCache.TargetTypeCode = ValueBoxTarget.AllTarget;
			if (mediaProgressManager.SetValue(mediaProgressCache))
			{
				var ticks = _mpTottConverter.ConvertTo(currentProgress);
				SetCurrentTimeTicksInternal(ticks);
			}
		}

		public void SetCurrentTimeSpan(TimeSpan currentTimeSpan)
		{
			if (mediaProgressManager is null)
			{
				return;
			}
			if (mediaProgressManager.IsPercentile)
			{
				_mpTottConverter.SetProgressRange(0, 100);
			}
			else
			{
				_mpTottConverter.SetProgressRange(mediaProgressManager.Minimum, mediaProgressManager.Maximum);
			}
			var ticks = currentTimeSpan.Ticks;
			var currentProgress = _mpTottConverter.ConvertBack(ticks);
			mediaProgressCache.Value = currentProgress;
			mediaProgressCache.TargetTypeCode = ValueBoxTarget.AllTarget;
			if (mediaProgressManager.SetValue(mediaProgressCache))
			{
				SetCurrentTimeTicksInternal(ticks);
			}
		}

		protected void SetTotalTimeTicksInternal(long ticks)
		{
			if (TimeUtil.SetTimeTicks(ref _totalTimeSpan, ticks))
			{
				_mpTottConverter.TotalTicks = ticks;
				RaisePropertyChangedEvent(nameof(TotalTimeSpan));
			}
		}

		protected void SetCurrentTimeTicksInternal(long ticks)
		{
			if (TimeUtil.SetTimeTicks(ref _currentTimeSpan, ticks))
			{
				RaisePropertyChangedEvent(nameof(CurrentTimeSpan));
			}
		}

		public void SetMediaVolume(double volumn)
		{
			if (volumeProgressManager is null)
			{
				return;
			}
			volumeProgressCache.Value = volumn;
			//if (volumeProgressManager.SetValue(volumeProgressCache))
			//{
			//	RaisePropertyChangedEvent(nameof(MediaVolume));
			//}
			volumeProgressManager.SetValue(volumeProgressCache);
		}

		protected bool SetVolumeState(VolumeState value)
		{
			volumeStateCache.Value = value;
			return mpContext.SetValue(volumeStateCache);
			//if (mpContext.SetValue(volumeStateCache))
			//{
			//	RaisePropertyChangedEvent(nameof(VolumeState));
			//}
		}

		public bool Equals(MediaPlayerManager other)
		{
			return UniqueObjUtil.Equals(this, other);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as MediaPlayerManager);
		}

		public override int GetHashCode()
		{
			return _guid.GetHashCode();
		}
	}
}
