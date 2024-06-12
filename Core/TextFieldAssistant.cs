using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace FantaziaDesign.Wpf.Core
{
	public static class TextFieldAssistant
	{
		static TextFieldAssistant()
		{
			EventManager.RegisterClassHandler(typeof(TextBoxBase), Keyboard.PreviewKeyUpEvent, new KeyEventHandler(OnTextBoxBaseClassPreviewKeyUp), false);
		}

		public static KeyGesture GetToNextFocusKeyGesture(DependencyObject obj)
		{
			return (KeyGesture)obj.GetValue(ToNextFocusKeyGestureProperty);
		}

		public static void SetToNextFocusKeyGesture(DependencyObject obj, KeyGesture value)
		{
			obj.SetValue(ToNextFocusKeyGestureProperty, value);
		}

		// Using a DependencyProperty as the backing store for ToNextFocusKeyGesture.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ToNextFocusKeyGestureProperty =
			DependencyProperty.RegisterAttached("ToNextFocusKeyGesture", typeof(KeyGesture), typeof(TextFieldAssistant), new FrameworkPropertyMetadata(null));


		private static void OnTextBoxBaseClassPreviewKeyUp(object sender, KeyEventArgs e)
		{
			if (sender is TextBoxBase textField)
			{
				var keyGesture = GetToNextFocusKeyGesture(textField);
				if (keyGesture != null && keyGesture.Matches(textField, e))
				{
					textField.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
				}
			}
		}
	}
}
