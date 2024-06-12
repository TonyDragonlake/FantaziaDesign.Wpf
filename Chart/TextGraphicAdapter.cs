using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using FantaziaDesign.Chart;
using FantaziaDesign.Core;
using FantaziaDesign.Graphics;
using FantaziaDesign.Wpf.Graphics;
using System.Globalization;
using WPF = System.Windows;

namespace FantaziaDesign.Wpf.Chart
{
	public class WpfTextGraphicAdapter : ITextGraphicAdapter
	{
		private SizeF _renderSize = new SizeF();
		private double _fontSize = TextFormatUtil.DefaultFontSize;
		private Typeface _typefaceCache;
		private Brush _brushCache = Brushes.Black;
		private GenericBrush _textBrush;
		private FormattedText _textGraphic;
		public SizeF RenderSize => _renderSize;

		public RenderFlag BuildTextGraphic(string text, VirtualTextFormat textFormat, GenericBrush textBrush)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				_renderSize.ZeroSize();
				if (string.IsNullOrWhiteSpace(_textGraphic?.Text))
				{
					return RenderFlag.None;
				}
				return RenderFlag.SizeChanged;
			}
			int updatedCode = 0;
			RenderFlag flag = 0;
			if (TryUpdateFontSize(textFormat))
			{
				updatedCode |= 0B001;
				flag |= RenderFlag.SizeChanged;
			}
			if (TryUpdateTypeface(textFormat))
			{
				updatedCode |= 0B010;
				flag |= RenderFlag.SizeChanged;
			}
			if (TryCompareAndUpdateBrush(textBrush))
			{
				updatedCode |= 0B100;
				flag |= RenderFlag.BrushChanged;
			}

			var direction = TextFormatUtil.WpfReadingDirection(textFormat?.ReadingDirection);
			var textAlignment = TextFormatUtil.WpfTextAlignment(textFormat?.TextAlignment);
			var trimming = TextFormatUtil.WpfTextTrimming(textFormat?.TrimmingBehavior);
			if (_textGraphic is null || _textGraphic.Text != text)
			{
				CreateTextGraphicInstance(text, direction, textAlignment, trimming);
				flag |= RenderFlag.SizeChanged;
				return flag;
			}

			switch (updatedCode)
			{
				case 0:
					break;
				case 1:
					{
						_textGraphic.SetFontSize(_fontSize);
					}
					break;
				case 2:
					{
						_textGraphic.SetFontTypeface(_typefaceCache);
					}
					break;
				case 4:
					{
						_textGraphic.SetForegroundBrush(_brushCache);
					}
					break;
				default:
					{
						CreateTextGraphicInstance(text, direction, textAlignment, trimming);
						return flag;
					}
			}
			
			if (_textGraphic.FlowDirection != direction)
			{
				_textGraphic.FlowDirection = direction;
				flag |= RenderFlag.SizeChanged;
			}

			if (_textGraphic.TextAlignment != textAlignment)
			{
				_textGraphic.TextAlignment = textAlignment;
				flag |= RenderFlag.SizeChanged;
			}

			if (_textGraphic.Trimming != trimming)
			{
				flag |= RenderFlag.SizeChanged;
			}

			if (flag > RenderFlag.None)
			{
				UpdateSize();
			}

