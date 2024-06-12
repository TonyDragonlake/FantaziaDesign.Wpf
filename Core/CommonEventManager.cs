using System;
using System.Windows;
using FantaziaDesign.Events;

namespace FantaziaDesign.Wpf.Core
{
	public class CommonEventManager : WeakEventManager
	{
		private static Type s_typeOfThis = typeof(CommonEventManager);

		public static CommonEventManager DefaultManager
		{
			get
			{
				var manager = GetCurrentManager(s_typeOfThis) as CommonEventManager;
				if (manager is null)
				{
					manager = new CommonEventManager();
					SetCurrentManager(s_typeOfThis, manager);
				}
				return manager;
			}
		}

		protected CommonEventManager()
		{
		}

		public void AddListener(IEventHost eventHost, IWeakEventListener listener)
		{
			if (eventHost is null)
			{
				throw new ArgumentNullException(nameof(eventHost));
			}

			if (listener is null)
			{
				throw new ArgumentNullException(nameof(listener));
			}

			ProtectedAddListener(eventHost, listener);
		}

		public void RemoveListener(IEventHost eventHost, IWeakEventListener listener)
		{
			if (listener is null)
			{
				throw new ArgumentNullException(nameof(listener));
			}
			ProtectedRemoveListener(eventHost, listener);
		}

		public void AddHandler(IEventHost eventHost, EventHandler handler)
		{
			if (handler is null)
			{
				throw new ArgumentNullException(nameof(handler));
			}
			ProtectedAddHandler(eventHost, handler);
		}

		public void RemoveHandler(IEventHost eventHost, EventHandler handler)
		{
			if (handler is null)
			{
				throw new ArgumentNullException(nameof(handler));
			}
			ProtectedRemoveHandler(eventHost, handler);
		}

		protected override void StartListening(object source)
		{
			var eventHost = source as IEventHost;
			if (eventHost is null)
			{
				return;
			}
			eventHost.AddHandler(DeliverContextChangedEvent);
		}

		private void DeliverContextChangedEvent(object sender, EventArgs args)
		{
			DeliverEvent(sender, args);
		}

		protected override void StopListening(object source)
		{
			var eventHost = source as IEventHost;
			if (eventHost is null)
			{
				return;
			}
			eventHost.RemoveHandler(DeliverContextChangedEvent);
		}
	}
}
