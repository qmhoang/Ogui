using System;
using DEngine.Core;

namespace Ogui.UI {
	/// <summary>
	/// Represents a check box control.  A CheckBox has a label and a checkable element that displays the 
	/// current state of the IsChecked property.  This state
	/// is toggled by left mouse button clicks, or by setting the IsChecked property manually.
	/// </summary>
	public class CheckBox : Control {
		#region Events

		/// <summary>
		/// Raised when the state of a checkbox has been toggled by user input.  Get IsChecked to get
		/// current state.  Manually setting IsChecked property will not cause this event to be raised.
		/// </summary>
		public event EventHandler CheckBoxToggled;

		#endregion

		#region Constructors

		/// <summary>
		/// Construct a CheckBox instance from the given template.
		/// </summary>
		public CheckBox() {
			HasFrame = true;

			if (Size.Height < 3 || Size.Width < 3)
				HasFrame = false;

			HilightWhenMouseOver = false;
			CanHaveKeyboardFocus = false;

			this.Label = String.Empty;
			
			this.CheckOnLeft = true;
			this.LabelAlignment = HorizontalAlignment.Left;
			this.VerticalAlign = VerticalAlignment.Center;

			CalcMetrics();
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Get or set the current checked state of the checkbox.  Setting this property will
		/// not raise a CheckBoxToggled event.
		/// </summary>
		public bool IsChecked {
			get { return isChecked; }
			set { isChecked = value; }
		}

		/// <summary>
		/// Get the label string
		/// </summary>
		public string Label {
			get { return label; }
			private set { label = value; }
		}

		/// <summary>
		/// True if the check element appears left of the label, otherwise on right side
		/// </summary>
		public bool CheckOnLeft { get; private set; }

		/// <summary>
		/// Text alignment of the label
		/// </summary>
		public HorizontalAlignment LabelAlignment { get; private set; }

		/// <summary>
		/// The vertical alignment of the label and check element.
		/// </summary>
		public VerticalAlignment VerticalAlign { get; private set; }

		#endregion

		#region Message Handlers

		/// <summary>
		/// Triggers appropriate events based on a mouse button down action.  Override to
		/// add custom mouse button handling code.
		/// </summary>
		/// <param name="mouseData"></param>
		protected internal override void OnMouseButtonDown(MouseData mouseData) {
			base.OnMouseButtonDown(mouseData);

			if (mouseData.MouseButton == MouseButton.LeftButton) {
				if (IsChecked)
					IsChecked = false;
				else
					IsChecked = true;

				if (CheckBoxToggled != null)
					CheckBoxToggled(this, EventArgs.Empty);
			}
		}


		/// <summary>
		/// Draws the label and chec element based on current state.  Override to add custom
		/// drawing code.
		/// </summary>
		protected override void Redraw() {
			base.Redraw();

			if (!string.IsNullOrEmpty(Label))
				Canvas.PrintStringAligned(labelRect,
				                          Label,
				                          LabelAlignment,
				                          VerticalAlign);

			if (IsActive)
				if (IsChecked)
					Canvas.PrintChar(checkPos, 225, Pigments[PigmentType.ViewNormal]);
				else
					Canvas.PrintChar(checkPos, 224, Pigments[PigmentType.ViewNormal]);
		}

		#endregion

		#region Private 

		//private int labelPosX;
		//private int labelFieldLength;
		//private int labelPosY;
		private Rectangle labelRect;
		private Point checkPos;

		private bool isChecked;
		private string label;

		private void CalcMetrics() {
			Rectangle inner = this.LocalRect;

			if (HasFrame && Size.Height >= 3)
				inner = inner.Inflate(-1, -1);

			int checkX;

			if (CheckOnLeft) {
				checkX = inner.Left;
				labelRect = new Rectangle(inner.TopLeft.Shift(1, 0),
				                     inner.BottomRight.Shift(-1, -1));
			} else {
				checkX = inner.Right - 1;
				labelRect = new Rectangle(inner.TopLeft,
				                     inner.BottomRight.Shift(-2, -1));
			}

			if (labelRect.Size.Width < 1)
				Label = String.Empty;

			switch (VerticalAlign) {
				case VerticalAlignment.Bottom:
					checkPos = new Point(checkX, labelRect.Bottom - 1);
					break;

				case VerticalAlignment.Center:
					checkPos = new Point(checkX, labelRect.Center.Y);
					break;

				case VerticalAlignment.Top:
					checkPos = new Point(checkX, labelRect.Top);
					break;
			}
		}

		#endregion
	}
}