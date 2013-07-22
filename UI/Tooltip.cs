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