//using System;
//using DEngine.Core;
//using Ogui.Core;
//
//namespace Ogui.UI {
//	public class ValueBarTemplate : ProgressBarTemplate {
//		public float MinimumBGIntensity { get; set; }
//		public float MinimumFGIntensity { get; set; }
//
//	}
//
//	public abstract class ValueBarBase : ProgressBar {
//		protected float minimumBGIntensity;
//		protected float minimumFGIntensity;
//		protected int range;
//		protected ValueBarBase(ProgressBarTemplate template) : base(template) {}
//	}
//
//	/// <summary>
//	/// A value bar is a graphical representation of a value.  It provides one of the elements
//	/// for a Slider, but it can also be used standalone as, for example, a progress bar.
//	/// </summary>
//	public class ValueBar : ValueBarBase {
//		#region Constructors
//
//		public ValueBar(ValueBarTemplate template)
//				: base(template) {
//			HasFrame = false;
//			CurrentValue = template.StartingValue;
//
//			range = this.Size.Width - 2;
//
//
//			BarPigment = template.BarPigment;
//
//			minimumBGIntensity = template.MinimumBGIntensity;
//			minimumFGIntensity = template.MinimumFGIntensity;
//			CanHaveKeyboardFocus = template.CanHaveKeyboardFocus;
//		}
//
//		#endregion
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
//			Canvas.PrintChar(0, 0, (int) libtcod.TCODSpecialCharacter.DoubleVertLine);
//			Canvas.PrintChar(this.LocalRect.TopRight.Shift(-1, 0), (int) libtcod.TCODSpecialCharacter.DoubleVertLine);
//
//			for (int x = 0; x < range; x++) {
//				float fx = (float) (x);
//				float delta = Math.Abs(fx + 0.5f - currBarFine);
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
//				Canvas.PrintChar(x + 1, 0,
//				                 (int) libtcod.TCODSpecialCharacter.HorzLine,
//				                 new Pigment(fg, bg));
//			}
//
//
//		}
//	}
//}