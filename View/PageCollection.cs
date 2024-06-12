using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace FantaziaDesign.Wpf.View
{
	public class PageBean : FrameworkElementBean<Page>
	{
		public PageBean(Type objectType, bool lazyLoading = true) : base(objectType, lazyLoading)
		{
		}

		public PageBean(Type objectType, bool lazyLoading = true, params object[] ctorParams) : base(objectType, lazyLoading, ctorParams)
		{
		}
	}

	public class PageNavigationEventArgs : EventArgs
	{
		private bool _isPageNavigationSuccess;
		private int _lastPageIndex = -1;
		private int _currentPageIndex = -1;

		public PageNavigationEventArgs(bool isPageNavigationSuccess, int lastPageIndex, int currentPageIndex)
		{
			_isPageNavigationSuccess = isPageNavigationSuccess;
			_lastPageIndex = lastPageIndex;
			_currentPageIndex = currentPageIndex;
		}

		public bool IsPageNavigationSuccess { get => _isPageNavigationSuccess; }
		public int LastPageIndex { get => _lastPageIndex; }
		public int CurrentPageIndex { get => _currentPageIndex; }


	}

	public delegate void PageNavigationEventHandler(object sender, PageNavigationEventArgs e);

	public class PageCollectionNavigator<TNavigationKey> where TNavigationKey : IComparable<TNavigationKey>, IEquatable<TNavigationKey>
	{
		private int _currentPageIndex = -1;
		private object _currentContent = "Content";
		private PageCollection<TNavigationKey> _pageCollection;
		private bool _isCyclingNavigationEnabled;

		public PageCollectionNavigator()
		{

		}

		public PageCollectionNavigator(PageCollection<TNavigationKey> pageCollection)
		{
			_pageCollection = pageCollection ?? throw new ArgumentNullException(nameof(pageCollection));
		}

		public event PageNavigationEventHandler Navigated;

		public int CurrentPageIndex
		{
			get => _currentPageIndex;
			set
			{
				NavigateTo(value);
			}
		}

		public object CurrentContent
		{
			get => _currentContent;
		}
		public bool IsCyclingNavigationEnabled { get => _isCyclingNavigationEnabled; set => _isCyclingNavigationEnabled = value; }

		private void NavigateInternal(int index, Page page)
		{
			var navigationEventArgs = new PageNavigationEventArgs(true, _currentPageIndex, index);
			_currentPageIndex = index;
			_currentContent = page;
			Navigated?.Invoke(this, navigationEventArgs);
		}

		public void NavigateToNext()
		{
			var index = _currentPageIndex + 1;
			if (_pageCollection != null)
			{
				if (_pageCollection.IsInRange(index))
				{
					if (_pageCollection.TryGetPage(index, out Page page))
					{
						NavigateInternal(index, page);
					}
				}
				else if (_isCyclingNavigationEnabled)
				{
					index = 0;
					if (_currentPageIndex != index && _pageCollection.TryGetPage(index, out Page page))
					{
						NavigateInternal(index, page);
					}
				}
			}
		}

		public void NavigateToPrev()
		{
			var index = _currentPageIndex - 1;
			if (_pageCollection != null)
			{
				if (_pageCollection.IsInRange(index))
				{
					if (_pageCollection.TryGetPage(index, out Page page))
					{
						NavigateInternal(index, page);
					}
				}
				else if (_isCyclingNavigationEnabled)
				{
					index = _pageCollection.Count - 1;
					if (_currentPageIndex != index && _pageCollection.TryGetPage(index, out Page page))
					{
						NavigateInternal(index, page);
					}
				}
			}
		}

		public void NavigateTo(int index)
		{
			if (_currentPageIndex != index && _pageCollection != null && _pageCollection.TryGetPage(index, out Page page))
			{
				NavigateInternal(index, page);
			}
		}

		public bool ConnectTo(PageCollection<TNavigationKey> pageCollection)
		{
			if (pageCollection is null)
			{
				return false;
			}
			if (_pageCollection != null)
			{
				Reset();
			}
			_pageCollection = pageCollection;
			return true;
		}

		public void Reset()
		{
			_currentPageIndex = -1;
			_currentContent = "Content";
			_pageCollection = null;
		}
	}

	public interface IPageNavigator<TNavigationKey> where TNavigationKey : IComparable<TNavigationKey>, IEquatable<TNavigationKey>
	{
		PageCollectionNavigator<TNavigationKey> Navigator { get; }
	}

	public class PageCollection<TNavigationKey> : 
		ObservableCollection<TNavigationKey>, ICollection<KeyValuePair<TNavigationKey, Type>>, IPageNavigator<TNavigationKey> 
		where TNavigationKey : IComparable<TNavigationKey>, IEquatable<TNavigationKey>
	{
		private Dictionary<TNavigationKey, PageBean> _pageCollection;
		private PageCollectionNavigator<TNavigationKey> _pageCollectionNavigator;

		public PageCollection()
		{
			_pageCollection = new Dictionary<TNavigationKey, PageBean>();
			_pageCollectionNavigator = new PageCollectionNavigator<TNavigationKey>(this);
		}

		public bool IsReadOnly => false;

		public event EventHandler<Page> OnPageObjectCreated;

		public void Add(TNavigationKey navigationKey, Type pageType)
		{
			if (pageType is null)
			{
				throw new ArgumentNullException(nameof(pageType));
			}

			if (_pageCollection.ContainsKey(navigationKey))
			{
				return;
			}
			_pageCollection.Add(navigationKey, new PageBean(pageType));
			base.Add(navigationKey);
		}

		public void Add(KeyValuePair<TNavigationKey, Type> item)
		{
			Add(item.Key, item.Value);
		}

		public new void Add(TNavigationKey navigationKey)
		{
			throw new NotImplementedException();
		}

		public bool Contains(KeyValuePair<TNavigationKey, Type> item)
		{
			return _pageCollection.ContainsKey(item.Key);
		}

		public new bool Contains(TNavigationKey navigationKey)
		{
			return _pageCollection.ContainsKey(navigationKey);
		}

		public void CopyTo(KeyValuePair<TNavigationKey, Type>[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public bool Remove(KeyValuePair<TNavigationKey, Type> item)
		{
			if (Contains(item.Key))
			{
				Remove(item.Key);
			}
			return false;
		}

		protected override void ClearItems()
		{
			_pageCollection.Clear();
			base.ClearItems();
		}

		protected override void RemoveItem(int index)
		{
			if (index >= 0 && index < Count)
			{
				_pageCollection.Remove(base[index]);
				base.RemoveItem(index);
			}
		}

		protected override void SetItem(int index, TNavigationKey item)
		{
			if (index >= 0 && index < Count)
			{
				var key = base[index];
				if (key.Equals(item) && _pageCollection.TryGetValue(key, out PageBean pageBean))
				{
					_pageCollection.Remove(key);
					_pageCollection.Add(key, pageBean);
					base.SetItem(index, item);
				}
			}
		}

		public bool IsInRange(int index)
		{
			return index >= 0 && index < Count;
		}

		public bool TryGetPage(int index, out Page page)
		{
			page = null;
			if (index >= 0 && index < Count)
			{
				return TryGetPage(base[index], out page);
			}
			return false;
		}


		public bool TryGetPage(TNavigationKey navigationKey, out Page page)
		{
			page = null;
			if (_pageCollection.TryGetValue(navigationKey, out PageBean pageBean))
			{
				var isFirstTime = pageBean.IsTargetDefault;
				page = pageBean.Target;
				if (isFirstTime)
				{
					OnPageObjectCreated?.Invoke(this, page);
				}
				return true;
			}
			return false;
		}

		private IEnumerable<KeyValuePair<TNavigationKey, Type>> EnumerateContent()
		{
			foreach (var item in _pageCollection)
			{
				yield return new KeyValuePair<TNavigationKey, Type>(item.Key, item.Value.TargetType);
			}
		}

		IEnumerator<KeyValuePair<TNavigationKey, Type>> IEnumerable<KeyValuePair<TNavigationKey, Type>>.GetEnumerator()
		{
			return EnumerateContent().GetEnumerator();
		}

		public PageCollectionNavigator<TNavigationKey> Navigator => _pageCollectionNavigator;
	}

}
