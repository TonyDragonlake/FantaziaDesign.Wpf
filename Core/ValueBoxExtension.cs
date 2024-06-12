using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using FantaziaDesign.Core;
using FantaziaDesign.Events;

namespace FantaziaDesign.Wpf.Core
{
	public interface IValueConverterWrapper : IValueConverter
	{
		IValueConverter ConverterProvider { get; set; }
	}

	public interface IValueConverterWrapper<Tsource, Ttarget> : IValueConverter
	{
		IValueConverter<Tsource, Ttarget> ConverterProvider { get; set; }
	}

	public class ValueConverterWrapper<Tsource, Ttarget> : IValueConverterWrapper<Tsource, Ttarget>
	{
		IValueConverter<Tsource, Ttarget> _converterProvider;

		public ValueConverterWrapper()
		{
		}

		public ValueConverterWrapper(IValueConverter<Tsource, Ttarget> converterProvider)
		{
			_converterProvider = converterProvider;
		}

		public IValueConverter<Tsource, Ttarget> ConverterProvider { get => _converterProvider; set => _converterProvider = value; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			//System.Diagnostics.Debug.WriteLine($"ValueConverterWrapper<{typeof(Tsource).Name},{typeof(Ttarget).Name}>.Convert({value})");
			//System.Diagnostics.Debug.Indent();
			object res;
			if (_converterProvider is null)
			{
				res = DependencyProperty.UnsetValue;
			}
			else
			if (value is Tsource source)
			{
				try
				{
					res = _converterProvider.ConvertTo(source);
				}
				catch (NotImplementedException niex)
				{
					throw niex;
				}
				catch (Exception)
				{
					res = DependencyProperty.UnsetValue;
				}
			}
			else
			{
				res = DependencyProperty.UnsetValue;
			}
			//System.Diagnostics.Debug.WriteLine($"Return = {res}");
			//System.Diagnostics.Debug.Unindent();
			//System.Diagnostics.Debug.WriteLine($"End ValueConverterWrapper<{typeof(Tsource).Name},{typeof(Ttarget).Name}>.Convert");
			return res;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			//System.Diagnostics.Debug.WriteLine($"ValueConverterWrapper<{typeof(Tsource).Name},{typeof(Ttarget).Name}>.ConvertBack({value})");
			//System.Diagnostics.Debug.Indent();
			object res;
			if (_converterProvider is null)
			{
				res = DependencyProperty.UnsetValue;
			}
			else
			if (value is Ttarget target)
			{
				try
				{
					res = _converterProvider.ConvertBack(target);
				}
				catch (NotImplementedException niex)
				{
					throw niex;
				}
				catch (Exception)
				{
					res = DependencyProperty.UnsetValue;
				}
			}
			else
			{
				res = DependencyProperty.UnsetValue;
			}
			//System.Diagnostics.Debug.WriteLine($"Return = {res}");
			//System.Diagnostics.Debug.Unindent();
			//System.Diagnostics.Debug.WriteLine($"End ValueConverterWrapper<{typeof(Tsource).Name},{typeof(Ttarget).Name}>.ConvertBack");
			return res;
		}
	}

	public class ValueConverterWrapper : IValueConverterWrapper
	{
		private IValueConverter _converterProvider;

		public ValueConverterWrapper()
		{
		}

		public ValueConverterWrapper(IValueConverter converterProvider)
		{
			_converterProvider = converterProvider;
		}

		public IValueConverter ConverterProvider { get => _converterProvider; set => _converterProvider = value; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			//System.Diagnostics.Debug.WriteLine($"ValueConverterWrapper.Convert({value})");
			//System.Diagnostics.Debug.Indent();
			object res;
			if (_converterProvider is null)
			{
				res = DependencyProperty.UnsetValue;
			}
			else
			{
				res = _converterProvider.Convert(value, targetType, parameter, culture);
			}
			//System.Diagnostics.Debug.WriteLine($"Return = {res}");
			//System.Diagnostics.Debug.Unindent();
			//System.Diagnostics.Debug.WriteLine($"End ValueConverterWrapper.Convert");
			return res;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			//System.Diagnostics.Debug.WriteLine($"ValueConverterWrapper.ConvertBack({value})");
			//System.Diagnostics.Debug.Indent();
			object res;
			if (_converterProvider is null)
			{
				res = DependencyProperty.UnsetValue;
			}
			else
			{
				res = _converterProvider.ConvertBack(value, targetType, parameter, culture);
			}
			//System.Diagnostics.Debug.WriteLine($"Return = {res}");
			//System.Diagnostics.Debug.Unindent();
			//System.Diagnostics.Debug.WriteLine($"End ValueConverterWrapper.ConvertBack");
			return res;
		}
	}

