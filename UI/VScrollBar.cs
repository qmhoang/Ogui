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
	public class VScrollBarTemplate : ControlTemplate {
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
		/// The speed of the buttons' emit event cycle, in millisecond delay between each event.
		/// Defaults to 0.
		/// </summary>
		public uint SpinSpeed { get; set; }

		/// <summary>
		/// The minimum value that this spin control can have.  Defaults to 0.
		/// </summary>
		public int MinimumValue { get; set; }

		/// <summary>
		/// The maximum value that this spin control can have.  Defaults to 1.
		/// </summary>
		public int MaximumValue { get; set; }

		public int Height { get; set; }

		public Pigment BarPigment { get; set; }

		public override Size CalculateSize() {
			return new Size(1, Height);
		}
	}

	public class VScrollBar : Control {
		#region Events

		public event EventHandler ValueChanged;

		#endregion
		#region Public Properties
		/// <summary>
		/// Get the minimum value that this spin control can have.
		/// </summary>
		public int MinimumValue {
			get { return _minimumValue; }
			set {
				_minimumValue = value;
				if (_valueBar != null)
					_valueBar.MinimumValue = value;
			}
		}


		/// <summary>
		/// Get the maximum value that this spin control can have.
		/// </summary>
		public int MaximumValue {
			get { return _maximumValue; }
			set {
				_maximumValue = value;
				if (_valueBar != null)
					_valueBar.MaximumValue = value;
			}
		}

		/// <summary>
		/// Get the current value of the slider.
		/// </summary>
		public int CurrentValue {
			get { return _currentValue; }

			set {
				int newVal = value;

				if (newVal < MinimumValue)
					newVal = MinimumValue;

				if (newVal > MaximumValue)
					newVal = MaximumValue;


				_currentValue = newVal;

				if (_valueBar != null)
					_valueBar.CurrentValue = _currentValue;

				OnValueChanged();
			}
		}

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

		public Pigment BarPigment { get; set; }
		#endregion

		/// <summary>
		/// Called when this.CurrentValue has changed to a different value.
		/// </summary>
		protected virtual void OnValueChanged() {
			if (ValueChanged != null)
				ValueChanged(this, EventArgs.Empty);
		}

		public VScrollBar(VScrollBarTemplate template) : base(template) {
			MinimumValue = template.MinimumValue;
			MaximumValue = template.MaximumValue;

			CurrentValue = template.StartingValue;
			if (CurrentValue < MinimumValue || CurrentValue > MaximumValue)
				CurrentValue = MinimumValue;

			HasFrame = false;
			CanHaveKeyboardFocus = false;
			HilightWhenMouseOver = false;

			BarPigment = template.BarPigment;

			SpinDelay = template.SpinDelay;
			SpinSpeed = template.SpinSpeed;
		}

		protected internal override void OnSettingUp() {
			base.OnSettingUp();
			
			int fieldWidth = NumberEntryTemplate.CalculateFieldWidth(MaximumValue, MinimumValue);
			Size fieldSize = new Size(fieldWidth, 1);

			if (BarPigment == null)
				BarPigment = DetermineMainPigment();

			_valueBar = new VerticalValueBar(new VerticalValueBarTemplate()
			                                {
			                                		TopLeftPos = this.LocalToScreen(new Point(0, 0)),
			                                		Length = this.Size.Height - 2,
			                                		MaximumValue = this.MaximumValue,
			                                		MinimumValue = this.MinimumValue,
			                                		StartingValue = this.CurrentValue,
			                                		BarPigment = this.BarPigment,
			                                });

			_topButton = new EmitterButton(new EmitterButtonTemplate()
			                              {
			                              		HasFrameBorder = false,
												Label = Char.ToString((char)libtcod.TCODSpecialCharacter.ArrowNorthNoTail),
			                              		TopLeftPos = this.LocalToScreen(new Point(0, 0)),
			                              		StartEmittingDelay = SpinDelay,
			                              		Speed = SpinSpeed
			                              });

			_bottomButton = new EmitterButton(new EmitterButtonTemplate()
			                                 {
			                                 		HasFrameBorder = false,
													Label = Char.ToString((char)libtcod.TCODSpecialCharacter.ArrowSouthNoTail),
			                                 		TopLeftPos = this.LocalToScreen(new Point(0, 0).Shift(0, Size.Height - 1)),
			                                 		StartEmittingDelay = SpinDelay,
			                                 		Speed = SpinSpeed
			                                 });

			_valueBar.MouseMoved += valueBar_MouseMoved;

			_valueBar.MouseButtonDown += valueBar_MouseButtonDown;

			_topButton.Emit += topButton_Emit;
			_bottomButton.Emit += bottomButton_Emit;
		}

		protected internal override void OnAdded() {
			base.OnAdded();
			ParentWindow.AddControls(_valueBar);
			ParentWindow.AddControls(_topButton, _bottomButton);
		}

		protected internal override void OnRemoved() {
			base.OnRemoved();
			ParentWindow.RemoveControl(_valueBar);
			ParentWindow.RemoveControl(_topButton);
			ParentWindow.RemoveControl(_bottomButton);
		}

		private void valueBar_MouseMoved(object sender, MouseEventArgs e) {
			if (e.MouseData.MouseButton == MouseButton.LeftButton) {
				CurrentValue = CalculateValue(e.MouseData.PixelPosition.Y);
			}
		}

		private void valueBar_MouseButtonDown(object sender, MouseEventArgs e) {
			if (e.MouseData.MouseButton == MouseButton.LeftButton) {
				CurrentValue = CalculateValue(e.MouseData.PixelPosition.Y);
			}
		}

		private void topButton_Emit(object sender, EventArgs e) {
			if (CurrentValue > MinimumValue)
				CurrentValue = (CurrentValue - 1);
		}

		private void bottomButton_Emit(object sender, EventArgs e) {
			if (CurrentValue < MaximumValue)
				CurrentValue = (CurrentValue + 1);
		}

		private int CalculateValue(int pixelPosY) {
			int charHeight = Canvas.GetCharSize().Height;
			int currPy = pixelPosY;

			currPy = currPy - (charHeight * _valueBar.ScreenRect.Top) - 2 * charHeight;

			int widthInPy = (_valueBar.Size.Height - 4) * charHeight;

			float pixposPercent = (float)currPy / (float)widthInPy;

			return (int)((float)(MaximumValue - MinimumValue) * pixposPercent) + MinimumValue;
		}

		#region Private Fields
		private EmitterButton _topButton, _bottomButton;

		private VerticalValueBar _valueBar;

		private int _minimumValue, _maximumValue;
		private int _currentValue;
		#endregion

		#region	Disposable
		protected override void Dispose(bool isDisposing) {
			base.Dispose(isDisposing);

			if (isDisposing) {

				if (_valueBar != null)
					_valueBar.Dispose();

				if (_topButton != null)
					_topButton.Dispose();

				if (_bottomButton != null)
					_bottomButton.Dispose();
			}
		}

		#endregion
	}
}