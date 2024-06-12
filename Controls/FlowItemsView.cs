using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using FantaziaDesign.Core;
using FantaziaDesign.Wpf.Core;
using FantaziaDesign.Wpf.Input;

namespace FantaziaDesign.Wpf.Controls
{
	public interface IDrawingConnectorContext
	{
		//Visual ReferenceVisual { get; set; }

		bool IsConnectorDrawingValid { get; }

		void PushStartPoint(double startPointX, double startPointY);

		void PushEndPoint(double endPointX, double endPointY);

		void PeekStartPoint(out double startPointX, out double startPointY);

		void PeekEndPoint(out double endPointX, out double endPointY);

		void PopConnectorPoints(out double startPointX, out double startPointY, out double endPointX, out double endPointY);

		void CancelConnectorDrawing();

	}

	internal sealed class DefaultDrawingConnectorContext : IDrawingConnectorContext
	{
		private Visual m_refVisual;
		private double[] m_points = new double[4];
		private IDrawingConnectorContext m_context;

		public bool IsConnectorDrawingValid
		{
			get
			{
				if (m_context is null)
				{
					for (int i = 0; i < 4; i++)
					{
						if (double.IsNaN(m_points[i]))
						{
							return false;
						}
					}
					return true;
				}
				return m_context.IsConnectorDrawingValid;
			}
		}

		public void CancelConnectorDrawing()
		{
			if (m_context is null)
			{
				for (int i = 0; i < 4; i++)
				{
					m_points[i] = double.NaN;
				}
			}
			else
			{
				m_context.CancelConnectorDrawing();
			}
		}

		public void PushStartPoint(double startPointX, double startPointY)
		{
			if (m_context is null)
			{
				m_points[0] = startPointX;
				m_points[1] = startPointY;
			}
			else
			{
				m_context.PushStartPoint(startPointX, startPointY);
			}
		}

		public void PushEndPoint(double endPointX, double endPointY)
		{
			if (m_context is null)
			{
				m_points[2] = endPointX;
				m_points[3] = endPointY;
			}
			else
			{
				m_context.PushEndPoint(endPointX, endPointY);
			}
		}

		public void PeekStartPoint(out double startPointX, out double startPointY)
		{
			if (m_context is null)
			{
				startPointX = m_points[0];
				startPointY = m_points[1];
			}
			else
			{
				m_context.PeekStartPoint(out startPointX, out startPointY);
			}
		}

		public void PeekEndPoint(out double endPointX, out double endPointY)
		{
			if (m_context is null)
			{
				endPointX = m_points[2];
				endPointY = m_points[3];
			}
			else
			{
				m_context.PeekEndPoint(out endPointX, out endPointY);
			}
		}

		public void PopConnectorPoints(out double startPointX, out double startPointY, out double endPointX, out double endPointY)
		{
			if (m_context is null)
			{
				startPointX = m_points[0];
				startPointY = m_points[1];
				endPointX = m_points[2];
				endPointY = m_points[3];
				CancelConnectorDrawing();
			}
			else
			{
				m_context.PopConnectorPoints(out startPointX, out startPointY, out endPointX, out endPointY);
			}
		}

		internal void SetContext(IDrawingConnectorContext drawingContext)
		{
			if (drawingContext is null)
			{
				if (m_context is null)
				{
					return;
				}

				var from = m_context;
				m_context = null;
				CopyData(from, this);
			}
			else
			{
				if (m_context != null)
				{
					if (m_context != drawingContext)
					{
						CopyData(m_context, drawingContext);
					}
				}
				else
				{
					CopyData(this, drawingContext);
				}
				m_context = drawingContext;
			}
		}

		private static void CopyData(IDrawingConnectorContext from, IDrawingConnectorContext to)
		{
			if (from.IsConnectorDrawingValid)
			{
				from.PopConnectorPoints(out double startPointX, out double startPointY, out double endPointX, out double endPointY);
				to.PushStartPoint(startPointX, startPointY);
				to.PushEndPoint(endPointX, endPointY);
			}
			else
			{
				to.CancelConnectorDrawing();
			}
			//to.ReferenceVisual = from.ReferenceVisual;
		}
	}

