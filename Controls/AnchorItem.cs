using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using FantaziaDesign.Wpf.Core;
using FantaziaDesign.Wpf.Input;

namespace FantaziaDesign.Wpf.Controls
{
	public class AnchorItem : SelectableItem
	{
		private static readonly Type s_typeOfThis = typeof(AnchorItem);
		private Guid m_hostId;

		static AnchorItem()
		{
			DefaultStyleKeyProperty.OverrideMetadata(s_typeOfThis, new FrameworkPropertyMetadata(typeof(AnchorItem)));
		}

		public string ItemCategory
		{
			get { return (string)GetValue(ItemCategoryProperty); }
			set { SetValue(ItemCategoryProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ItemCategory.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ItemCategoryProperty =
			Anchorable.ItemCategoryProperty.AddOwner(s_typeOfThis);

		public Point ItemPosition
		{
			get { return (Point)GetValue(ItemPositionProperty); }
			set { SetValue(ItemPositionProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ItemPosition.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ItemPositionProperty =
			Anchorable.ItemPositionProperty.AddOwner(s_typeOfThis, new FrameworkPropertyMetadata(ControlMetrics.NaNPoint, OnItemPositionPropertyChanged));

		private static void OnItemPositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is SelectableItem item)
			{
				if (item.IsSelected)
				{
					Anchorable.RaiseAnchorItemPositionChangedEvent(item, (Point)e.NewValue);
				}
			}
		}

		public Guid AnchorParentId
		{
			get { return (Guid)GetValue(AnchorParentIdProperty); }
			set { SetValue(AnchorParentIdProperty, value); }
		}

		// Using a DependencyProperty as the backing store for AnchorParentId.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty AnchorParentIdProperty =
			Anchorable.AnchorParentIdProperty.AddOwner(s_typeOfThis);

		public event AnchorItemPositionChangedEventHandler AnchorItemPositionChanged
		{
			add { AddHandler(AnchorItemPositionChangedEvent, value); }
			remove { RemoveHandler(AnchorItemPositionChangedEvent, value); }
		}

		public static readonly RoutedEvent AnchorItemPositionChangedEvent =
			Anchorable.AnchorItemPositionChangedEvent.AddOwner(s_typeOfThis);

		public ICommand BeginAnchorActionCommand
		{
			get { return (ICommand)GetValue(BeginAnchorActionCommandProperty); }
			set { SetValue(BeginAnchorActionCommandProperty, value); }
		}

		// Using a DependencyProperty as the backing store for BeginAnchorActionCommand.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty BeginAnchorActionCommandProperty =
			Anchorable.BeginAnchorActionCommandProperty.AddOwner(s_typeOfThis, new FrameworkPropertyMetadata(null, OnCommonCommandChanged));

		public object BeginAnchorActionCommandParameter
		{
			get { return GetValue(BeginAnchorActionCommandParameterProperty); }
			set { SetValue(BeginAnchorActionCommandParameterProperty, value); }
		}

		// Using a DependencyProperty as the backing store for BeginAnchorActionCommandParameter.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty BeginAnchorActionCommandParameterProperty =
			Anchorable.BeginAnchorActionCommandParameterProperty.AddOwner(s_typeOfThis, new FrameworkPropertyMetadata(null, OnCommonCommandParameterChanged));

		public IInputElement BeginAnchorActionCommandTarget
		{
			get { return (IInputElement)GetValue(BeginAnchorActionCommandTargetProperty); }
			set { SetValue(BeginAnchorActionCommandTargetProperty, value); }
		}

		// Using a DependencyProperty as the backing store for BeginAnchorActionCommandTarget.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty BeginAnchorActionCommandTargetProperty =
			Anchorable.BeginAnchorActionCommandTargetProperty.AddOwner(s_typeOfThis, new FrameworkPropertyMetadata(null, OnCommonCommandTargetChanged));

		public ICommand EndAnchorActionCommand
		{
			get { return (ICommand)GetValue(EndAnchorActionCommandProperty); }
			set { SetValue(EndAnchorActionCommandProperty, value); }
		}

		// Using a DependencyProperty as the backing store for EndAnchorActionCommand.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty EndAnchorActionCommandProperty =
			Anchorable.EndAnchorActionCommandProperty.AddOwner(s_typeOfThis, new FrameworkPropertyMetadata(null, OnCommonCommandChanged));

		public object EndAnchorActionCommandParameter
		{
			get { return GetValue(EndAnchorActionCommandParameterProperty); }
			set { SetValue(EndAnchorActionCommandParameterProperty, value); }
		}

		// Using a DependencyProperty as the backing store for EndAnchorActionCommandParameter.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty EndAnchorActionCommandParameterProperty =
			Anchorable.EndAnchorActionCommandParameterProperty.AddOwner(s_typeOfThis, new FrameworkPropertyMetadata(null, OnCommonCommandParameterChanged));

		public IInputElement EndAnchorActionCommandTarget
		{
			get { return (IInputElement)GetValue(EndAnchorActionCommandTargetProperty); }
			set { SetValue(EndAnchorActionCommandTargetProperty, value); }
		}

		// Using a DependencyProperty as the backing store for EndAnchorActionCommandTarget.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty EndAnchorActionCommandTargetProperty =
			Anchorable.EndAnchorActionCommandTargetProperty.AddOwner(s_typeOfThis, new FrameworkPropertyMetadata(null, OnCommonCommandTargetChanged));

		public AnchorItem()
		{
			Loaded += AnchorItem_Loaded;
			Unloaded += AnchorItem_Unloaded;
		}

		private void AnchorItem_Loaded(object sender, RoutedEventArgs e)
		{
			if (Anchorable.TryFindAnchorItemsHost(this, out Visual anchorItemsHost))
			{
				m_hostId = Anchorable.GetAnchorItemsHostId(anchorItemsHost);
				Anchorable.AddAnchorItem(m_hostId, this);
				SetCurrentValue(AnchorParentIdProperty, m_hostId);
				var centerPoint = new Point(RenderSize.Width / 2, RenderSize.Height / 2);
				UpdateItemPositionIf(m_hostId != Guid.Empty, centerPoint);
				Anchorable.SetShouldUpdateCoordinateTransform(anchorItemsHost, true);
			}
		}

		private void AnchorItem_Unloaded(object sender, RoutedEventArgs e)
		{
			Anchorable.RemoveAnchorItem(m_hostId, this);
			m_hostId = Guid.Empty;
			SetCurrentValue(AnchorParentIdProperty, m_hostId);
		}

		//protected override void OnIsSelectedChanged(bool isSelected)
		//{
		//	base.OnIsSelectedChanged(isSelected);
		//	//var centerPoint = new Point(RenderSize.Width / 2, RenderSize.Height / 2);
		//	//UpdateItemPositionIf(isSelected && m_hostId != Guid.Empty, centerPoint);
		//}

		private void UpdateItemPositionIf(bool canUpdate, Point point = default(Point))
		{
			if (canUpdate && Anchorable.TryGetAnchorItemsHost(m_hostId, out DependencyObject dObj))
			{
				Visual refVisual = dObj as Visual;
				if (refVisual is null)
				{
					return;
				}
				var pos = TransformToVisual(refVisual).Transform(point);
				SetCurrentValue(ItemPositionProperty, pos);
			}
		}

		protected override Size ArrangeOverride(Size arrangeBounds)
		{
			var finalSize = base.ArrangeOverride(arrangeBounds);
			var centerPoint = new Point(finalSize.Width / 2, finalSize.Height / 2);
			//UpdateItemPositionIf(IsSelected && m_hostId != Guid.Empty, centerPoint);
			UpdateItemPositionIf(m_hostId != Guid.Empty, centerPoint);
			return finalSize;
		}

		protected override void InitCommandSources()
		{
			base.InitCommandSources();
			m_commandSources.RegisterCommandSource(BeginAnchorActionCommandProperty,CommandComponentKind.Command, true);
			m_commandSources.RegisterCommandSource(EndAnchorActionCommandProperty, CommandComponentKind.Command, true);
		}

		//protected override void OnIsSelectedChanged(bool isSelected)
		//{
		//	base.OnIsSelectedChanged(isSelected);
		//}

		//protected override void OnMouseEnter(MouseEventArgs e)
		//{
		//	Mouse.Capture(this, CaptureMode.Element);
		//	System.Diagnostics.Debug.WriteLine($"OnMouseEnter");
		//	base.OnMouseEnter(e);
		//}

		//protected override void OnMouseLeave(MouseEventArgs e)
		//{
		//	Mouse.Capture(null);
		//	System.Diagnostics.Debug.WriteLine($"OnMouseLeave");
		//	base.OnMouseLeave(e);

		//}

	}
}
