using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using FantaziaDesign.Core;

namespace FantaziaDesign.Wpf.Core
{
	public class AnchorItemPositionChangedEventArgs : RoutedEventArgs
	{
		private double m_pOffsetX;
		private double m_pOffsetY;

		public AnchorItemPositionChangedEventArgs(double offsetX, double offsetY) : base(Anchorable.AnchorItemPositionChangedEvent, null)
		{
			m_pOffsetX = offsetX;
			m_pOffsetY = offsetY;
		}

		public AnchorItemPositionChangedEventArgs(Point offset) : this(offset.X, offset.Y)
		{

		}

		public double PositionOffsetX { get => m_pOffsetX; set => m_pOffsetX = value; }
		public double PositionOffsetY { get => m_pOffsetY; set => m_pOffsetY = value; }

		protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
		{
			((AnchorItemPositionChangedEventHandler)genericHandler)(genericTarget, this);
		}
	}

	public delegate void AnchorItemPositionChangedEventHandler(object sender, AnchorItemPositionChangedEventArgs args);

	public static class Anchorable
	{
		internal class AnchorInfo
		{
			private DependencyObject m_anchorHost;
			private HashSet<DependencyObject> m_anchorItems = new HashSet<DependencyObject>();

			public AnchorInfo()
			{
			}

			public AnchorInfo(DependencyObject anchorHost)
			{
				m_anchorHost = anchorHost;
			}

			public DependencyObject AnchorHost { get => m_anchorHost; set => m_anchorHost = value; }
			public HashSet<DependencyObject> AnchorItems { get => m_anchorItems; }
		}

		private static Dictionary<Guid, AnchorInfo> s_anchorInfosTable = new Dictionary<Guid, AnchorInfo>();

		public static ICommand GetBeginAnchorActionCommand(DependencyObject obj)
		{
			return (ICommand)obj.GetValue(BeginAnchorActionCommandProperty);
		}

		public static void SetBeginAnchorActionCommand(DependencyObject obj, ICommand value)
		{
			obj.SetValue(BeginAnchorActionCommandProperty, value);
		}

		// Using a DependencyProperty as the backing store for BeginAnchorActionCommand.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty BeginAnchorActionCommandProperty =
			DependencyProperty.RegisterAttached("BeginAnchorActionCommand", typeof(ICommand), typeof(Anchorable), new FrameworkPropertyMetadata(null));

		public static object GetBeginAnchorActionCommandParameter(DependencyObject obj)
		{
			return obj.GetValue(BeginAnchorActionCommandParameterProperty);
		}

		public static void SetBeginAnchorActionCommandParameter(DependencyObject obj, object value)
		{
			obj.SetValue(BeginAnchorActionCommandParameterProperty, value);
		}

		// Using a DependencyProperty as the backing store for BeginAnchorActionCommandParameter.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty BeginAnchorActionCommandParameterProperty =
			DependencyProperty.RegisterAttached("BeginAnchorActionCommandParameter", typeof(object), typeof(Anchorable), new FrameworkPropertyMetadata(null));

		public static IInputElement GetBeginAnchorActionCommandTarget(DependencyObject obj)
		{
			return (IInputElement)obj.GetValue(BeginAnchorActionCommandTargetProperty);
		}

		public static void SetBeginAnchorActionCommandTarget(DependencyObject obj, IInputElement value)
		{
			obj.SetValue(BeginAnchorActionCommandTargetProperty, value);
		}

		// Using a DependencyProperty as the backing store for BeginAnchorActionCommandTarget.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty BeginAnchorActionCommandTargetProperty =
			DependencyProperty.RegisterAttached("BeginAnchorActionCommandTarget", typeof(IInputElement), typeof(Anchorable), new FrameworkPropertyMetadata(null));

		public static ICommand GetEndAnchorActionCommand(DependencyObject obj)
		{
			return (ICommand)obj.GetValue(EndAnchorActionCommandProperty);
		}

		public static void SetEndAnchorActionCommand(DependencyObject obj, ICommand value)
		{
			obj.SetValue(EndAnchorActionCommandProperty, value);
		}

