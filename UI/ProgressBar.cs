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
using DEngine.Core;
using Ogui.Core;

namespace Ogui.UI {
	/// <summary>
	/// Base abstract class for all progress bars
	/// </summary>
	public class ProgressBarTemplate : ControlTemplate {
		public ProgressBarTemplate() {
			MinimumValue = 0;
			MaximumValue = 1;
			StartingValue = 0;
			Length = 2;
		}

		public int MinimumValue { get; set; }
		public int MaximumValue { get; set; }
		public int StartingValue { get; set; }
		public int Length { get; set; }

		public Pigment BarPigment { get; set; }

		public bool CanHaveKeyboardFocus { get; set; }

		public override Size CalculateSize() {
			return new Size(Length + 2, 1);
		}
	}

	public abstract class ProgressBar : Control {
		public int MinimumValue { get; set; }

		public int MaximumValue { get; set; }

		public Pigment BarPigment { get; set; }

		private int _currValue;

		public int CurrentValue {
			get { return _currValue; }
			set {
				if (value >= MinimumValue && value <= MaximumValue)
					_currValue = value;
			}
		}

		protected override Pigment DetermineMainPigment() {
			if (BarPigment != null)
				return BarPigment;

			return base.DetermineMainPigment();
		}

		protected ProgressBar(ProgressBarTemplate template) : base(template) {
			MinimumValue = template.MinimumValue;
			MaximumValue = template.MaximumValue;
			CurrentValue = template.StartingValue;
			BarPigment = template.BarPigment;

			HasFrame = false;

			CanHaveKeyboardFocus = template.CanHaveKeyboardFocus;
		}
	}
}