	[StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(FlowItem))]
	public class FlowItemsView : ListBox
	{
		static FlowItemsView()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(FlowItemsView), new FrameworkPropertyMetadata(typeof(FlowItemsView)));
			Control.IsTabStopProperty.OverrideMetadata(typeof(FlowItemsView), new FrameworkPropertyMetadata(false));
			KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(FlowItemsView), new FrameworkPropertyMetadata(KeyboardNavigationMode.Contained));
			KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(FlowItemsView), new FrameworkPropertyMetadata(KeyboardNavigationMode.Once));
			//ItemsPanelTemplate itemsPanelTemplate = new ItemsPanelTemplate(new FrameworkElementFactory(typeof(Canvas)));
			ItemsPanelTemplate itemsPanelTemplate = new ItemsPanelTemplate(new FrameworkElementFactory(typeof(InfiniteCanvas)));
			itemsPanelTemplate.Seal();
			ItemsControl.ItemsPanelProperty.OverrideMetadata(typeof(FlowItemsView), new FrameworkPropertyMetadata(itemsPanelTemplate));
			CommandHelper.RegisterCommandHandler(typeof(FlowItemsView), ItemsControlHelper.RoutedAddItemCommand, new ExecutedRoutedEventHandler(OnRoutedAddItemCommand), new CanExecuteRoutedEventHandler(CanExecuteRoutedAddItemCommand));
			CommandHelper.RegisterCommandHandler(typeof(FlowItemsView), ItemsControlHelper.RoutedClearItemsCommand, new ExecutedRoutedEventHandler(OnRoutedClearItemsCommand), new CanExecuteRoutedEventHandler(CanExecuteRoutedClearItemsCommand));
		}

		private static void CanExecuteRoutedClearItemsCommand(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = ((sender as FlowItemsView)?.CanExecuteRoutedClearItemsCommand()).GetValueOrDefault();
			e.Handled = true;
		}

		private bool CanExecuteRoutedClearItemsCommand()
		{
			return CommandSources.CanExecuteCommandSource(ClearItemsCommandProperty, CommandComponentKind.Command);
		}

		private static void CanExecuteRoutedAddItemCommand(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = ((sender as FlowItemsView)?.CanExecuteRoutedAddItemCommand()).GetValueOrDefault();
			e.Handled = true;
		}

		private bool CanExecuteRoutedAddItemCommand()
		{
			return CommandSources.CanExecuteCommandSource(AddItemsCommandProperty, CommandComponentKind.Command);
		}

		private static void OnRoutedClearItemsCommand(object sender, ExecutedRoutedEventArgs e)
		{
			(sender as FlowItemsView)?.OnRoutedClearItemsCommand();
			e.Handled = true;
		}

		protected virtual void OnRoutedClearItemsCommand()
		{
			CommandSources.ExecuteCommandSource(ClearItemsCommandProperty, CommandComponentKind.Command);
		}

		private static void OnRoutedAddItemCommand(object sender, ExecutedRoutedEventArgs e)
		{
			(sender as FlowItemsView)?.OnRoutedAddItemCommand();
			e.Handled = true;
		}

		protected virtual void OnRoutedAddItemCommand()
		{
			var pt = Mouse.GetPosition(PresenterPanel);
			if (pt.X < 0 || pt.Y < 0 || pt.X > RenderSize.Width || pt.Y > RenderSize.Height)
			{
				pt.X = RenderSize.Width / 2;
				pt.Y = RenderSize.Height / 2;
			}
			OnCommonCommandParameterChanged(AddItemsCommandProperty, pt, CommandComponentKind.Command);
			CommandSources.ExecuteCommandSource(AddItemsCommandProperty, CommandComponentKind.Command);
		}

		public FlowItemsView()
		{
			InitCommandSources();
		}

		protected virtual void InitCommandSources()
		{
			CommandSources.ParentInputElement = this;
			CommandSources.RegisterCommandSource(AddItemsCommandProperty, CommandComponentKind.Command, true);
			CommandSources.RegisterCommandSource(RemoveItemsCommandProperty, CommandComponentKind.Command, true);
			CommandSources.RegisterCommandSource(ClearItemsCommandProperty, CommandComponentKind.Command, true);
		}

		protected CommonCommandSourceCollection CommandSources = new CommonCommandSourceCollection();

		private ConnectorControlAdornerController m_controller;
		private bool isOnDrawingPreviewConnector;
		private DefaultDrawingConnectorContext m_connectorContext = new DefaultDrawingConnectorContext();

		protected object m_currentItem;

		public Panel PresenterPanel => this.GetItemsPanelFromItemsControl();

		public bool IsOnDrawingPreviewConnector { get => isOnDrawingPreviewConnector; }

		public ItemContainerTemplateSelector ItemContainerTemplateSelector
		{
			get { return (ItemContainerTemplateSelector)GetValue(ItemContainerTemplateSelectorProperty); }
			set { SetValue(ItemContainerTemplateSelectorProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ItemContainerTemplateSelector.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ItemContainerTemplateSelectorProperty =
			MenuBase.ItemContainerTemplateSelectorProperty.AddOwner(typeof(FlowItemsView), new FrameworkPropertyMetadata(null, OnContainerTemplateSelectorChanged));

		private static void OnContainerTemplateSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
		{

		}

		public IDrawingConnectorContext ConnectorContext
		{
			get { return (IDrawingConnectorContext)GetValue(ConnectorContextProperty); }
			set { SetValue(ConnectorContextProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ConnectorContext.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ConnectorContextProperty =
			DependencyProperty.Register("ConnectorContext", typeof(IDrawingConnectorContext), typeof(FlowItemsView), new FrameworkPropertyMetadata(null, OnConnectorContextChanged));

		private static void OnConnectorContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
		{
			if (d is FlowItemsView view)
			{
				view.m_connectorContext.SetContext(args.NewValue as IDrawingConnectorContext);
			}
		}

		public ICommand AddItemsCommand
		{
			get { return (ICommand)GetValue(AddItemsCommandProperty); }
			set { SetValue(AddItemsCommandProperty, value); }
		}

		// Using a DependencyProperty as the backing store for AddItemsCommand.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty AddItemsCommandProperty =
			DependencyProperty.Register("AddItemsCommand", typeof(ICommand), typeof(FlowItemsView), new FrameworkPropertyMetadata(null, OnCommonCommandChanged));


		public ICommand RemoveItemsCommand
		{
			get { return (ICommand)GetValue(RemoveItemsCommandProperty); }
			set { SetValue(RemoveItemsCommandProperty, value); }
		}

		// Using a DependencyProperty as the backing store for RemoveItemsCommand.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty RemoveItemsCommandProperty =
			DependencyProperty.Register("RemoveItemsCommand", typeof(ICommand), typeof(FlowItemsView), new FrameworkPropertyMetadata(null, OnCommonCommandChanged));

		public ICommand ClearItemsCommand
		{
			get { return (ICommand)GetValue(ClearItemsCommandProperty); }
			set { SetValue(ClearItemsCommandProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ClearItemsCommand.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ClearItemsCommandProperty =
			DependencyProperty.Register("ClearItemsCommand", typeof(ICommand), typeof(FlowItemsView), new FrameworkPropertyMetadata(null, OnCommonCommandChanged));

		protected static void OnCommonCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is FlowItemsView view)
			{
				view.OnCommonCommandChanged(e.Property, (ICommand)e.NewValue);
			}
		}

		public IInputElement RemoveItemsCommandTarget
		{
			get { return (IInputElement)GetValue(RemoveItemsCommandTargetProperty); }
			set { SetValue(RemoveItemsCommandTargetProperty, value); }
		}

		// Using a DependencyProperty as the backing store for RemoveItemsCommandTarget.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty RemoveItemsCommandTargetProperty =
			DependencyProperty.Register("RemoveItemsCommandTarget", typeof(IInputElement), typeof(FlowItemsView), new FrameworkPropertyMetadata(null, OnCommonCommandTargetChanged));

		protected static void OnCommonCommandTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is FlowItemsView view)
			{
				view.OnCommonCommandTargetChanged(e.Property, (IInputElement)e.NewValue);
			}
		}

		protected override bool IsItemItsOwnContainerOverride(object item)
		{
			m_currentItem = item;
			return item is FlowItem;
		}

		protected override DependencyObject GetContainerForItemOverride()
		{
			var selector = ItemContainerTemplateSelector;
			if (selector != null)
			{
				DataTemplate dataTemplate = selector.SelectTemplate(m_currentItem, this);
				if (dataTemplate != null)
				{
					var flowItem = dataTemplate.LoadContent() as FlowItem;

					if (flowItem != null)
					{
						return flowItem;
					}
				}
			}
			return new FlowItem();
		}

		//protected override bool IsItemItsOwnContainerOverride(object item)
		//{
		//	return item is FlowItem;
		//}

		public void EnsureAdornerControllerForPresenterPanel()
		{
			if (m_controller is null)
			{
				var controllers = Adornments.GetAdornerControllers(PresenterPanel);
				foreach (var item in controllers)
				{
					if (item is ConnectorControlAdornerController controller)
					{
						m_controller = controller;
						break;
					}
				}
				if (m_controller is null)
				{
					m_controller = new ConnectorControlAdornerController();
					controllers.Add(m_controller);
				}
			}
		}

		public bool InvalidateAdornerControllerForPresenterPanel()
		{
			if (isOnDrawingPreviewConnector)
			{
				return false;
			}
			var controllers = Adornments.GetAdornerControllers(PresenterPanel);
			if (controllers.Remove(m_controller))
			{
				m_controller = null;
				return true;
			}
			return false;
		}

		public void BeginDrawPreviewConnector()
		{
			if (isOnDrawingPreviewConnector)
			{
				return;
			}
			var executor = GetExecutorFromMouseDevice();
			if (executor is null)
			{
				return;
			}
			var commandName = Anchorable.BeginAnchorActionCommandProperty.Name;
			if (executor.CanExecuteCommandSource(commandName, CommandComponentKind.Command))
			{
				//if (m_connectorContext.ReferenceVisual is null)
				//{
				//	m_connectorContext.ReferenceVisual = PresenterPanel;
				//}
				var element = executor as Visual;
				Point startPoint;
				if (element is null)
				{
					startPoint = Mouse.GetPosition(PresenterPanel);
				}
				else
				{
					if (!element.TryGetAnchorItemPositionRelativeTo(PresenterPanel, true, out startPoint))
					{
						startPoint = Mouse.GetPosition(PresenterPanel);
					}
				}
				m_connectorContext.PushStartPoint(startPoint.X, startPoint.Y);
				EnsureAdornerControllerForPresenterPanel();
				m_controller.SetCurrentActivity(true);
				var connector = m_controller.CurrentAdorner.Child;
				if (connector.IsHitTestVisible)
				{
					connector.IsHitTestVisible = false;
				}
				connector.SuspendConnectorPointsChange();
				connector.ConnectorStartPoint = startPoint;
				connector.ConnectorEndPoint = startPoint;
				connector.ResumeConnectorPointsChange();
				executor.ExecuteCommandSource(commandName, CommandComponentKind.Command);
				isOnDrawingPreviewConnector = true;
			}
			else
			{
				m_connectorContext.CancelConnectorDrawing();
			}
		}

		public void ContinueDrawPreviewConnector()
		{
			if (isOnDrawingPreviewConnector)
			{
				Point mousePoint = Mouse.GetPosition(PresenterPanel);
				var connector = m_controller.CurrentAdorner.Child;
				connector.ConnectorEndPoint = mousePoint;
			}
		}

		public void EndDrawPreviewConnector()
		{
			if (isOnDrawingPreviewConnector)
			{
				Point mousePoint = Mouse.GetPosition(PresenterPanel);
				var connector = m_controller.CurrentAdorner.Child;
				connector.ConnectorEndPoint = mousePoint;
				isOnDrawingPreviewConnector = false;
				m_controller.SetCurrentActivity(false);
				var executor = GetExecutorFromMouseDevice();
				if (executor is null)
				{
					m_connectorContext.CancelConnectorDrawing();
					return;
				}
				var commandName = Anchorable.EndAnchorActionCommandProperty.Name;
				if (executor.CanExecuteCommandSource(commandName, CommandComponentKind.Command))
				{
					var element = executor as Visual;
					Point endPoint;
					if (element is null)
					{
						endPoint = Mouse.GetPosition(PresenterPanel);
					}
					else
					{
						if (!element.TryGetAnchorItemPositionRelativeTo(PresenterPanel, true, out endPoint))
						{
							endPoint = Mouse.GetPosition(PresenterPanel);
						}
					}
					m_connectorContext.PushEndPoint(endPoint.X, endPoint.Y);
					executor.ExecuteCommandSource(commandName, CommandComponentKind.Command);
				}
				else
				{
					m_connectorContext.CancelConnectorDrawing();
				}
			}
		}

		private ICommandSourceExecutor<string> GetExecutorFromMouseDevice()
		{
			var mouseDevice = Mouse.PrimaryDevice;
			var absolutelyOverElement = mouseDevice.GetMouseAbsolutelyOver();
			if (absolutelyOverElement is null)
			{
				return null;
			}
			var result = absolutelyOverElement as ICommandSourceExecutor<string>;
			if (result is null)
			{
				var element = absolutelyOverElement as FrameworkElement;
				result = element?.TemplatedParent as ICommandSourceExecutor<string>;
			}
			return result;
		}

		protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			//base.OnPreviewMouseLeftButtonDown(e);
			BeginDrawPreviewConnector();
			Focus();
		}

		protected override void OnPreviewMouseMove(MouseEventArgs e)
		{
			//base.OnPreviewMouseMove(e);
			ContinueDrawPreviewConnector();
		}

		protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			//base.OnPreviewMouseLeftButtonUp(e);
			EndDrawPreviewConnector();
		}

		internal void TryRemoveItem(FlowItem item)
		{
			var itemModel = ItemContainerGenerator.ItemFromContainer(item);
			if (itemModel != null)
			{
				OnCommonCommandParameterChanged(RemoveItemsCommandProperty, itemModel, CommandComponentKind.Command);
				CommandSources.ExecuteCommandSource(RemoveItemsCommandProperty, CommandComponentKind.Command);
			}
		}

		protected virtual void OnCommonCommandChanged(DependencyProperty dependencyProperty, ICommand newValue, CommandComponentKind componentKind = CommandComponentKind.Command)
		{
			CommandSources.TrySetCommand(dependencyProperty, newValue, componentKind);
		}

		protected virtual void OnCommonCommandParameterChanged(DependencyProperty dependencyProperty, object newValue, CommandComponentKind componentKind = CommandComponentKind.CommandParameter)
		{
			CommandSources.TrySetCommandParameter(dependencyProperty, newValue, componentKind);
		}

		protected virtual void OnCommonCommandTargetChanged(DependencyProperty dependencyProperty, IInputElement newValue, CommandComponentKind componentKind = CommandComponentKind.CommandTarget)
		{
			CommandSources.TrySetCommandTarget(dependencyProperty, newValue, componentKind);
		}
	}
}