			return flag;
		}

		private void CreateTextGraphicInstance(string text, WPF.FlowDirection direction, WPF.TextAlignment textAlignment, WPF.TextTrimming trimming)
		{
			_textGraphic = new FormattedText(text, CultureInfo.CurrentCulture, direction, _typefaceCache, _fontSize, _brushCache);
			_textGraphic.TextAlignment = textAlignment;
			_textGraphic.Trimming = trimming;
			UpdateSize();
		}

		private void UpdateSize()
		{
			_renderSize.SetSize((float)_textGraphic.WidthIncludingTrailingWhitespace, (float)_textGraphic.Height);
		}

		private bool TryUpdateFontSize(VirtualTextFormat textFormat)
		{
			if (textFormat is null)
			{
				if (_fontSize != TextFormatUtil.DefaultFontSize)
				{
					_fontSize = TextFormatUtil.DefaultFontSize;
					return true;
				}
				return false;
			}
			double target = textFormat.FontSize;
			if (target <= 0)
			{
				target = TextFormatUtil.DefaultFontSize;
			}
			if (_fontSize != target)
			{
				_fontSize = target;
				return true;
			}
			return false;
		}

		private bool TryCreateDefaultTypeface()
		{
			if (_typefaceCache is null)
			{
				_typefaceCache = TextFormatUtil.DefaultTypeface;
				return true;
			}
			return false;
		}

		private bool TryCompareAndUpdateBrush(GenericBrush textBrush)
		{
			if (textBrush is null && _textBrush is null)
			{
				if (_brushCache is null)
				{
					_brushCache = Brushes.Black;
					return true;
				}
				return false;
			}
			if (!_textBrush.Equals(textBrush))
			{
				if (!_textBrush.TryMakeBrush(out _brushCache))
				{
					_brushCache = Brushes.Black;
				}
				_textBrush = textBrush;
				return true;
			}
			return false;
		}

		private bool TryUpdateTypeface(VirtualTextFormat textFormat)
		{
			if (textFormat is null)
			{
				return TryCreateDefaultTypeface();
			}

			var style = textFormat.FontStyle.WpfFontStyle();
			var weight = textFormat.FontWeight.WpfFontWeight();
			var stretch = textFormat.FontStretch.WpfFontStretch();
			if (_typefaceCache is null
				|| _typefaceCache.FontFamily.Source != textFormat.FontFamilyName
				|| _typefaceCache.Style != style
				|| _typefaceCache.Weight != weight
				|| _typefaceCache.Stretch != stretch
				)
			{
				FontFamily fontFamily;
				if (string.IsNullOrWhiteSpace(textFormat.FontFamilyName))
				{
					fontFamily = TextFormatUtil.DefaultFontFamily;
				}
				else
				{
					fontFamily = new FontFamily(textFormat.FontFamilyName);
				}
				_typefaceCache = new Typeface(fontFamily, style, weight, stretch);
				return true;
			}
			return false;
		}

		public T GetTextGraphic<T>()
		{
			if (typeof(T) == typeof(FormattedText))
			{
				return (T)(object)_textGraphic;
			}
			else
			{
				throw new InvalidCastException("Cannot cast FormattedText");
			}
		}
	}

	public static class TextFormatUtil
	{
		private static readonly Typeface defaultTypeface = new Typeface(
					WPF.SystemFonts.MessageFontFamily,
					WPF.SystemFonts.MessageFontStyle,
					WPF.SystemFonts.MessageFontWeight,
					WPF.FontStretches.Normal
					);

		public static Typeface DefaultTypeface => defaultTypeface;

		public static FontFamily DefaultFontFamily => WPF.SystemFonts.MessageFontFamily;

		public static double DefaultFontSize => WPF.SystemFonts.MessageFontSize;

		public static WPF.FontStyle WpfFontStyle(this FontStyle style)
		{
			switch (style)
			{
				case FontStyle.Oblique:
					return WPF.FontStyles.Oblique;
				case FontStyle.Italic:
					return WPF.FontStyles.Italic;
				default:
					return WPF.FontStyles.Normal;
			}
		}

		public static WPF.FontStretch WpfFontStretch(this FontStretch stretch)
		{
			switch (stretch)
			{
				case FontStretch.UltraCondensed:
					return WPF.FontStretches.UltraCondensed;
				case FontStretch.ExtraCondensed:
					return WPF.FontStretches.ExtraCondensed;
				case FontStretch.Condensed:
					return WPF.FontStretches.Condensed;
				case FontStretch.SemiCondensed:
					return WPF.FontStretches.SemiCondensed;
				case FontStretch.SemiExpanded:
					return WPF.FontStretches.SemiExpanded;
				case FontStretch.Expanded:
					return WPF.FontStretches.Expanded;
				case FontStretch.ExtraExpanded:
					return WPF.FontStretches.ExtraExpanded;
				case FontStretch.UltraExpanded:
					return WPF.FontStretches.UltraExpanded;
				default:
					return WPF.FontStretches.Normal;
			}
		}

		public static WPF.FontWeight WpfFontWeight(this FontWeight weight)
		{
			switch (weight)
			{
				case FontWeight.Thin:
					return WPF.FontWeights.Thin;
				case FontWeight.ExtraLight:
					return WPF.FontWeights.ExtraLight;
				case FontWeight.Light:
					return WPF.FontWeights.Light;
				case FontWeight.Medium:
					return WPF.FontWeights.Medium;
				case FontWeight.SemiBold:
					return WPF.FontWeights.SemiBold;
				case FontWeight.Bold:
					return WPF.FontWeights.Bold;
				case FontWeight.ExtraBold:
					return WPF.FontWeights.ExtraBold;
				case FontWeight.Black:
					return WPF.FontWeights.Black;
				case FontWeight.ExtraBlack:
					return WPF.FontWeights.ExtraBlack;
				default:
					return WPF.FontWeights.Normal;
			}
		}

		public static WPF.FlowDirection WpfReadingDirection(ReadingDirection? direction)
		{
			switch (direction.GetValueOrDefault())
			{
				case ReadingDirection.RightToLeft:
				case ReadingDirection.BottomToTop:
					return WPF.FlowDirection.RightToLeft;
				default:
					return WPF.FlowDirection.LeftToRight;
			}
		}

		public static WPF.FlowDirection WpfReadingDirection(this ReadingDirection direction)
		{
			switch (direction)
			{
				case ReadingDirection.RightToLeft:
				case ReadingDirection.BottomToTop:
					return WPF.FlowDirection.RightToLeft;
				default:
					return WPF.FlowDirection.LeftToRight;
			}
		}

		internal static int WpfTrimmingCode(this TrimmingBehavior behavior)
		{
			var trim = (int)behavior - 1;
			if (trim < 0)
			{
				trim = 0;
			}
			return trim;
		}

		public static WPF.TextTrimming WpfTextTrimming(this TrimmingBehavior behavior)
		{
			var trim = (int)behavior - 1;
			if (trim < 0)
			{
				trim = 0;
			}
			return (WPF.TextTrimming)trim;
		}

		public static WPF.TextTrimming WpfTextTrimming(TrimmingBehavior? behavior)
		{
			var trim = (int)behavior.GetValueOrDefault() - 1;
			if (trim < 0)
			{
				trim = 0;
			}
			return (WPF.TextTrimming)trim;
		}

		public static WPF.TextAlignment WpfTextAlignment(TextAlignment? alignment)
		{
			return (WPF.TextAlignment)alignment.GetValueOrDefault();
		}

		public static WPF.TextAlignment WpfTextAlignment(this TextAlignment alignment)
		{
			return (WPF.TextAlignment)alignment;
		}

		internal static bool WpfTrimmingCodeNonEquals(TrimmingBehavior thisBehavior, TrimmingBehavior newBehavior, out int newCode)
		{
			var trim1 = (int)thisBehavior - 1;
			if (trim1 < 0)
			{
				trim1 = 0;
			}
			newCode = (int)newBehavior - 1;
			if (newCode < 0)
			{
				newCode = 0;
			}
			if (trim1 == newCode)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

	}

	public class WpfTextGraphicAdapterFactory : ITextGraphicAdapterFactory
	{
		public ITextGraphicAdapter NewAdapter()
		{
			return new WpfTextGraphicAdapter();
		}
	}


}
