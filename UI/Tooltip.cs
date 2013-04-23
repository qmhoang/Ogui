using System;
using DEngine.Core;

namespace Ogui.UI {
	/// <summary>
	/// Used in TooltipEvent.  Test property can be set to
	/// override the displayed text.
	/// </summary>
	public class TooltipEventArgs : EventArgs {
		/// <summary>
		/// Construct a TooltipEventArgs with specified text string and position
		/// </summary>
		/// <param name="text"></param>
		/// <param name="position"></param>
		public TooltipEventArgs(string text, Point position) {
			this.Text = text;
			this._mousePosition = position;
		}


		/// <summary>
		/// Set this to override the displayed text.
		/// </summary>
		public string Text { get; set; }


		private readonly Point _mousePosition;

		/// <summary>
		/// Get the mouse position related to the Tooltip event, in screen space
		/// coordinates.  This is typically the
		/// origin point of a hover action.
		/// </summary>
		public Point MousePosition {
			get { return _mousePosition; }
		}
	}

	internal class Tooltip : IDisposable {
		public Tooltip(string text, Point sPos, Window parentWindow) {
			_size = new Size(Canvas.TextLength(text) + 2, 3);
			this._parentWindow = parentWindow;

			AutoPosition(sPos);

			_canvas = new Canvas(_size);

			_canvas.SetDefaultPigment(parentWindow.Pigments[PigmentType.Tooltip]);
			_canvas.PrintFrame("");

			_canvas.PrintString(1, 1, text);
		}


		public void DrawToScreen() {
			_canvas.ToScreenAlpha(_sPos, _parentWindow.TooltipFGAlpha,
			                     _parentWindow.TooltipBGAlpha);
		}


		private void AutoPosition(Point nearPos) {
			_sPos = _parentWindow.AutoPosition(nearPos.Shift(2, 2), _size);
		}


		private Canvas _canvas;
		private Size _size;
		private Point _sPos;
		private Window _parentWindow;

		#region Dispose

		private bool alreadyDisposed;

		~Tooltip() {
			Dispose(false);
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool isDisposing) {
			if (alreadyDisposed)
				return;
			if (isDisposing)
				if (_canvas != null)
					_canvas.Dispose();
			alreadyDisposed = true;
		}

		#endregion
	}
}