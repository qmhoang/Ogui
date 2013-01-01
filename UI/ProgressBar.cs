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
			Width = 2;
		}

		public int MinimumValue { get; set; }
		public int MaximumValue { get; set; }
		public int StartingValue { get; set; }
		public int Width { get; set; }

		public Pigment BarPigment { get; set; }

		public bool CanHaveKeyboardFocus { get; set; }

		public override Size CalculateSize() {
			return new Size(Width + 2, 1);
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