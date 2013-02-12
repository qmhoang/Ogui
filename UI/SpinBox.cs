using System;
using DEngine.Core;

namespace Ogui.UI {
	public class SpinBoxTemplate : ControlTemplate {
		public SpinBoxTemplate() {
			MaximumValue = 1;
			MaximumValue = 0;
			Label = "";
		}

		/// <summary>
		/// The minimum value that this spin control can have.  Defaults to 0.
		/// </summary>
		public int MinimumValue { get; set; }

		/// <summary>
		/// The maximum value that this spin control can have.  Defaults to 1.
		/// </summary>
		public int MaximumValue { get; set; }

		/// <summary>
		/// The delay in milliseconds after first clicking on a spin button before
		/// the spin cycle starts.  Defaults to 0.
		/// </summary>
		public uint SpinDelay { get; set; }

		/// <summary>
		/// The speed of the spin cycle.  This is measure in the millisecond delay before
		/// each step; thus, a smaller number here means a faster spin.  Defaults to 0.
		/// </summary>
		public uint SpinSpeed { get; set; }

		/// <summary>
		/// And optional label to display to the left of the numerical entry and spin buttons.  Defaults
		/// to empty string (no label).
		/// </summary>
		public string Label { get; set; }

		/// <summary>
		/// The value that the spin box will initially have.  Defaults to 0.
		/// </summary>
		public int StartingValue { get; set; }

		public override Size CalculateSize() {
			int width = 2; // for frame

			if (!string.IsNullOrEmpty(Label))
				width += Canvas.TextLength(Label) + 1;

			width += NumberEntryTemplate.CalculateFieldWidth(MaximumValue, MinimumValue);
			width += 3; // for buttons

			return new Size(width, 3);
		}
	}

	public class SpinBox : Control {
		#region Events

		public event EventHandler ValueChanged;

		#endregion

		#region Constructors