		// Using a DependencyProperty as the backing store for EndAnchorActionCommand.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty EndAnchorActionCommandProperty =
			DependencyProperty.RegisterAttached("EndAnchorActionCommand", typeof(ICommand), typeof(Anchorable), new FrameworkPropertyMetadata(null));

		public static object GetEndAnchorActionCommandParameter(DependencyObject obj)
		{
			return obj.GetValue(EndAnchorActionCommandParameterProperty);
		}

		public static void SetEndAnchorActionCommandParameter(DependencyObject obj, object value)
		{
			obj.SetValue(EndAnchorActionCommandParameterProperty, value);
		}

		// Using a DependencyProperty as the backing store for EndAnchorActionCommandParameter.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty EndAnchorActionCommandParameterProperty =
			DependencyProperty.RegisterAttached("EndAnchorActionCommandParameter", typeof(object), typeof(Anchorable), new FrameworkPropertyMetadata(null));

		public static IInputElement GetEndAnchorActionCommandTarget(DependencyObject obj)
		{
			return (IInputElement)obj.GetValue(EndAnchorActionCommandTargetProperty);
		}

		public static void SetEndAnchorActionCommandTarget(DependencyObject obj, IInputElement value)
		{
			obj.SetValue(EndAnchorActionCommandTargetProperty, value);
		}

		// Using a DependencyProperty as the backing store for EndAnchorActionCommandTarget.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty EndAnchorActionCommandTargetProperty =
			DependencyProperty.RegisterAttached("EndAnchorActionCommandTarget", typeof(IInputElement), typeof(Anchorable), new FrameworkPropertyMetadata(null));

		public static bool GetIsAnchorItemsHost(DependencyObject obj)
		{
			return (bool)obj.GetValue(IsAnchorItemsHostProperty);
		}

		public static void SetIsAnchorItemsHost(DependencyObject obj, bool value)
		{
			obj.SetValue(IsAnchorItemsHostProperty, value);
		}

		// Using a DependencyProperty as the backing store for IsAnchorItem.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IsAnchorItemsHostProperty =
			DependencyProperty.RegisterAttached("IsAnchorItemsHost", typeof(bool), typeof(Anchorable), new PropertyMetadata(false, OnIsAnchorItemsHostPropertyChanged));

		private static void OnIsAnchorItemsHostPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if ((bool)e.NewValue)
			{
				var id = Guid.NewGuid();
				SetAnchorItemsHostId(d, id);
				RegisterAnchorItemsHostPrivate(id, d);
			}
			else
			{
				var id = GetAnchorItemsHostId(d);
				if (id == Guid.Empty)
				{
					return;
				}
				UnregisterAnchorItemsHostPrivate(id);
				d.ClearValue(AnchorItemsHostIdPropertyKey);
			}
		}

		private static void UnregisterAnchorItemsHostPrivate(Guid guid)
		{
			s_anchorInfosTable.Remove(guid);
		}

		private static void RegisterAnchorItemsHostPrivate(Guid guid, DependencyObject dependencyObject)
		{
			s_anchorInfosTable.Add(guid, new AnchorInfo(dependencyObject));
		}

		public static Guid GetAnchorItemsHostId(DependencyObject obj)
		{
			return (Guid)obj.GetValue(AnchorItemsHostIdPropertyKey.DependencyProperty);
		}

		internal static void SetAnchorItemsHostId(DependencyObject obj, Guid value)
		{
			obj.SetValue(AnchorItemsHostIdPropertyKey, value);
		}

		// Using a DependencyProperty as the backing store for AnchorItemsHostId.  This enables animation, styling, binding, etc...
		public static readonly DependencyPropertyKey AnchorItemsHostIdPropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("AnchorItemsHostId", typeof(Guid), typeof(Anchorable), new PropertyMetadata(Guid.Empty));


		public static Guid GetAnchorParentId(DependencyObject obj)
		{
			return (Guid)obj.GetValue(AnchorParentIdProperty);
		}

		public static void SetAnchorParentId(DependencyObject obj, Guid value)
		{
			obj.SetValue(AnchorParentIdProperty, value);
		}

		// Using a DependencyProperty as the backing store for AnchorParentId.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty AnchorParentIdProperty =
			DependencyProperty.RegisterAttached("AnchorParentId", typeof(Guid), typeof(Anchorable), new PropertyMetadata(Guid.Empty));

