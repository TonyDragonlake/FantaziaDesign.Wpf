using System;
using System.Windows;

namespace FantaziaDesign.Wpf.Controls
{
	public class BoundaryFrameAdornerController : AdornerController<BoundaryFrameAdorner>
	{
		private BoundaryFrameAdorner m_adorner;

		public override BoundaryFrameAdorner CurrentAdorner 
		{ 
			get => m_adorner; 
			protected set => m_adorner = value; 
		}

		protected override void ActiveAdorner()
		{
			if (m_adorner is null)
			{
				Predicate<BoundaryFrameAdorner> matchCondition = MatchFirst;
				Func<FrameworkElement, BoundaryFrameAdorner> creator = Creator;
				if (TryGetMatchedAdornerOrNewOverride(out m_adorner, matchCondition, creator))
				{
					m_adorner.Visibility = Visibility.Visible;
				}
				else
				{
					m_adorner = null;
				}
			}
			else
			{
				m_adorner.Visibility = Visibility.Visible;
			}
		}

		private static bool MatchFirst(BoundaryFrameAdorner adorner)
		{
			return true;
		}

		private static BoundaryFrameAdorner Creator(FrameworkElement element)
		{
			return new BoundaryFrameAdorner(element);
		}

		protected override void DeactiveAdorner(bool removeAction = false)
		{
			if (removeAction)
			{
				if (m_adorner is null)
				{
					return;
				}
				m_adornerLayer.Remove(m_adorner);
				m_adorner = null;
			}
			else
			{
				if (m_adorner is null)
				{
					Predicate<BoundaryFrameAdorner> matchCondition = MatchFirst;
					Func<FrameworkElement, BoundaryFrameAdorner> creator = Creator;
					if (TryGetMatchedAdornerOrNewOverride(out m_adorner, matchCondition, creator))
					{
						m_adorner.Visibility = Visibility.Collapsed;
					}
					else
					{
						m_adorner = null;
					}
				}
				else
				{
					m_adorner.Visibility = Visibility.Collapsed;
				}
			}
		}

	}
}
