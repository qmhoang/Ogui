using System;
using DEngine.Core;
using Ogui.Core;

namespace Ogui.UI {

	#region Button Class

	/// <summary>
	/// Represents a button control.  A button can be pushed, which happens when the left mouse button is 
	/// pressed then subsequently released while over the button.
	/// </summary>
	public class Button : Control {
		#region Events

		/// <summary>
		/// Raised when a button has been pushed (mouse button down then up over control).
		/// </summary>
		public event EventHandler ButtonPushed;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructs a Button instance given the template.
		/// </summary>
		public Button() {
			this.Label = String.Empty;
			this.LabelAlignment = HorizontalAlignment.Left;
			HilightWhenMouseOver = true;
			HasFrame = true;
			CanHaveKeyboardFocus = false;
			
			VAlignment = VerticalAlignment.Center;
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Get the button Label.  Defaults to empty string ""
		/// </summary>
		public string Label {
			get { return label; }
			set {
				label = value;

				if (AutoSize) {
					int len = Canvas.TextLength(Label);
					int width = len;
					int height = 1;

					if (HasFrame) {
						width += 2;
						height += 2;
					}

					Size = new Size(width, height);
				}
			}
		}

		/// <summary>
		/// Get or set the label's horizontal alignment.  Defaults to HorizontalAlignment.Left.
		/// </summary>
		public HorizontalAlignment LabelAlignment { get; set; }

		/// <summary>
		/// Get or set the label's vertical alignment.  This will only have an effect if
		/// the height of the button is larger than 3 as specified by the AutoSizeOverride
		/// property of the creating template.  Defaults to VerticalAlignment.Center.
		/// </summary>
		protected VerticalAlignment VAlignment { get; set; }

		#endregion

		#region  Protected Methods

		/// <summary>
		/// This base method clears the Canvas, draws the frame (if any), and draws the label, unless
		/// OwnerDraw is set to true in which case the base methods do nothing.  Override to add custom
		/// drawing code here.
		/// </summary>
		protected override void Redraw() {
			base.Redraw();
			if (!OwnerDraw) {
				if (HasFrame &&
				this.Size.Width > 2 &&
				this.Size.Height > 2) {
					Canvas.PrintStringAligned(new Rectangle(Point.One, new Size(Size.Width - 2, Size.Height - 2)),
					                          Label,
					                          LabelAlignment,
					                          VAlignment);
				} else {
					Canvas.PrintStringAligned(new Rectangle(Point.Origin, Size),
											  Label,
											  LabelAlignment,
											  VAlignment);
				}
			}
				
		}


		/// <summary>
		/// Returns the pigment of the main control area based on its current state.
		/// Override to return a custom color for the main drawing area of the button, or to add
		/// additional colors for the button based on custom states.
		/// </summary>
		protected override Pigment DetermineMainPigment() {
			if (IsActive && IsBeingPushed)
				return Pigments[PigmentType.ViewDepressed];
			return base.DetermineMainPigment();
		}

		/// <summary>
		/// Returns the pigment of the frame based on the current state.
		/// Override to return a custom color for the frame, or to add additional colors
		/// for the button based on custom states.
		/// </summary>
		/// <returns></returns>
		protected override Pigment DetermineFramePigment() {
			if (IsActive && IsBeingPushed)
				return Pigments[PigmentType.FrameDepressed];
			return base.DetermineFramePigment();
		}

		#endregion

		#region Message Handlers
		protected internal override void OnSizeChanged() {
			base.OnSizeChanged();


		}
		/// <summary>
		/// Called when a mouse button is released while over this button.  Triggers proper
		/// events.  Override to add custom handling.
		/// </summary>
		/// <param name="mouseData"></param>
		protected internal override void OnMouseButtonUp(MouseData mouseData) {
			bool wasBeingPushed = IsBeingPushed; // store, since base call will reset this to false

			base.OnMouseButtonUp(mouseData);

			if (mouseData.MouseButton == MouseButton.LeftButton && wasBeingPushed)
				OnButtonPushed();
		}


		/// <summary>
		/// Called by the framework when a buton click
		/// action is performed.  Triggers proper
		/// events.  Override to add custom handling.
		/// </summary>
		protected virtual void OnButtonPushed() {
			if (ButtonPushed != null)
				ButtonPushed(this, EventArgs.Empty);
		}

		#endregion

		#region Private
		private string label;
		#endregion
	}

	#endregion
}