		public static string GetItemCategory(DependencyObject obj)
		{
			return (string)obj.GetValue(ItemCategoryProperty);
		}

		public static void SetItemCategory(DependencyObject obj, string value)
		{
			obj.SetValue(ItemCategoryProperty, value);
		}

		// Using a DependencyProperty as the backing store for ItemCategory.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ItemCategoryProperty =
			DependencyProperty.RegisterAttached("ItemCategory", typeof(string), typeof(Anchorable), new PropertyMetadata(string.Empty));

		public static Point GetItemPosition(DependencyObject obj)
		{
			return (Point)obj.GetValue(ItemPositionProperty);
		}

		public static void SetItemPosition(DependencyObject obj, Point value)
		{
			obj.SetValue(ItemPositionProperty, value);
		}

		// Using a DependencyProperty as the backing store for ItemPosition.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ItemPositionProperty =
			// DependencyProperty.RegisterAttached("ItemPosition", typeof(Point), typeof(Anchorable), new FrameworkPropertyMetadata(default(Point), OnItemPositionPropertyChanged));
			DependencyProperty.RegisterAttached("ItemPosition", typeof(Point), typeof(Anchorable), new FrameworkPropertyMetadata(ControlMetrics.NaNPoint, OnItemPositionPropertyChanged));