	public static class ValueBoxExtension
	{
		public static Guid GetSourceIdFromDependencyObject(DependencyObject dependencyObject)
		{
			if (dependencyObject is null)
			{
				return Guid.Empty;
			}
			var id = GetValueSourceId(dependencyObject);
			if (id.HasValue)
			{
				return id.Value;
			}
			else
			{
				var sourceId = Guid.NewGuid();
				SetValueSourceId(dependencyObject, sourceId);
				return sourceId;
			}
		}

		public static void AttachSource(this ValueBoxSource valueSource, DependencyObject dependencyObject)
		{
			valueSource.SourceId = GetSourceIdFromDependencyObject(dependencyObject);
		}

		public static bool DetachSource(this ValueBoxSource valueSource, DependencyObject dependencyObject)
		{
			if (dependencyObject is null)
			{
				return false;
			}
			var id = GetValueSourceId(dependencyObject);
			if (id.HasValue && valueSource.SourceId == id.Value)
			{
				SetValueSourceId(dependencyObject, null);
				valueSource.SourceId = Guid.Empty;
				return true;
			}
			return false;
		}

		public static Guid? GetValueSourceId(DependencyObject obj)
		{
			return (Guid?)obj.GetValue(ValueSourceIdProperty);
		}

		public static void SetValueSourceId(DependencyObject obj, Guid? value)
		{
			obj.SetValue(ValueSourceIdProperty, value);
		}

		// Using a DependencyProperty as the backing store for ValueSourceId.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ValueSourceIdProperty =
			DependencyProperty.RegisterAttached("ValueSourceId", typeof(Guid?), typeof(ValueBoxExtension), new PropertyMetadata(null));


	}

	public class ValueBoxChangedEventManager : WeakEventManager
	{
		private static Type s_typeOfThis = typeof(ValueBoxChangedEventManager);

		private ListenerList<ValueBoxEventArgs> m_listenerList;

		public static ValueBoxChangedEventManager DefaultManager
		{
			get
			{
				var manager = GetCurrentManager(s_typeOfThis) as ValueBoxChangedEventManager;
				if (manager is null)
				{
					manager = new ValueBoxChangedEventManager();
					SetCurrentManager(s_typeOfThis, manager);
				}
				return manager;
			}
		}

		protected static TEventManager GetEventManager<TEventManager>() where TEventManager : ValueBoxChangedEventManager
		{
			var type = typeof(TEventManager);
			if (type == s_typeOfThis)
			{
				return DefaultManager as TEventManager;
			}
			var manager = GetCurrentManager(type) as TEventManager;
			if (manager is null)
			{
				var ctor = type.GetConstructor(ReflectionUtil.AllInstance, null, Type.EmptyTypes, null);
				manager = ctor.Invoke(null) as TEventManager;
				SetCurrentManager(type, manager);
			}
			return manager;
		}

		protected override ListenerList NewListenerList()
		{
			if (m_listenerList is null)
			{
				m_listenerList = new ListenerList<ValueBoxEventArgs>();
			}
			return m_listenerList;
		}

		protected ValueBoxChangedEventManager()
		{
		}

		public void AddListener(IValueBoxChanged eventHost, IWeakEventListener listener)
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

		public void RemoveListener(IValueBoxChanged eventHost, IWeakEventListener listener)
		{
			if (listener is null)
			{
				throw new ArgumentNullException(nameof(listener));
			}
			ProtectedRemoveListener(eventHost, listener);
		}

		public void AddHandler(IValueBoxChanged eventHost, EventHandler<ValueBoxEventArgs> handler)
		{
			if (handler is null)
			{
				throw new ArgumentNullException(nameof(handler));
			}
			ProtectedAddHandler(eventHost, handler);
		}

		public void RemoveHandler(IValueBoxChanged eventHost, EventHandler<ValueBoxEventArgs> handler)
		{
			if (handler is null)
			{
				throw new ArgumentNullException(nameof(handler));
			}
			ProtectedRemoveHandler(eventHost, handler);
		}

		protected override void StartListening(object source)
		{
			var eventHost = source as IValueBoxChanged;
			if (eventHost is null)
			{
				return;
			}
			eventHost.AddHandler(DeliverContextChangedEvent);
		}

		private void DeliverContextChangedEvent(object sender, ValueBoxEventArgs args)
		{
			DeliverEvent(sender, args);
		}

		protected override void StopListening(object source)
		{
			var eventHost = source as IValueBoxChanged;
			if (eventHost is null)
			{
				return;
			}
			eventHost.RemoveHandler(DeliverContextChangedEvent);
		}
	}


}
