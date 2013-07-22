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
using libtcod;

namespace Ogui.UI {
	/// <summary>
	/// Handles all the input polling and message dispatch to the attached
	/// Window.  An InputManager is contained in and controlled by an Application object.
	/// </summary>
	public class InputManager {
		#region Constructors

		/// <summary>
		/// Create an InputManager instance by providing the attached Window.
		/// </summary>
		public InputManager(Component component) {
			if (component == null)
				throw new ArgumentNullException("component");

			_attachedComponent = component;
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Called by the Application on each update tick to perform input polling.  The InputManager instance
		/// will send the appropriate user input messages to the attached window provided
		/// during construction.
		/// </summary>
		/// <param name="elapsed"></param>
		public void Update(uint elapsed) {
			PollMouse(elapsed);
			PollKeyboard();
		}

		/// <summary>
		/// Returns true if the specified <paramref name="key"/> is currently being
		/// pushed.  This method is here for non-standard use of the framework - it is
		/// typically better to use the normal keyboard messaging system provided by
		/// components.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public static bool IsKeyDown(TCODKeyCode key) {
			return TCODConsole.isKeyPressed(key);
		}

		#endregion

		#region Private Fields

		private readonly Component _attachedComponent;
		private Point _lastMousePosition;
		private Point _lastMousePixelPosition;
		private MouseButton _lastMouseButton;
		private float _lastMouseMoveTime;
		private bool _isHovering;
		private Point _startLBDown;
		private bool _isDragging;

		#endregion

		#region Private Constants

		// NOTE: consider making these configurable instead of constants
		private const int DragPixelTol = 24;
		private const float HoverMSTol = 600f;
		private const int DelayUntilNextClick = 100;

		#endregion

		#region Keyboard Input

		private void PollKeyboard() {
			TCODKey key = TCODConsole.checkForKeypress((int) TCODKeyStatus.KeyPressed
			                                           | (int) TCODKeyStatus.KeyReleased);

			if (key.KeyCode != TCODKeyCode.NoKey)
				if (key.Pressed)
					_attachedComponent.OnKeyPressed(new KeyboardData(key));
				else
					_attachedComponent.OnKeyReleased(new KeyboardData(key));
		}

		#endregion

		#region Mouse Input

		private void PollMouse(uint elapsedTime) {
			MouseData mouse = new MouseData(TCODMouse.getStatus());

			CheckMouseButtons(mouse);

			// check for mouse move
			//if (mouse.Position != lastMousePosition)
			//{
			//    DoMouseMove(mouse);

			//    lastMousePosition = mouse.Position;
			//    lastMouseMoveTime = totalElapsed;
			//}
			if ((mouse.PixelPosition != _lastMousePixelPosition)) {
				DoMouseMove(mouse);

				_lastMousePosition = mouse.Position;
				_lastMousePixelPosition = mouse.PixelPosition;
				_lastMouseMoveTime = elapsedTime;
			}

			// check for hover
			if (_isHovering == false)
				StartHover(mouse);
		}


		private void CheckMouseButtons(MouseData mouse) {
			if (mouse.MouseButton != _lastMouseButton) {
				if (_lastMouseButton == MouseButton.None)
					DoMouseButtonDown(mouse);
				else
					DoMouseButtonUp(new MouseData(_lastMouseButton, mouse.Position, mouse.PixelPosition));

				_lastMouseButton = mouse.MouseButton;
			}
		}


		private void StartHover(MouseData mouse) {
			_attachedComponent.OnMouseHoverBegin(mouse);

			_isHovering = true;
		}


		private void StopHover(MouseData mouse) {
			_attachedComponent.OnMouseHoverEnd(mouse);

			_isHovering = false;
		}


		private void StartDrag(MouseData mouse) {
			_isDragging = true;

			// TODO fix this, it does not pass origin of drag as intended
			_attachedComponent.OnMouseDragBegin(mouse.Position);
		}


		private void StopDrag(MouseData mouse) {
			_isDragging = false;

			_attachedComponent.OnMouseDragEnd(mouse.Position);
		}


		private void DoMouseMove(MouseData mouse) {
			StopHover(mouse);

			_attachedComponent.OnMouseMoved(mouse);

			// check for BeginDrag
			if (mouse.MouseButton == MouseButton.LeftButton) {
				int delta = Math.Abs(mouse.PixelPosition.X - _startLBDown.X) +
				            Math.Abs(mouse.PixelPosition.Y - _startLBDown.Y);

				if (delta > DragPixelTol && _isDragging == false)
					StartDrag(mouse);
			}
		}


		private void DoMouseButtonDown(MouseData mouse) {
			if (_isDragging)
				StopDrag(mouse);

			if (mouse.MouseButton == MouseButton.LeftButton)
				_startLBDown = mouse.PixelPosition;

			_attachedComponent.OnMouseButtonDown(mouse);
		}


		private void DoMouseButtonUp(MouseData mouse) {
			if (_isDragging)
				StopDrag(mouse);

			_attachedComponent.OnMouseButtonUp(mouse);
		}

		#endregion
	}
}