		public SpinBox(SpinBoxTemplate template)
				: base(template) {
			MinimumValue = template.MinimumValue;
			MaximumValue = template.MaximumValue;
			SpinDelay = template.SpinDelay;
			SpinSpeed = template.SpinSpeed;
			Label = template.Label;

			if (Label == null)
				Label = "";

			CurrentValue = template.StartingValue;
			if (CurrentValue < MinimumValue || CurrentValue > MaximumValue)
				CurrentValue = MinimumValue;


			HasFrame = true;
			CanHaveKeyboardFocus = false;
			HilightWhenMouseOver = false;
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// The current value of the spin box.
		/// </summary>
		public int CurrentValue {
			get { return currentValue; }
			set {
				int newVal = value;

				if (newVal < MinimumValue)
					newVal = MinimumValue;

				if (newVal > MaximumValue)
					newVal = MaximumValue;

				if (newVal != currentValue)
					currentValue = value;
			}
		}

		private int currentValue;

		#endregion

		#region Protected Properties

		/// <summary>
		/// The minimum value that this spin control can have.  Defaults to 0.
		/// </summary>
		protected int MinimumValue { get; private set; }

		/// <summary>
		/// The delay in milliseconds after first clicking on a spin button before
		/// the spin cycle starts.  Defaults to 0.
		/// </summary>
		protected int MaximumValue { get; private set; }

		/// <summary>
		/// The delay in milliseconds after first clicking on a spin button before
		/// the spin cycle starts.  Defaults to 0.
		/// </summary>
		protected uint SpinDelay { get; set; }

		/// <summary>
		/// And optional label to display to the left of the numerical entry and spin buttons.  Defaults
		/// to empty string (no label).
		/// </summary>
		protected uint SpinSpeed { get; set; }

		/// <summary>
		/// And optional label to display to the left of the numerical entry and spin buttons.  Defaults
		/// to empty string (no label).
		/// </summary>
		protected string Label { get; private set; }

		#endregion

		#region Protected Methods

		protected virtual void OnValueChanged() {
			if (ValueChanged != null)
				ValueChanged(this, EventArgs.Empty);
		}

		protected internal override void OnSettingUp() {
			base.OnSettingUp();

			if (!string.IsNullOrEmpty(Label)) {
				labelRect = new Rect(new Point(1, 1), new Point(Label.Length + 1, 1));

				upButtonPos = new Point(Label.Length + 2, 1) + ActualScreenPosition;
			} else
				upButtonPos = Point.One + ActualScreenPosition;

			int fieldWidth = NumberEntryTemplate.CalculateFieldWidth(MaximumValue, MinimumValue);
			Size fieldSize = new Size(fieldWidth, 1);
			fieldRect = new Rect(upButtonPos.Shift(2, 0), fieldSize);

			downButtonPos = fieldRect.TopRight.Shift(1, 0);

			numEntry = new NumberEntry(new NumberEntryTemplate()
			                           {
			                           		HasFrameBorder = false,
			                           		MinimumValue = this.MinimumValue,
			                           		MaximumValue = this.MaximumValue,
			                           		StartingValue = CurrentValue,
			                           		CommitOnLostFocus = true,
			                           		ReplaceOnFirstKey = true,
			                           		TopLeftPos = fieldRect.TopLeft
			                           });

			upButton = new EmitterButton(new EmitterButtonTemplate()
			                             {
			                             		HasFrameBorder = false,
			                             		Label = ((char) libtcod.TCODSpecialCharacter.ArrowNorthNoTail).ToString(),
			                             		TopLeftPos = upButtonPos,
			                             		StartEmittingDelay = SpinDelay,
			                             		Speed = SpinSpeed
			                             });

			downButton = new EmitterButton(new EmitterButtonTemplate()
			                               {
			                               		HasFrameBorder = false,
			                               		Label = ((char) libtcod.TCODSpecialCharacter.ArrowSouthNoTail).ToString(),
			                               		TopLeftPos = downButtonPos,
			                               		StartEmittingDelay = SpinDelay,
			                               		Speed = SpinSpeed
			                               });


			upButton.Emit += upButton_Emit;
			downButton.Emit += downButton_Emit;
			numEntry.EntryChanged += numEntry_EntryChanged;
		}

		protected internal override void OnAdded() {
			base.OnAdded();
			ParentWindow.AddControls(downButton, upButton, numEntry);
		}

		protected internal override void OnRemoved() {
			base.OnRemoved();
			ParentWindow.RemoveControl(numEntry);
			ParentWindow.RemoveControl(upButton);
			ParentWindow.RemoveControl(downButton);
		}

		protected override void Redraw() {
			base.Redraw();
			if (!string.IsNullOrEmpty(Label)) {
				Canvas.PrintString(labelRect.TopLeft, Label);

				Canvas.PrintChar(labelRect.TopRight.Shift(0, -1),
				                 (int) libtcod.TCODSpecialCharacter.TeeSouth,
				                 DetermineFramePigment());

				Canvas.PrintChar(labelRect.TopRight.Shift(0, 0),
				                 (int) libtcod.TCODSpecialCharacter.VertLine,
				                 DetermineFramePigment());

				Canvas.PrintChar(labelRect.TopRight.Shift(0, 1),
				                 (int) libtcod.TCODSpecialCharacter.TeeNorth,
				                 DetermineFramePigment());
			}
		}

		#endregion

		#region Private

		private void numEntry_EntryChanged(object sender, EventArgs e) {
			NumberEntry entry = sender as NumberEntry;

			if (this.CurrentValue != entry.CurrentValue) {
				this.CurrentValue = entry.CurrentValue;
				OnValueChanged();
			}
		}

		private void downButton_Emit(object sender, EventArgs e) {
			numEntry.TryCommit();
			if (CurrentValue > MinimumValue)
				numEntry.TrySetValue(CurrentValue - 1);
		}

		private void upButton_Emit(object sender, EventArgs e) {
			numEntry.TryCommit();
			if (CurrentValue < MaximumValue)
				numEntry.TrySetValue(CurrentValue + 1);
		}

		private NumberEntry numEntry;
		private EmitterButton downButton;
		private EmitterButton upButton;
		private Rect fieldRect;
		private Rect labelRect;
		private Point upButtonPos;
		private Point downButtonPos;

		#endregion

		#region Dispose

		protected override void Dispose(bool isDisposing) {
			base.Dispose(isDisposing);

			if (isDisposing) {
				if (numEntry != null)
					numEntry.Dispose();

				if (upButton != null)
					upButton.Dispose();

				if (downButton != null)
					downButton.Dispose();
			}
		}

		#endregion
	}
}