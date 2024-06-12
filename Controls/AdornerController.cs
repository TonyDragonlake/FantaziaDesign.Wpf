using FantaziaDesign.Wpf.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace FantaziaDesign.Wpf.Controls
{
	public static class Adornments
	{
		public static AdornerControllerCollection GetAdornerControllers(DependencyObject obj)
		{
			var collection = (AdornerControllerCollection)obj.GetValue(AdornerControllersProperty);
			if (collection is null)
			{
				collection = new AdornerControllerCollection();
				obj.SetValue(AdornerControllersProperty, collection);
			}
			return collection;
		}

		//internal static void SetAdornerControllers(DependencyObject obj, AdornerControllerCollection value)
		//{
		//	obj.SetValue(AdornerControllersProperty, value);
		//}

		// Using a DependencyProperty as the backing store for AdornerControllers.  This enables animation, styling, binding, etc...
		private static readonly DependencyProperty AdornerControllersProperty =
			DependencyProperty.RegisterAttached("ShadowAdornerControllers", typeof(AdornerControllerCollection), typeof(Adornments), new PropertyMetadata(null, OnAdornerControllersChanged));

		private static void OnAdornerControllersChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
		{
			AdornerControllerCollection collection1 = (AdornerControllerCollection)args.OldValue;
			AdornerControllerCollection collection2 = (AdornerControllerCollection)args.NewValue;
			if (collection1 != collection2)
			{
				if (collection1 != null && ((IAttachedObject)collection1).AssociatedObject != null)
				{
					collection1.Detach();
				}
				if (collection2 != null && obj != null)
				{
					if (((IAttachedObject)collection2).AssociatedObject != null)
					{
						throw new InvalidOperationException("Cannot Host AdornerControllerCollection Multiple Times");
					}
					collection2.Attach(obj);
				}
			}
		}
	}

	public abstract class AdornerControllerBase : DependencyObject, IAttachedObject
	{
		private Type m_associatedType;
		private byte m_loadEventState;
		protected bool m_isAdornerActived;
		protected AdornerLayer m_adornerLayer;

		public event EventHandler AssociatedFrameworkElementChanged;

		protected bool LoadedEventListened
		{
			get => (m_loadEventState & 1) == 1;
			set
			{
				byte mask = 1;
				if (value)
				{
					m_loadEventState |= mask;
				}
				else
				{
					m_loadEventState &= (byte)~mask;
				}
			}
		}

		protected bool UnloadedEventListened
		{
			get => (m_loadEventState & 2) == 2;
			set
			{
				byte mask = 2;
				if (value)
				{
					m_loadEventState |= mask;
				}
				else
				{
					m_loadEventState &= (byte)~mask;
				}
			}
		}

		internal AdornerControllerBase(Type associatedType)
		{
			if (!typeof(FrameworkElement).IsAssignableFrom(associatedType))
			{
				throw new ArgumentException("AdornerController.AssociatedType must inherit from FrameworkElement type");
			}
			m_associatedType = associatedType;
		}


		public bool IsAdornerActived
		{
			get { return (bool)GetValue(IsAdornerActivedProperty); }
			set { SetValue(IsAdornerActivedProperty, value); }
		}


		// Using a DependencyProperty as the backing store for IsAdornerActived.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IsAdornerActivedProperty =
			DependencyProperty.Register("IsAdornerActived", typeof(bool), typeof(AdornerControllerBase), new PropertyMetadata(false, OnIsAdornerActivedPropertyChanged));

		public void SetCurrentActivity(bool isActived)
		{
			SetCurrentValue(IsAdornerActivedProperty, isActived);
		}

		private static void OnIsAdornerActivedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is AdornerControllerBase controller)
			{
				controller.SetAdornerActivity((bool)e.NewValue);
			}
		}

		protected void SetAdornerActivity(bool newValue)
		{
			m_isAdornerActived = newValue;
			SetAdornerActivityInternal();
		}

		private void SetAdornerActivityInternal()
		{
			var element = AssociatedFrameworkElement;
			if (element != null && element.IsLoaded)
			{
				if (m_isAdornerActived)
				{
					ActiveAdorner();
				}
				else
				{
					DeactiveAdorner();
				}
			}
		}

		protected abstract void ActiveAdorner();

		protected abstract void DeactiveAdorner(bool removeAction = false);

		protected Type AssociatedType => m_associatedType;

		public abstract FrameworkElement AssociatedFrameworkElement { get;}

		protected abstract void SetAssociatedFrameworkElement(DependencyObject dependencyObject);

		DependencyObject IAttachedObject.AssociatedObject => AssociatedFrameworkElement;

		private void OnAssociatedFrameworkElementChanged()
		{
			AssociatedFrameworkElementChanged?.Invoke(this, new EventArgs());
		}

		public void Attach(DependencyObject dependencyObject)
		{
			if (dependencyObject is null)
			{
				throw new ArgumentNullException(nameof(dependencyObject));
			}
			var currentType = dependencyObject.GetType();
			bool isAssignableFrom = m_associatedType.IsAssignableFrom(currentType);
			if (isAssignableFrom)
			{
				if (dependencyObject != ((IAttachedObject)this).AssociatedObject)
				{
					if (AssociatedFrameworkElement != null)
					{
						throw new InvalidOperationException("Cannot Host AdornerController Multiple Times");
					}
					SetAssociatedFrameworkElement(dependencyObject);
					OnAssociatedFrameworkElementChanged();
					OnAttached();
				}
			}
			else
			{
				throw new InvalidOperationException("TypeConstraintViolated");
			}
		}

		protected virtual void OnAssociatedFrameworkElementLoaded(object sender, RoutedEventArgs e)
		{
			SetAdornerActivityInternal();
		}
		protected virtual void OnAssociatedFrameworkElementUnloaded(object sender, RoutedEventArgs e)
		{
			SetValue(IsAdornerActivedProperty, false);
			DeactiveAdorner(true);
		}

		protected virtual void OnAttached()
		{
			var element = AssociatedFrameworkElement;
			if (element != null)
			{
				if (!LoadedEventListened)
				{
					element.Loaded += OnAssociatedFrameworkElementLoaded;
					LoadedEventListened = true;
				}
				if (!UnloadedEventListened)
				{
					element.Unloaded += OnAssociatedFrameworkElementUnloaded;
					UnloadedEventListened = true;
				}
			}
		}

		public void Detach()
		{
			OnDetaching();
			ClearAssociatedFrameworkElement();
			OnAssociatedFrameworkElementChanged();
		}

		protected virtual void OnDetaching()
		{
			var element = AssociatedFrameworkElement;
			if (element != null)
			{
				if (LoadedEventListened)
				{
					element.Loaded -= OnAssociatedFrameworkElementLoaded;
					LoadedEventListened = false;
				}
				if (UnloadedEventListened)
				{
					element.Unloaded -= OnAssociatedFrameworkElementUnloaded;
					UnloadedEventListened = false;
				}
			}
		}

		protected abstract void ClearAssociatedFrameworkElement();

		internal bool TryGetMatchedAdornerOrNewInternal<TAdorner>(out TAdorner matchedAdorner, Predicate<TAdorner> matchCondition, Func<FrameworkElement, TAdorner> creator = null) where TAdorner : Adorner
		{
			matchedAdorner = null;
			if (matchCondition is null)
			{
				return false;
			}
			var element = AssociatedFrameworkElement;
			if (element != null)
			{
				AdornerLayer layer;
				Adorner[] adornerList;
				if (m_adornerLayer != null)
				{
					layer = m_adornerLayer;
					adornerList = layer.GetAdorners(element);

					if (adornerList is null || adornerList.Length == 0)
					{
						if (creator != null)
						{
							matchedAdorner = creator.Invoke(element);
							layer.Add(matchedAdorner);
							return true;
						}
					}
					else
					{
						var length = adornerList.Length;
						for (int i = 0; i < length; i++)
						{
							var current = adornerList[i] as TAdorner;
							if (current != null && matchCondition.Invoke(current))
							{
								matchedAdorner = current;
								return true;
							}
						}
					}
				}
				else
				{
					layer = AdornerLayer.GetAdornerLayer(element);
					if (layer is null)
					{
						return false;
					}
					adornerList = layer.GetAdorners(element);

					if (adornerList is null || adornerList.Length == 0)
					{
						if (creator != null)
						{
							matchedAdorner = creator.Invoke(element);
							layer.Add(matchedAdorner);
							m_adornerLayer = layer;
							return true;
						}
					}
					else
					{
						var length = adornerList.Length;
						for (int i = 0; i < length; i++)
						{
							var current = adornerList[i] as TAdorner;
							if (current != null && matchCondition.Invoke(current))
							{
								matchedAdorner = current;
								m_adornerLayer = layer;
								return true;
							}
						}
					}
				}

			}
			return false;
		}


		public virtual bool TryGetMatchedAdornerOrNew(out Adorner matchedAdorner, Predicate<Adorner> matchCondition, Func<FrameworkElement,Adorner> creator = null)
		{
			return TryGetMatchedAdornerOrNewInternal(out matchedAdorner, matchCondition, creator);
		}

	}

	public abstract class AdornerController<TFrameworkElement, TAdorner> : AdornerControllerBase where TFrameworkElement : FrameworkElement where TAdorner : Adorner
	{
		private TFrameworkElement m_associatedTarget;

		protected AdornerController() : base(typeof(TFrameworkElement))
		{
		}

		public sealed override FrameworkElement AssociatedFrameworkElement { get => m_associatedTarget;}
		public TFrameworkElement AssociatedTarget => m_associatedTarget;

		public abstract TAdorner CurrentAdorner { get; protected set; }

		protected sealed override void SetAssociatedFrameworkElement(DependencyObject dependencyObject)
		{
			TFrameworkElement element = dependencyObject as TFrameworkElement;
			if (element is null)
			{
				throw new InvalidCastException($"AssociatedFrameworkElement cannot cast type as {typeof(TFrameworkElement)}");
			}
			m_associatedTarget = element;
		}

		protected sealed override void ClearAssociatedFrameworkElement()
		{
			m_associatedTarget = null;
		}

		public virtual bool TryGetMatchedAdornerOrNewOverride(out TAdorner matchedAdorner, Predicate<TAdorner> matchCondition, Func<FrameworkElement, TAdorner> creator = null)
		{
			return TryGetMatchedAdornerOrNewInternal(out matchedAdorner, matchCondition, creator);
		}

		public virtual bool TryGetAllAdorner(out IList<TAdorner> adorners)
		{
			adorners = null;

			var element = AssociatedFrameworkElement;
			if (element != null)
			{
				AdornerLayer layer;
				Adorner[] adornerList;
				if (m_adornerLayer != null)
				{
					layer = m_adornerLayer;
				}
				else
				{
					layer = AdornerLayer.GetAdornerLayer(element);
				}
				if (layer is null)
				{
					return false;
				}
				adornerList = layer.GetAdorners(element);

				if (adornerList is null || adornerList.Length == 0)
				{
					return false;
				}
				else
				{
					var length = adornerList.Length;
					adorners = new List<TAdorner>();
					for (int i = 0; i < length; i++)
					{
						var current = adornerList[i] as TAdorner;
						if (current != null)
						{
							adorners.Add(current);
						}
					}
					if (adorners.Count > 0)
					{
						m_adornerLayer = layer;
						return true;
					}
					adorners = null;
				}
			}
			return false;
		}

	}

	public abstract class AdornerController<TAdorner> : AdornerController<FrameworkElement, TAdorner> where TAdorner : Adorner
	{

	}

	public sealed class AdornerControllerCollection : AttachableCollection<AdornerControllerBase>
	{
		internal AdornerControllerCollection()
		{
		}

		protected override void OnAttached()
		{
			foreach (AdornerControllerBase controllerBase in this)
			{
				controllerBase.Attach(AssociatedObject);
			}
		}

		protected override void OnDetaching()
		{
			foreach (AdornerControllerBase controllerBase in this)
			{
				controllerBase.Detach();
			}
		}

		internal override void ItemAdded(AdornerControllerBase item)
		{
			if (AssociatedObject != null)
			{
				item.Attach(AssociatedObject);
			}
		}

		internal override void ItemRemoved(AdornerControllerBase item)
		{
			if (((IAttachedObject)item).AssociatedObject != null)
			{
				item.Detach();
			}
		}

		protected override Freezable CreateInstanceCore()
		{
			return new AdornerControllerCollection();
		}
	}


	public class FrameworkElementAdorner<TFrameworkElement> : Adorner where TFrameworkElement : FrameworkElement
	{
		protected bool m_canSetChild;
		protected TFrameworkElement m_child;

		public FrameworkElementAdorner(UIElement target, bool canSetChild = false) : base(target)
		{
			m_canSetChild = canSetChild;
			AttachAdornerChild(target);
		}

		protected void AttachAdornerChild(UIElement target)
		{
			OnAttachAdornerChild(target);
			if (m_child is null)
			{
				return;
			}
			AddLogicalChild(m_child);
			AddVisualChild(m_child);
		}

		protected void DettachAdornerChild(UIElement target)
		{
			if (m_child is null)
			{
				return;
			}
			OnDetachAdornerChild(target);
			RemoveVisualChild(m_child);
			RemoveLogicalChild(m_child);
			m_child = null;
		}

		protected virtual void OnAttachAdornerChild(UIElement target)
		{
		}

		protected virtual void OnDetachAdornerChild(UIElement target)
		{
		}

		public TFrameworkElement Child => m_child;

		public bool TrySetChild(TFrameworkElement element)
		{
			if (m_canSetChild && m_child != element)
			{
				var target = AdornedElement;
				DettachAdornerChild(target);
				m_child = element;
				AttachAdornerChild(target);
				if (IsMeasureValid)
				{
					InvalidateMeasure();
				}
				return true;
			}
			return false;
		}

		protected override int VisualChildrenCount
		{
			get
			{
				if (m_child != null)
				{
					return 1;
				}
				return 0;
			}
		}

		protected override Visual GetVisualChild(int index)
		{
			return m_child;
		}

		protected override Size MeasureOverride(Size constraint)
		{
			if (m_child != null)
			{
				m_child.Measure(constraint);
				return m_child.DesiredSize;
			}
			return default(Size);
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			if (m_child != null)
			{
				m_child.Arrange(new Rect(finalSize));
				return finalSize;
			}
			return default(Size);
		}
	}
}

