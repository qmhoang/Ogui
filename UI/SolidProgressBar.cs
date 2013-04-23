using System;
using libtcod;

namespace Ogui.UI {
	public class SolidProgressBar : ProgressBar {
		private libtcod.TCODSpecialCharacter _block;

		public SolidProgressBar(ProgressBarTemplate template, libtcod.TCODSpecialCharacter block = TCODSpecialCharacter.HorzLine) : base(template) {
			this._block = block;
		}

		protected override void Redraw() {
			base.Redraw();
//            Canvas.PrintChar(0, 0, (int)libtcod.TCODSpecialCharacter.DoubleVertLine);
//            Canvas.PrintChar(this.LocalRect.TopRight, (int)libtcod.TCODSpecialCharacter.DoubleVertLine);
			double fraction = ((double) CurrentValue / MaximumValue);
			int barValue = (int) Math.Round(fraction * Size.Width);

			for (int x = 0; x < barValue; x++)
				Canvas.PrintChar(x, 0,
				                 (int) _block,
				                 DetermineMainPigment());
		}
	}
}