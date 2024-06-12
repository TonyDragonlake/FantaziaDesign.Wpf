using FantaziaDesign.Wpf.Core;
using System.Windows;
using System.Windows.Media;

namespace FantaziaDesign.Wpf.Controls
{
	public class BoundaryFrameAdorner : FrameworkElementAdorner<BoundaryFrame>
	{
		private MatrixTransform m_desiredTransform = new MatrixTransform();

		public BoundaryFrameAdorner(UIElement adornedElement) : base(adornedElement)
		{
		}

		public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
		{
			var child = m_child.LogicalChildVisual;
			if (child != null && TransformHelper.TryGetTransformMatrix(transform, out Matrix mat))
			{
				var pt = child.TransformToAncestor(this).Transform(new Point());
				mat.Translate(-pt.X, -pt.Y);
				m_desiredTransform.Matrix = mat;
				return m_desiredTransform;
			}
			return transform;
		}

		protected override void OnAttachAdornerChild(UIElement adornedElement)
		{
			var sizePlaceholder = new SizePlaceholder();
			// sizePlaceholder.SetBinding(SizePlaceholder.UIElementTargetProperty, new Binding(nameof(AdornedElement)) { Source = this, Mode = BindingMode.OneWay});
			sizePlaceholder.UIElementTarget = adornedElement;
			m_child = new BoundaryFrame();
			m_child.Content = sizePlaceholder;
		}
	}
}
