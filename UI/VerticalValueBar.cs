//using System;
//using DEngine.Core;
//using Ogui.Core;
//
//namespace Ogui.UI {
//	public class VerticalValueBarTemplate : ValueBarTemplate {
//		public override Size CalculateSize() {
//			return new Size(1, Length + 2);
//		}
//	}
//
//	public class VerticalValueBar : ValueBarBase {
//		public VerticalValueBar(VerticalValueBarTemplate template)
//				: base(template) {
//			HasFrame = false;
//			CurrentValue = template.StartingValue;
//			
//			range = this.Size.Height - 2;
//
//			BarPigment = template.BarPigment;
//
//			minimumBGIntensity = template.MinimumBGIntensity;
//			minimumFGIntensity = template.MinimumFGIntensity;
//			CanHaveKeyboardFocus = template.CanHaveKeyboardFocus;
//		}
//
//		protected override void Redraw() {
//			base.Redraw();
//
//			float currBarFine = (float) CurrentValue - (float) MinimumValue;
//			currBarFine = currBarFine / (float) (MaximumValue - MinimumValue);
//			currBarFine = currBarFine * (float) range;
//
//			Color bg, fg;
//			float intensity;
//
//			Canvas.PrintChar(0, 0, (int) libtcod.TCODSpecialCharacter.DoubleHorzLine);
//			Canvas.PrintChar(this.LocalRect.BottomRight.Shift(-1, -1), (int) libtcod.TCODSpecialCharacter.DoubleHorzLine);
//
//			for (int y = 0; y < range; y++) {
//				float fy = (float) (y);
//				float delta = Math.Abs(fy + 0.5f - currBarFine);
//				if (delta <= 3f)
//					intensity = (float) Math.Pow((3f - delta) / 3f, 0.5d);
//				else
//					intensity = 0f;
//
//				bg = DetermineMainPigment().Background.ReplaceValue(
//						Math.Max(minimumBGIntensity, intensity));
//
//				fg = DetermineMainPigment().Foreground.ReplaceValue(
//						Math.Max(minimumFGIntensity, intensity));
//
//				Canvas.PrintChar(0, y + 1,
//				                 (int) libtcod.TCODSpecialCharacter.HorzLine,
//				                 new Pigment(fg, bg));
//			}
//		}
//	}
//}