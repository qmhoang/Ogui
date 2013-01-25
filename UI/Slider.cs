//Copyright (c) 2010 Shane Baker
//
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in
//all copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//THE SOFTWARE.
//

using System;
using DEngine.Core;
using Ogui.Core;

namespace Ogui.UI {
	public class SliderTemplate : ControlTemplate {
		public SliderTemplate() {}

		/// <summary>
		/// If true, the button will have a frame drawn around it.  If autosizing, space
		/// for the frame will be added.  Defaults to true.
		/// </summary>
		public bool HasFrameBorder { get; set; }

		/// <summary>
		/// If true, the label will not be display or taken into account
		/// </summary>
		public bool ShowLabel { get; set; }

		/// <summary>
		/// The minimum value that this spin control can have.  Defaults to 0.
		/// </summary>
		public int MinimumValue { get; set; }

		/// <summary>
		/// The maximum value that this spin control can have.  Defaults to 1.
		/// </summary>
		public int MaximumValue { get; set; }

		/// <summary>
		/// An optional label to display to the left of the numerical entry.  Defaults
		/// to empty string (no label).
		/// </summary>
		public string Label { get; set; }

		/// <summary>
		/// The value that the slider will initially have.  Defaults to 0.
		/// </summary>
		public int StartingValue { get; set; }

		/// <summary>
		/// The delay in milliseconds after first clicking on a spin button before
		/// the spin cycle starts.  Defaults to 0.
		/// </summary>
		public uint SpinDelay { get; set; }

		/// <summary>
		/// Speed for the buttons spins
		/// </summary>
		public uint SpinSpeed { get; set; }

		public int MinimumWidth { get; set; }

		public Pigment BarPigment { get; set; }

		public override Size CalculateSize() {
			int width = HasFrameBorder ? 2 : 0; 
			int height = HasFrameBorder ? 2 : 0;	// frames add 2

			if (!string.IsNullOrEmpty(Label))
				width += Canvas.TextLength(Label) + 1;

			width += NumberEntryTemplate.CalculateFieldWidth(MaximumValue, MinimumValue);
			//width += 3;

			width = Math.Max(width, MinimumWidth);

			if (ShowLabel) // we don't have a label;
				height++;

			return new Size(width, height);
		}
	}

	public class Slider : Control {
		#region Events

		public event EventHandler ValueChanged;

		#endregion

		#region Constructors

		/// <summary>
		/// Construct a Slider instance using the specified template
		/// </summary>
		/// <param name="template"></param>
		public Slider(SliderTemplate template)
				: base(template) {
			MinimumValue = template.MinimumValue;
			MaximumValue = template.MaximumValue;

			Label = template.Label;
			if (Label == null)
				Label = "";

			ShowLabel = template.ShowLabel;

			CurrentValue = template.StartingValue;
			if (CurrentValue < MinimumValue || CurrentValue > MaximumValue)
				CurrentValue = MinimumValue;

			HasFrame = template.HasFrameBorder;
			CanHaveKeyboardFocus = false;
			HilightWhenMouseOver = false;

			BarPigment = template.BarPigment;

			SpinDelay = template.SpinDelay;
			SpinSpeed = template.SpinSpeed;
		}

		#endregion

		#region Public Properties
		/// <summary>
		/// If true, the label will not be display or taken into account
		/// </summary>
		public bool ShowLabel { get; private set; }

		/// <summary>
		/// Get the minimum value that this spin control can have.
		/// </summary>
		public int MinimumValue { get; private set; }

		/// <summary>
		/// Get the maximum value that this spin control can have.
		/// </summary>
		public int MaximumValue { get; private set; }

		/// <summary>
		/// Get the optional label to display to the left of the numerical entry. Defaults
		/// to empty string (no label).
		/// </summary>
		public string Label { get; private set; }

		/// <summary>
		/// Get or set the slider bar color pigment.
		/// </summary>
		public Pigment BarPigment { get; set; }

		/// <summary>
		/// The delay in milliseconds after first clicking on a spin button before
		/// the spin cycle starts.  Defaults to 0.
		/// </summary>
		protected uint SpinDelay { get; set; }

		/// <summary>
		/// The speed of the buttons' emit event cycle, in millisecond delay between each event.
		/// Defaults to 0.
		/// </summary>
		protected uint SpinSpeed { get; set; }

		/// <summary>
		/// Get the current value of the slider.
		/// </summary>
		public int CurrentValue {
			get { return currentValue; }

			protected set {
				int newVal = value;

				if (newVal < MinimumValue)
					newVal = MinimumValue;

				if (newVal > MaximumValue)
					newVal = MaximumValue;

				if (newVal != currentValue)
					currentValue = newVal;
			}
		}

		private int currentValue;

		#endregion

		#region Protected Methods

		/// <summary>
		/// Called when this.CurrentValue has changed to a different value.
		/// </summary>
		protected virtual void OnValueChanged() {
			if (ValueChanged != null)
				ValueChanged(this, EventArgs.Empty);
		}
		
