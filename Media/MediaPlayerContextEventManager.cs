using System;
using System.Windows;
using FantaziaDesign.Events;
using FantaziaDesign.Media;
using FantaziaDesign.Wpf.Core;

namespace FantaziaDesign.Wpf.Media
{
	public class MediaPlayerContextEventManager : ValueBoxChangedEventManager
	{
		// private static Type s_typeOfThis = typeof(MediaPlayerContextEventManager);

		protected override ListenerList NewListenerList()
		{
			return new ListenerList<ValueBoxEventArgs>();
		}

		protected MediaPlayerContextEventManager()
		{
		}

		public static void AddListener(MediaPlayerContext context, IWeakEventListener listener)
		{
			if (context is null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			if (listener is null)
			{
				throw new ArgumentNullException(nameof(listener));
			}
			var manager = GetEventManager<MediaPlayerContextEventManager>();
			manager.ProtectedAddListener(context, listener);
		}

		public static void RemoveListener(MediaPlayerContext context, IWeakEventListener listener)
		{
			if (listener is null)
			{
				throw new ArgumentNullException(nameof(listener));
			}
			var manager = GetEventManager<MediaPlayerContextEventManager>();
			manager.ProtectedRemoveListener(context, listener);
		}

		public static void AddHandler(MediaPlayerContext context, EventHandler<ValueBoxEventArgs> handler)
		{
			if (handler is null)
			{
				throw new ArgumentNullException(nameof(handler));
			}
			var manager = GetEventManager<MediaPlayerContextEventManager>();
			manager.ProtectedAddHandler(context, handler);
		}

		public static void RemoveHandler(MediaPlayerContext context, EventHandler<ValueBoxEventArgs> handler)
		{
			if (handler is null)
			{
				throw new ArgumentNullException(nameof(handler));
			}
			var manager = GetEventManager<MediaPlayerContextEventManager>();
			manager.ProtectedRemoveHandler(context, handler);
		}

		//protected override void StartListening(object source)
		//{
		//	var context = source as MediaPlayerContext;
		//	if (context is null)
		//	{
		//		return;
		//	}
		//	context.AddHandler(DeliverContextChangedEvent);
		//}

		//private void DeliverContextChangedEvent(object sender, ValueBoxEventArgs args)
		//{
		//	DeliverEvent(sender, args);
		//}

		//protected override void StopListening(object source)
		//{
		//	var context = source as MediaPlayerContext;
		//	if (context is null)
		//	{
		//		return;
		//	}
		//	context.RemoveHandler(DeliverContextChangedEvent);
		//}
	}
}
