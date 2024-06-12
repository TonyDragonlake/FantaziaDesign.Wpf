using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using FantaziaDesign.Wpf.Input;

namespace FantaziaDesign.Wpf.Controls
{
	public class SelectableItem : ContentControl, ICommandSourceExecutor<string>
	{
		private static readonly Type s_typeOfThis = typeof(SelectableItem);

		protected CommonCommandSourceCollection m_commandSources = new CommonCommandSourceCollection();

		static SelectableItem()
		{
			DefaultStyleKeyProperty.OverrideMetadata(s_typeOfThis, new FrameworkPropertyMetadata(s_typeOfThis));
			KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(s_typeOfThis, new FrameworkPropertyMetadata(KeyboardNavigationMode.Once));
			KeyboardNavigation.TabNavigationProperty.OverrideMetadata(s_typeOfThis, new FrameworkPropertyMetadata(KeyboardNavigationMode.Local));
		}

		public SelectableItem()
		{
			InitCommandSources();
		}

		protected virtual void InitCommandSources()
		{
			m_commandSources.ParentInputElement = this;
			m_commandSources.RegisterCommandSource(IsSelectedCommandProperty, CommandComponentKind.Command, true);
		}

		private bool m_isSelectable = true;

		public bool IsSelectable
		{
			get { return (bool)GetValue(IsSelectableProperty); }
			set { SetValue(IsSelectableProperty, value); }
		}

