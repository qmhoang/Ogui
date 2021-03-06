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
	//public interface ITemplate
	//{
	//    Size CalculateSize();
	//}

	/// <summary>
	/// The abstract base class for widget templates.  When subclassing a type of Widget, consider
	/// also subclassing WidgetTemplate to provide an interface for the client to specify
	/// options, and override CalculateSize to ensure that the widget is created with the correct
	/// size.
	/// </summary>
	public abstract class WidgetTemplate {
		/// <summary>
		/// Default constructor initializes properties to their defaults.
		/// </summary>
		protected WidgetTemplate() {
			OwnerDraw = false;
			Pigments = new PigmentAlternatives();
		}

		/// <summary>
		/// If true, then base classes will not do any drawing to the canvas, including clearing
		/// or blitting to the screen.  This property is present so that subclasses can implement
		/// specialized drawing code for optimization or that would otherwise be difficult to 
		/// implement using overrides/events.  Defaults to false.
		/// </summary>
		public bool OwnerDraw { get; set; }

		/// <summary>
		/// Any pigments added to this dictionary will override the default pigments for the widget.
		/// Use this to define custom pigments for a widget and any of its children.
		/// Defaults to an empty collection.
		/// </summary>
		public PigmentAlternatives Pigments { get; set; }

		/// <summary>
		/// An override of this method should return the exact and final size of the widget.  This size is
		/// used during the contruction of an object from a template.
		/// </summary>
		public abstract Size CalculateSize();
	}

	/// <summary>
	/// Base class for any component that gets drawn on the screen.  A widget provides
	/// a Canvas for drawing operations.
	/// </summary>
	public abstract class Widget : Component, IDisposable {
		#region Events

		/// <summary>
		/// Raised when the the Widget receives a draw message from the framework.  Subscribers
		/// can perform custom drawing when this is raised.
		/// </summary>
		public event EventHandler Draw;

		#endregion

		#region Constructors

		/// <summary>
		/// Construct a Widget instance from the given template.
		/// </summary>
		/// <param name="template"></param>
		protected Widget(WidgetTemplate template) {
			this.Position = new Point(0, 0);
			this.Size = template.CalculateSize();
			this.Canvas = new Canvas(Size);

			this.OwnerDraw = template.OwnerDraw;

			this.PigmentOverrides = template.Pigments.Copy();
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Returns widget's rect in screen space coordinates
		/// </summary>
		public virtual Rectangle ScreenRect {
			get { return new Rectangle(ScreenPosition, Size); }
		}

		/// <summary>
		/// Returns the widget's rect in local space coordinates.  The TopLeft coordinate
		/// will always be the Origin (0,0).
		/// </summary>
		public virtual Rectangle LocalRect {
			get { return new Rectangle(Point.Origin, Size); }
		}

		/// <summary>
		/// Get the Canvas object associated with this widget.
		/// </summary>
		public Canvas Canvas { get; private set; }

		/// <summary>
		/// Get the the size of the widget.
		/// </summary>
		public Size Size { get; private set; }

		/// <summary>
		/// Get the pigment map for this widget.  Alternatives can be set or removed
		/// to change the pigments for this widget and its children during runtime.
		/// </summary>
		public PigmentMap Pigments { get; internal set; }

		#endregion

		#region Protected Properties

		/// <summary>
		/// The upper left position of this widget in screen space coordinates.
		/// </summary>
		protected internal virtual Point ScreenPosition {
			get { return Position; }
			set { Position = value; }
		}

		/// <summary>
		/// If true, then base classes will not do any drawing to the canvas, including clearing
		/// or blitting to the screen.  This property is present so that subclasses can implement
		/// specialized drawing code for optimization or that would otherwise be difficult to 
		/// implement using overrides/events.
		/// </summary>
		protected bool OwnerDraw { get; set; }

		#endregion

		#region Protected Methods

		/// <summary>
		/// If OwnerDraw is false, this base method clears the Canvas with the Pigment
		/// returned from DetermineMainPigment.
		/// </summary>
		protected virtual void Redraw() {
			if (!OwnerDraw) {
				Canvas.SetDefaultPigment(DetermineMainPigment());
				Canvas.Clear();
			}
		}

		/// <summary>
		/// Calculates the current Pigment of the main drawing area for the widget.  Override to change
		/// which pigment is used.
		/// </summary>
		/// <returns></returns>
		protected abstract Pigment DetermineMainPigment();

		/// <summary>
		/// Calculate and return the current Pigment of the frame area for this widget.
		/// Override to change which pigment is used.
		/// </summary>
		/// <returns></returns>
		protected abstract Pigment DetermineFramePigment();

		#endregion

		#region Message Handlers

		/// <summary>
		/// Called during the drawing phase of the application loop.  Base method calls Redraw(), 
		/// triggers the Draw event, and blits the canvas to the screen if OwnerDraw is false.
		/// This method should rarely need to be overriden - instead, to provide custom drawing code
		/// (whether OwnerDraw is true or false), override Redraw(), DetermineMainPigment(), and DetermineFramePigment().
		/// </summary>
		protected internal virtual void OnDraw() {
			Redraw();

			if (Draw != null)
				Draw(this, EventArgs.Empty);

			if (!OwnerDraw)
				Canvas.ToScreen(this.ScreenPosition);
		}

		#endregion

		#region Internal

		internal PigmentAlternatives PigmentOverrides { get; set; }
		internal Point Position;

		#endregion

		#region Dispose

		private bool alreadyDisposed;

		/// <summary>
		/// Default finalizer calls Dispose.
		/// </summary>
		~Widget() {
			Dispose(false);
		}

		/// <summary>
		/// Safely dispose this object and all of its contents.
		/// </summary>
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Override to add custom disposing code.
		/// </summary>
		/// <param name="isDisposing"></param>
		protected virtual void Dispose(bool isDisposing) {
			if (alreadyDisposed)
				return;
			if (isDisposing)
				if (Canvas != null)
					Canvas.Dispose();
			alreadyDisposed = true;
		}

		#endregion
	}
}