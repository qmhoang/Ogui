// Copyright (c) 2010-2013 Shane Baker, Quang-Minh Hoang
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using DEngine.Core;
using Ogui.Core;

namespace Ogui.UI {
	public class VerticalValueBarTemplate : ValueBarTemplate {
		public override Size CalculateSize() {
			return new Size(1, Length + 2);
		}
	}

	public class VerticalValueBar : ValueBarBase {
		public VerticalValueBar(VerticalValueBarTemplate template)
				: base(template) {
			HasFrame = false;
			CurrentValue = template.StartingValue;
			
			range = this.Size.Height - 2;

			BarPigment = template.BarPigment;

			minimumBGIntensity = template.MinimumBGIntensity;
			minimumFGIntensity = template.MinimumFGIntensity;
			CanHaveKeyboardFocus = template.CanHaveKeyboardFocus;
		}

		protected override void Redraw() {
			base.Redraw();

			float currBarFine = (float) CurrentValue - (float) MinimumValue;
			currBarFine = currBarFine / (float) (MaximumValue - MinimumValue);
			currBarFine = currBarFine * (float) range;

			Color bg, fg;
			float intensity;

			Canvas.PrintChar(0, 0, (int) libtcod.TCODSpecialCharacter.DoubleHorzLine);
			Canvas.PrintChar(this.LocalRect.BottomRight.Shift(-1, -1), (int) libtcod.TCODSpecialCharacter.DoubleHorzLine);

			for (int y = 0; y < range; y++) {
				float fy = (float) (y);
				float delta = Math.Abs(fy + 0.5f - currBarFine);
				if (delta <= 3f)
					intensity = (float) Math.Pow((3f - delta) / 3f, 0.5d);
				else
					intensity = 0f;

				bg = DetermineMainPigment().Background.ReplaceValue(
						Math.Max(minimumBGIntensity, intensity));

				fg = DetermineMainPigment().Foreground.ReplaceValue(
						Math.Max(minimumFGIntensity, intensity));

				Canvas.PrintChar(0, y + 1,
				                 (int) libtcod.TCODSpecialCharacter.HorzLine,
				                 new Pigment(fg, bg));
			}
		}
	}
}