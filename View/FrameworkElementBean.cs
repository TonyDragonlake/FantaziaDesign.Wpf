using System;
using System.Windows;
using FantaziaDesign.Core;

namespace FantaziaDesign.Wpf.View
{
	public abstract class FrameworkElementBean<TFrameworkElement> : ObjectBean<TFrameworkElement> where TFrameworkElement : FrameworkElement
	{
		//public event EventHandler<TFrameworkElement> OnFrameworkElementCreated;

		protected FrameworkElementBean(Type objectType, bool lazyLoading = true) : base(objectType, lazyLoading)
		{
		}

		protected FrameworkElementBean(Type objectType, bool lazyLoading = true, params object[] ctorParams) : base(objectType, lazyLoading, ctorParams)
		{
		}

		protected override bool CheckIsTargetDefault()
		{
			return target is null;
		}

		//protected override bool CreateObject()
		//{
		//	if (base.CreateObject())
		//	{
		//		OnFrameworkElementCreated?.Invoke(this, target);
		//		return true;
		//	}
		//	return false;
		//}
	}
}