		/// <summary>
		/// Creates the NumberEntry and ValueBar for this slider.
		/// </summary>
		protected internal override void OnSettingUp() {
			base.OnSettingUp();

			Point fieldPos;
			if (HasFrame) {
				if (!string.IsNullOrEmpty(Label)) {
					labelRect = new Rect(1, 1, Label.Length + 1, 1);
					fieldPos = new Point(Label.Length + 2, 1);
				} else
					fieldPos = new Point(1, 1);
			} else {
				if (!string.IsNullOrEmpty(Label)) {
					labelRect = new Rect(0, 0, Label.Length + 1, 1);
					fieldPos = new Point(Label.Length + 2, 0);
				} else
					fieldPos = new Point(0, 0);
			}


			int fieldWidth = NumberEntryTemplate.CalculateFieldWidth(MaximumValue, MinimumValue);
			Size fieldSize = new Size(fieldWidth, 1);
			fieldRect = new Rect(fieldPos, fieldSize);

			if (BarPigment == null)
				BarPigment = DetermineMainPigment();

			numEntry = new NumberEntry(new NumberEntryTemplate()
			                           {
			                           		HasFrameBorder = false,
			                           		MinimumValue = this.MinimumValue,
			                           		MaximumValue = this.MaximumValue,
			                           		StartingValue = CurrentValue,
			                           		CommitOnLostFocus = true,
			                           		ReplaceOnFirstKey = true,
			                           		TopLeftPos = this.LocalToScreen(fieldRect.TopLeft)
			                           });

			int w = 0, h = 0;

			if (ShowLabel)
				h++;
			
			if (HasFrame) {
				h++;
				w++;
			}
			
			valueBar = new ValueBar(new ValueBarTemplate()
			                        {
			                        		TopLeftPos = this.LocalToScreen(new Point(w, h)),
			                        		Length = this.Size.Width - (HasFrame ? 4 : 2),
			                        		MaximumValue = this.MaximumValue,
			                        		MinimumValue = this.MinimumValue,
			                        		StartingValue = this.CurrentValue,
			                        		BarPigment = this.BarPigment
			                        });

			leftButton = new EmitterButton(new EmitterButtonTemplate()
			                               {
			                               		HasFrameBorder = false,
			                               		Label = "-",
												TopLeftPos = this.LocalToScreen(new Point(w, h)),
			                               		StartEmittingDelay = SpinDelay,
			                               		Speed = SpinSpeed
			                               });
			rightButton = new EmitterButton(new EmitterButtonTemplate()
			                                {
			                                		HasFrameBorder = false,
			                                		Label = "+",
													TopLeftPos = this.LocalToScreen(new Point(w, h).Shift(Size.Width - (HasFrame ? 3 : 1), 0)),
			                                		StartEmittingDelay = SpinDelay,
			                                		Speed = SpinSpeed
			                                });
			
			numEntry.EntryChanged += numEntry_EntryChanged;

			valueBar.MouseMoved += valueBar_MouseMoved;

			valueBar.MouseButtonDown += valueBar_MouseButtonDown;
			
			leftButton.Emit += leftButton_Emit;
			rightButton.Emit += rightButton_Emit;
		}

		protected internal override void OnAdded() {
			base.OnAdded();
			if (ShowLabel)
				ParentWindow.AddControl(numEntry);
			ParentWindow.AddControls(valueBar);
			ParentWindow.AddControls(leftButton, rightButton);
		}

		protected internal override void OnRemoved() {
			base.OnRemoved();

			ParentWindow.RemoveControl(numEntry);
			ParentWindow.RemoveControl(valueBar);
			ParentWindow.RemoveControl(leftButton);
			ParentWindow.RemoveControl(rightButton);
		}

		/// <summary>
		/// Draws this Slider's label.
		/// </summary>
		protected override void Redraw() {
			base.Redraw();
			
			if (ShowLabel)
				Canvas.PrintString(labelRect.TopLeft, Label);
		}

		#endregion

		#region Private

		private void valueBar_MouseMoved(object sender, MouseEventArgs e) {
			if (e.MouseData.MouseButton == MouseButton.LeftButton) {
				int newVal = CalculateValue(e.MouseData.PixelPosition.X);

				numEntry.TrySetValue(newVal);
			}
		}

		private int CalculateValue(int pixelPosX) {
			int charWidth = Canvas.GetCharSize().Width;
			int currPx = pixelPosX;

			currPx = currPx - (charWidth * valueBar.ScreenRect.Left) - 2 * charWidth;

			int widthInPx = (valueBar.Size.Width - 4) * charWidth;

			float pixposPercent = (float) currPx / (float) widthInPx;

			return (int) ((float) (MaximumValue - MinimumValue) * pixposPercent) + MinimumValue;
		}

		private void numEntry_EntryChanged(object sender, EventArgs e) {
			int value = numEntry.CurrentValue;

			if (this.CurrentValue != value) {
				this.CurrentValue = value;
				valueBar.CurrentValue = this.CurrentValue;
				OnValueChanged();
			}
		}

		private void valueBar_MouseButtonDown(object sender, MouseEventArgs e) {
			if (e.MouseData.MouseButton == MouseButton.LeftButton) {
				int newVal = CalculateValue(e.MouseData.PixelPosition.X);

				numEntry.TrySetValue(newVal);
			}
		}

		private void leftButton_Emit(object sender, EventArgs e) {
			if (CurrentValue > MinimumValue)
				numEntry.TrySetValue(CurrentValue - 1);
		}

		private void rightButton_Emit(object sender, EventArgs e) {
			if (CurrentValue < MaximumValue)
				numEntry.TrySetValue(CurrentValue + 1);
		}
		
		private NumberEntry numEntry;
		private ValueBar valueBar;
		private Rect labelRect;
		private Rect fieldRect;

		private EmitterButton leftButton, rightButton;

		#endregion

		#region Dispose

		protected override void Dispose(bool isDisposing) {
			base.Dispose(isDisposing);

			if (isDisposing) {
				if (numEntry != null)
					numEntry.Dispose();

				if (valueBar != null)
					valueBar.Dispose();

				if (leftButton != null)
					leftButton.Dispose();

				if (rightButton != null)
					rightButton.Dispose();
			}
		}

		#endregion
	}
}