		private static void OnItemPositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is UIElement ui)
			{
				RaiseAnchorItemPositionChangedEvent(ui, (Point)e.NewValue);
			}
		}

		public static void RaiseAnchorItemPositionChangedEvent(UIElement element, Point offset)
		{
			element?.RaiseEvent(new AnchorItemPositionChangedEventArgs(offset));
		}

		public static void AddAnchorItemPositionChangedEventHandler(UIElement element, AnchorItemPositionChangedEventHandler handler)
		{
			element.AddHandler(AnchorItemPositionChangedEvent, handler);
		}

		public static void RemoveAnchorItemPositionChangedEventHandler(UIElement element, AnchorItemPositionChangedEventHandler handler)
		{
			element.RemoveHandler(AnchorItemPositionChangedEvent, handler);
		}

		public static readonly RoutedEvent AnchorItemPositionChangedEvent =
			EventManager.RegisterRoutedEvent("AnchorItemPositionChanged", RoutingStrategy.Bubble, typeof(RoutedEvent), typeof(Anchorable));

		public static Matrix? GetCoordinateTransform(DependencyObject obj)
		{
			return (Matrix?)obj.GetValue(CoordinateTransformProperty);
		}

		public static void SetCoordinateTransform(DependencyObject obj, Matrix? value)
		{
			obj.SetValue(CoordinateTransformProperty, value);
		}

		public static void SetCurrentCoordinateTransform(DependencyObject obj, Matrix? value)
		{
			obj.SetCurrentValue(CoordinateTransformProperty, value);
		}

		// Using a DependencyProperty as the backing store for CoordinateTransform.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty CoordinateTransformProperty =
			DependencyProperty.RegisterAttached("CoordinateTransform", typeof(Matrix?), typeof(Anchorable), new FrameworkPropertyMetadata(null));

		public static bool TryFindAnchorItemsHost(Visual child, out Visual anchorItemsHost) =>
			VisualTreeExtension.TryFindVisualAncestorIf(child, GetIsAnchorItemsHost, out anchorItemsHost);

		public static bool GetShouldUpdateCoordinateTransform(DependencyObject obj)
		{
			return (bool)obj.GetValue(ShouldUpdateCoordinateTransformProperty);
		}

		public static void SetShouldUpdateCoordinateTransform(DependencyObject obj, bool value)
		{
			obj.SetValue(ShouldUpdateCoordinateTransformProperty, value);
		}

		// Using a DependencyProperty as the backing store for ShouldUpdateCoordinateTransform.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ShouldUpdateCoordinateTransformProperty =
			DependencyProperty.RegisterAttached("ShouldUpdateCoordinateTransform", typeof(bool), typeof(Anchorable), new PropertyMetadata(false));

		public static bool AddAnchorItem(Guid hostId, DependencyObject anchorItem)
		{
			if (hostId == Guid.Empty || anchorItem is null)
			{
				return false;
			}
			if (s_anchorInfosTable.TryGetValue(hostId, out AnchorInfo info))
			{
				return info.AnchorItems.Add(anchorItem);
			}
			return false;
		}

		public static bool RemoveAnchorItem(Guid hostId, DependencyObject anchorItem)
		{
			if (hostId == Guid.Empty || anchorItem is null)
			{
				return false;
			}
			if (s_anchorInfosTable.TryGetValue(hostId, out AnchorInfo info) && info.AnchorItems.Remove(anchorItem))
			{
				SetCurrentCoordinateTransform(anchorItem, null);
				return true;
			}
			return false;
		}

		public static bool TryGetAnchorItemsHost(Guid hostId, out DependencyObject anchorItemsHost)
		{
			if (s_anchorInfosTable.TryGetValue(hostId, out AnchorInfo info))
			{
				anchorItemsHost = info.AnchorHost;
				return true;
			}
			anchorItemsHost = null;
			return false;
		}

		public static bool TryGetAnchorItemPositionRelativeTo(this Visual anchorItem, Visual relativeToVisual, bool allowUpdateTransform, out Point position)
		{
			position = default;
			if (anchorItem is null || relativeToVisual is null)
			{
				return false;
			}
			var point = GetItemPosition(anchorItem);
			if (point.IsNaNPoint())
			{
				return false;
			}
			var mat = GetCoordinateTransform(anchorItem);
			if (mat.HasValue)
			{
				//System.Diagnostics.Debug.WriteLine("TryGetAnchorItemPositionRelativeTo : has value");
				position = mat.Value.Transform(point);
				return true;
			}
			//System.Diagnostics.Debug.WriteLine("TryGetAnchorItemPositionRelativeTo : build value");
			var hostId = GetAnchorParentId(anchorItem);
			if (hostId == Guid.Empty)
			{
				return false;
			}
			if (s_anchorInfosTable.TryGetValue(hostId, out AnchorInfo info))
			{
				var anchorItemsHost = info.AnchorHost as Visual;
				if (anchorItemsHost is null)
				{
					return false;
				}
				var transform = anchorItemsHost.TransformToVisual(relativeToVisual);
				position = transform.Transform(point);
				if (allowUpdateTransform && TransformHelper.TryGetTransformMatrix(transform, out Matrix matrix))
				{
					SetCurrentCoordinateTransform(anchorItem, matrix);
				}
				return true;
			}
			return false;
		}

		public static bool TrySimpleGetAnchorItemPosition(this Visual anchorItem, out Point position)
		{
			position = default;
			if (anchorItem is null)
			{
				return false;
			}
			var point = GetItemPosition(anchorItem);
			if (point.IsNaNPoint())
			{
				return false;
			}
			var mat = GetCoordinateTransform(anchorItem);
			if (mat.HasValue)
			{
				position = mat.Value.Transform(point);
				return true;
			}
			return false;
		}

		public static bool UpdateAnchorItemsCoordinateTransformRelativeTo(this Visual anchorItemsHost, Visual relativeToVisual, Predicate<DependencyObject> filter = null)
		{
			if (anchorItemsHost is null || relativeToVisual is null)
			{
				return false;
			}
			if (GetIsAnchorItemsHost(anchorItemsHost))
			{
				var hostId = GetAnchorItemsHostId(anchorItemsHost);
				if (hostId == Guid.Empty)
				{
					return false;
				}

				if (s_anchorInfosTable.TryGetValue(hostId, out AnchorInfo info))
				{
					var itemsSet = info.AnchorItems;
					if (itemsSet.Count > 0)
					{
						var transform = anchorItemsHost.TransformToVisual(relativeToVisual);
						if (TransformHelper.TryGetTransformMatrix(transform, out Matrix mat))
						{
							if (filter is null)
							{
								foreach (var item in itemsSet)
								{
									SetCurrentCoordinateTransform(item, mat);
								}
							}
							else
							{
								foreach (var item in itemsSet)
								{
									if (filter(item))
									{
										SetCurrentCoordinateTransform(item, mat);
									}
								}
							}
						}
					}
				}
			}
			return false;
		}

	}
}

