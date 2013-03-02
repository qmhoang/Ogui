//using System;
//using DEngine.Core;
//using Ogui.Core;
//
//namespace Ogui.UI {
//	public class VScrollBarTemplate : ControlTemplate {
//		/// <summary>
//		/// The value that the slider will initially have.  Defaults to 0.
//		/// </summary>
//		public int StartingValue { get; set; }
//
//		/// <summary>
//		/// The delay in milliseconds after first clicking on a spin button before
//		/// the spin cycle starts.  Defaults to 0.
//		/// </summary>
//		public uint SpinDelay { get; set; }
//
//		/// <summary>
//		/// The speed of the buttons' emit event cycle, in millisecond delay between each event.
//		/// Defaults to 0.
//		/// </summary>
//		public uint SpinSpeed { get; set; }
//
//		/// <summary>
//		/// The minimum value that this spin control can have.  Defaults to 0.
//		/// </summary>
//		public int MinimumValue { get; set; }
//
//		/// <summary>
//		/// The maximum value that this spin control can have.  Defaults to 1.
//		/// </summary>
//		public int MaximumValue { get; set; }
//
//		public int Height { get; set; }
//
//		public Pigment BarPigment { get; set; }
//
//		public override Size CalculateSize() {
//			return new Size(1, Height);
//		}
//	}
//
//	public class VScrollBar : Control {
//		#region Events
//
//		public event EventHandler ValueChanged;
//
//		#endregion
//
//		private EmitterButton topButton, bottomButton;
//
//		private VerticalValueBar valueBar;
//
//		private int minimumValue;
//
//		/// <summary>
//		/// Get the minimum value that this spin control can have.
//		/// </summary>
//		public int MinimumValue {
//			get { return minimumValue; }
//			set {
//				minimumValue = value;
//				if (valueBar != null)
//					valueBar.MinimumValue = value;
//			}
//		}
//
//		private int maximumValue;
//
//		/// <summary>
//		/// Get the maximum value that this spin control can have.
//		/// </summary>
//		public int MaximumValue {
//			get { return maximumValue; }
//			set {
//				maximumValue = value;
//				if (valueBar != null)
//					valueBar.MaximumValue = value;
//			}
//		}
//
//		/// <summary>
//		/// The delay in milliseconds after first clicking on a spin button before
//		/// the spin cycle starts.  Defaults to 0.
//		/// </summary>
//		protected uint SpinDelay { get; set; }
//
//		/// <summary>
//		/// The speed of the buttons' emit event cycle, in millisecond delay between each event.
//		/// Defaults to 0.
//		/// </summary>
//		protected uint SpinSpeed { get; set; }
//
//		public Pigment BarPigment { get; set; }
//
//		/// <summary>
//		/// Called when this.CurrentValue has changed to a different value.
//		/// </summary>
//		protected virtual void OnValueChanged() {
//			if (ValueChanged != null)
//				ValueChanged(this, EventArgs.Empty);
//		}
//
//		/// <summary>
//		/// Get the current value of the slider.
//		/// </summary>
//		public int CurrentValue {
//			get { return currentValue; }
//
//			set {
//				int newVal = value;
//
//				if (newVal < MinimumValue)
//					newVal = MinimumValue;
//
//				if (newVal > MaximumValue)
//					newVal = MaximumValue;
//
//				if (newVal != currentValue)
//					currentValue = newVal;
//
//				if (valueBar != null)
//					valueBar.CurrentValue = CurrentValue;
//
//				OnValueChanged();
//			}
//		}
//
//		private int currentValue;
//
//		public VScrollBar(VScrollBarTemplate template) : base(template) {
//			MinimumValue = template.MinimumValue;
//			MaximumValue = template.MaximumValue;
//
//			CurrentValue = template.StartingValue;
//			if (CurrentValue < MinimumValue || CurrentValue > MaximumValue)
//				CurrentValue = MinimumValue;
//
//			HasFrame = false;
//			CanHaveKeyboardFocus = false;
//			HilightWhenMouseOver = false;
//
//			BarPigment = template.BarPigment;
//
//			SpinDelay = template.SpinDelay;
//			SpinSpeed = template.SpinSpeed;
//		}
//
//		protected internal override void OnSettingUp() {
//			base.OnSettingUp();
//			
//			int fieldWidth = NumberEntryTemplate.CalculateFieldWidth(MaximumValue, MinimumValue);
//			Size fieldSize = new Size(fieldWidth, 1);
//
//			if (BarPigment == null)
//				BarPigment = DetermineMainPigment();
//
//			valueBar = new VerticalValueBar(new VerticalValueBarTemplate()
//			                                {
//			                                		TopLeftPos = this.LocalToScreen(new Point(0, 0)),
//			                                		Length = this.Size.Height - 2,
//			                                		MaximumValue = this.MaximumValue,
//			                                		MinimumValue = this.MinimumValue,
//			                                		StartingValue = this.CurrentValue,
//			                                		BarPigment = this.BarPigment,
//			                                });
//
//			topButton = new EmitterButton(new EmitterButtonTemplate()
//			                              {
//			                              		HasFrameBorder = false,
//												Label = Char.ToString((char)libtcod.TCODSpecialCharacter.ArrowNorthNoTail),
//			                              		TopLeftPos = this.LocalToScreen(new Point(0, 0)),
//			                              		StartEmittingDelay = SpinDelay,
//			                              		Speed = SpinSpeed
//			                              });
//
//			bottomButton = new EmitterButton(new EmitterButtonTemplate()
//			                                 {
//			                                 		HasFrameBorder = false,
//													Label = Char.ToString((char)libtcod.TCODSpecialCharacter.ArrowSouthNoTail),
//			                                 		TopLeftPos = this.LocalToScreen(new Point(0, 0).Shift(0, Size.Height - 1)),
//			                                 		StartEmittingDelay = SpinDelay,
//			                                 		Speed = SpinSpeed
//			                                 });
//
//			valueBar.MouseMoved += valueBar_MouseMoved;
//
//			valueBar.MouseButtonDown += valueBar_MouseButtonDown;
//
//			topButton.Emit += topButton_Emit;
//			bottomButton.Emit += bottomButton_Emit;
//		}
//
//		protected internal override void OnAdded() {
//			base.OnAdded();
//			ParentWindow.AddControls(valueBar);
//			ParentWindow.AddControls(topButton, bottomButton);
//		}
//
//		protected internal override void OnRemoved() {
//			base.OnRemoved();
//			ParentWindow.RemoveControl(valueBar);
//			ParentWindow.RemoveControl(topButton);
//			ParentWindow.RemoveControl(bottomButton);
//		}
//
//		private void valueBar_MouseMoved(object sender, MouseEventArgs e) {
//			if (e.MouseData.MouseButton == MouseButton.LeftButton) {
//				CurrentValue = CalculateValue(e.MouseData.PixelPosition.Y);
//			}
//		}
//
//		private void valueBar_MouseButtonDown(object sender, MouseEventArgs e) {
//			if (e.MouseData.MouseButton == MouseButton.LeftButton) {
//				CurrentValue = CalculateValue(e.MouseData.PixelPosition.Y);
//			}
//		}
//
//		private void topButton_Emit(object sender, EventArgs e) {
//			if (CurrentValue > MinimumValue)
//				CurrentValue = (CurrentValue - 1);
//		}
//
//		private void bottomButton_Emit(object sender, EventArgs e) {
//			if (CurrentValue < MaximumValue)
//				CurrentValue = (CurrentValue + 1);
//		}
//
//		private int CalculateValue(int pixelPosY) {
//			int charHeight = Canvas.GetCharSize().Height;
//			int currPy = pixelPosY;
//
//			currPy = currPy - (charHeight * valueBar.ScreenRect.Top) - 2 * charHeight;
//
//			int widthInPy = (valueBar.Size.Height - 4) * charHeight;
//
//			float pixposPercent = (float)currPy / (float)widthInPy;
//
//			return (int)((float)(MaximumValue - MinimumValue) * pixposPercent) + MinimumValue;
//		}
//
//		#region		
//		protected override void Dispose(bool isDisposing) {
//			base.Dispose(isDisposing);
//
//			if (isDisposing) {
//
//				if (valueBar != null)
//					valueBar.Dispose();
//
//				if (topButton != null)
//					topButton.Dispose();
//
//				if (bottomButton != null)
//					bottomButton.Dispose();
//			}
//		}
//
//		#endregion
//	}
//}