		// Using a DependencyProperty as the backing store for IsSelectable.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IsSelectableProperty =
			DependencyProperty.Register("IsSelectable", typeof(bool), s_typeOfThis, new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnIsSelectableChanged)));

		private static void OnIsSelectableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			SelectableItem selectable = d as SelectableItem;
			if (selectable is null)
			{
				return;
			}
			selectable.m_isSelectable = (bool)e.NewValue;
			if (!selectable.m_isSelectable && selectable.IsSelected)
			{
				selectable.SetCurrentValue(IsSelectedProperty, false);
			}
		}

		public bool IsSelected
		{
			get { return (bool)GetValue(IsSelectedProperty); }
			set { SetValue(IsSelectedProperty, value); }
		}

		// Using a DependencyProperty as the backing store for IsSelected.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IsSelectedProperty = 
			Selector.IsSelectedProperty.AddOwner(
				s_typeOfThis, 
				new FrameworkPropertyMetadata(
					false, 
					FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal, 
					new PropertyChangedCallback(OnIsSelectedChanged),
					new CoerceValueCallback(OnCoerceIsSelected)
					)
				);

		private static object OnCoerceIsSelected(DependencyObject d, object baseValue)
		{
			// System.Diagnostics.Debug.WriteLine($"OnCoerceIsSelected : {baseValue}");

			SelectableItem selectable = d as SelectableItem;
			if (!(selectable is null) && !selectable.m_isSelectable)
			{
				return false;
			}
			return baseValue;
		}

		private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			// System.Diagnostics.Debug.WriteLine($"OnIsSelectedChanged : {e.OldValue} -> {e.NewValue}");

			SelectableItem selectable = d as SelectableItem;
			if (selectable is null)
			{
				return;
			}
			selectable.OnIsSelectedChanged((bool)e.NewValue);
		}

		public ICommand IsSelectedCommand
		{
			get { return (ICommand)GetValue(IsSelectedCommandProperty); }
			set { SetValue(IsSelectedCommandProperty, value); }
		}

		// Using a DependencyProperty as the backing store for IsSelectedCommand.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IsSelectedCommandProperty =
			DependencyProperty.Register("IsSelectedCommand", typeof(ICommand), s_typeOfThis, new FrameworkPropertyMetadata(null, OnCommonCommandChanged));

		protected static void OnCommonCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is SelectableItem item)
			{
				item.OnCommonCommandChanged(e.Property, (ICommand)e.NewValue);
			}
		}

		public object IsSelectedCommandParameter
		{
			get { return (object)GetValue(IsSelectedCommandParameterProperty); }
			set { SetValue(IsSelectedCommandParameterProperty, value); }
		}

		// Using a DependencyProperty as the backing store for IsSelectedCommandParameter.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IsSelectedCommandParameterProperty =
			DependencyProperty.Register("IsSelectedCommandParameter", typeof(object), s_typeOfThis, new FrameworkPropertyMetadata(null, OnCommonCommandParameterChanged));

		protected static void OnCommonCommandParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is SelectableItem item)
			{
				item.OnCommonCommandParameterChanged(e.Property, e.NewValue);
			}
		}

		public IInputElement IsSelectedCommandTarget
		{
			get { return (IInputElement)GetValue(IsSelectedCommandTargetProperty); }
			set { SetValue(IsSelectedCommandTargetProperty, value); }
		}

		// Using a DependencyProperty as the backing store for IsSelectedCommandTarget.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IsSelectedCommandTargetProperty =
			DependencyProperty.Register("IsSelectedCommandTarget", typeof(IInputElement), s_typeOfThis, new FrameworkPropertyMetadata(null, OnCommonCommandTargetChanged));

		protected static void OnCommonCommandTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is SelectableItem item)
			{
				item.OnCommonCommandTargetChanged(e.Property, (IInputElement)e.NewValue);
			}
		}

		protected virtual void OnCommonCommandChanged(DependencyProperty dependencyProperty, ICommand newValue, CommandComponentKind componentKind = CommandComponentKind.Command)
		{
			m_commandSources.TrySetCommand(dependencyProperty, newValue, componentKind);
		}

		protected virtual void OnCommonCommandParameterChanged(DependencyProperty dependencyProperty, object newValue, CommandComponentKind componentKind = CommandComponentKind.CommandParameter)
		{
			m_commandSources.TrySetCommandParameter(dependencyProperty, newValue, componentKind);
		}

		protected virtual void OnCommonCommandTargetChanged(DependencyProperty dependencyProperty, IInputElement newValue, CommandComponentKind componentKind = CommandComponentKind.CommandTarget)
		{
			m_commandSources.TrySetCommandTarget(dependencyProperty, newValue, componentKind);
		}

		protected virtual void OnIsSelectedChanged(bool isSelected)
		{
			RaiseSelectiveEvent(this, isSelected);
			UpdateVisualState(true);
		}

		public void UpdateVisualState(bool useTransitions)
		{
			if (!IsEnabled)
			{
				VisualStateManager.GoToState(this, (Content is Control) ? "Normal" : "Disabled", useTransitions);
			}
			else if (IsMouseOver)
			{
				VisualStateManager.GoToState(this, "MouseOver", useTransitions);
			}
			else
			{
				VisualStateManager.GoToState(this, "Normal", useTransitions);
			}
			if (IsSelected)
			{
				if (Selector.GetIsSelectionActive(this))
				{
					VisualStateManager.GoToState(this, "Selected", useTransitions);
				}
				else
				{
					if (!VisualStateManager.GoToState(this, "SelectedUnfocused", useTransitions))
					{
						VisualStateManager.GoToState(this, "Selected", useTransitions);
					}
				}
			}
			else
			{
				VisualStateManager.GoToState(this, "Unselected", useTransitions);
			}
			if (IsKeyboardFocused)
			{
				VisualStateManager.GoToState(this, "Focused", useTransitions);
			}
			else
			{
				VisualStateManager.GoToState(this, "Unfocused", useTransitions);
			}
		}

		protected static void RaiseSelectiveEvent(SelectableItem selectable, bool isSelected)
		{
			selectable.RaiseEvent(isSelected
				? new RoutedEventArgs(Selector.SelectedEvent, selectable)
				: new RoutedEventArgs(Selector.UnselectedEvent, selectable)
				);
			selectable.ExecuteCommandSource("IsSelectedCommandSource");
		}


		public event RoutedEventHandler Selected
		{
			add { AddHandler(SelectedEvent, value); }
			remove { RemoveHandler(SelectedEvent, value); }
		}

		public static readonly RoutedEvent SelectedEvent = Selector.SelectedEvent.AddOwner(s_typeOfThis);

		public event RoutedEventHandler Unselected
		{
			add { AddHandler(UnselectedEvent, value); }
			remove { RemoveHandler(UnselectedEvent, value); }
		}

		public static readonly RoutedEvent UnselectedEvent = Selector.UnselectedEvent.AddOwner(s_typeOfThis);

		public bool ExecuteCommandSource(string commandSourceName, CommandComponentKind componentKind = CommandComponentKind.CommandSource)
		{
			return m_commandSources.ExecuteCommandSource(commandSourceName, componentKind);
		}

		public bool CanExecuteCommandSource(string commandSourceName, CommandComponentKind componentKind = CommandComponentKind.CommandSource)
		{
			return m_commandSources.CanExecuteCommandSource(commandSourceName, componentKind);
		}
	}

}
