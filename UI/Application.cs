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
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using DEngine.Core;
using libtcod;

namespace Ogui.UI {

	#region ApplicationInfo

	/// <summary>
	/// This class holds the application options passed to Application.Start().
	/// </summary>
	public class ApplicationInfo {
		/// <summary>
		/// Default constructor, sets data to defaults
		/// </summary>
		public ApplicationInfo() {
			Fullscreen = false;
			ScreenSize = new Size(80, 60);
			Title = "";
			Font = null;
			FontFlags = TCODFontFlags.LayoutAsciiInColumn;
			Pigments = new PigmentAlternatives();
			UpdatesPerSecondLimit = 60;
			InitialDelay = 100;
			IntervalDelay = 75;
			RendererType = TCODRendererType.SDL;
		}

		/// <summary>
		/// True if fulscreen.  Defaults to false
		/// </summary>
		public bool Fullscreen { get; set; }

		/// <summary>
		/// The size of the screen.  This sets the screen resolution if Fullscreen is true,
		/// otherwise affacts the system window size.  Defaults to 80x60 characters.
		/// </summary>
		public Size ScreenSize { get; set; }

		/// <summary>
		/// The title of the system window.
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// The name of the font file to use, which must be in the same path as the executable.
		/// </summary>
		public string Font { get; set; }

		/// <summary>
		/// Information about the specified font as per TCODFontFlags.
		/// </summary>
		public TCODFontFlags FontFlags { get; set; }

		/// <summary>
		/// What renderer will libtcod use, defaults to the SDL.
		/// </summary>
		public TCODRendererType RendererType { get; set; }

		/// <summary>
		/// Any pigments added to this dictionary will override the defaults for this application
		/// and all child widgets.  Use this to set application-wide pigments.
		/// </summary>
		public PigmentAlternatives Pigments { get; set; }

		/// <summary>
		/// Limits the number of times update is called per second, defaults to 60.
		/// </summary>
		public int UpdatesPerSecondLimit { get; set; }

		/// <summary>
		/// Limits the number of times draw is called per second, defaults to 60.
		/// </summary>
		public int FpsLimit { get; set; }

		/// <summary>
		/// Delay in millisecond between the time when a key is pressed, and keyboard repeat begins.  If 0, keyboard repeat is disabled.  Defaults to 100.
		/// </summary>
		public int InitialDelay { get; set; }

		/// <summary>
		/// Interval in millisecond between keyboard repeat events, defaults to 75
		/// </summary>
		public int IntervalDelay { get; set; }
	}

	#endregion

	#region Application Class

	/// <summary>
	/// Represents the entire application, and controls top-level logic and state.  The Application
	/// contains <strike>a Window</strike> a stack which maintains several windows (which is a container 
	/// for all of the controls.), each of which can be, active or inactive<para>This object, of which there
	/// is only one being executed, handles libtcod initialization, encapsulates the main application loop,
	/// and is the ultimate origin for all top level messages</para>
	/// <remarks>A custom class should be derived from Application to, at minimal, implement setup code by
	/// overriding OnSetup.  Call Application.Start to initialize and start the application loop, which will
	/// continue until IsQuitting is set to true.</remarks>
	/// </summary>
	public class Application : IDisposable {
		#region Events

		/// <summary>
		/// Raised when the application is setting up.  This is raised after TCODInitRoot() has
		/// been called, so place any intitialization code dependant on libtcod being initialized here.
		/// This event is provided in case the
		/// framework is being used in a non-standard way - typically, the derived class will place top level
		/// setup code in an overriden Setup method.
		/// </summary>
		public event EventHandler SetupEventHandler;

		/// <summary>
		/// Raised each iteration of the main application loop.  This event is provided in case the
		/// framework is being used in a non-standard way - typically, the derived class will place top level
		/// logic updating in an overriden Update method, or within a custom Window class.
		/// </summary>
		public event EventHandler UpdateEventHandler;

		#endregion

		#region Constructors

		/// <summary>
		/// Default constructor.
		/// </summary>
		public Application() {
			IsQuitting = false;
			_windowsStack = new List<Window>();
			_windowsToUpdate = new List<Window>();
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Limits the number of times update is called per second, defaults to 60.
		/// </summary>
		public int UpdatesPerSecondLimit { get; private set; }

		/// <summary>
		/// Limits the number of times draw is called per second, defaults to 60.
		/// </summary>
		public int FpsLimit { get; private set; }

		/// <summary>
		/// True if the application wants to quit.  Set to true to quit.
		/// </summary>
		public bool IsQuitting { get; set; }

		/// <summary>
		/// Holds the map of pigments for this application.  Use this to make application-wide
		/// changes to pigments.
		/// </summary>
		public PigmentMap Pigments { get; protected set; }

		#endregion

		#region Public Methods

		/// <summary>
		/// Initializes libtcod and starts the application's main loop.  This will loop 
		/// until IsQuitting is set to true or the main system window is closed.
		/// </summary>
		/// <param name="setupInfo">An ApplicationInfo object containing the options specific
		/// to this application</param>
		public void Start(ApplicationInfo setupInfo) {
			Setup(setupInfo);

			Run();

			while (_windowsStack.Count > 0) {
				_windowsStack.ForEach(w =>
				{
					w.OnQuitting();
					w.Dispose();
				});

				_windowsStack.Clear();
			}
		}


		/// <summary>
		/// Gets the size of TCODConsole.root, which is the size of the screen (or system window)
		/// in cells.
		/// </summary>
		public static Size ScreenSize {
			get { return new Size(TCODConsole.root.getWidth(), TCODConsole.root.getHeight()); }
		}


		/// <summary>
		/// Gets a Rect representing the screen (or the system window).  The UpperLeft position
		/// will always be the origin (0,0).
		/// </summary>
		public static Rectangle ScreenRect {
			get { return new Rectangle(Point.Origin, ScreenSize); }
		}


		/// <summary>
		/// Get the Application's current window.
		/// </summary>
		public Window CurrentWindow {
			get { return _windowsStack[_windowsStack.Count - 1]; }
		}


		/// <summary>
		/// Pushes a window into the top of the stack, which will immediately begin to receive
		/// framework messages.  The stack can be popped and the window below the current stack 
		/// will then be on top.    If the specified window
		/// has not yet received a SettingUp message (i.e. it has not already been set as
		/// an application window previously), then OnSettingUp will be called.
		/// </summary>
		/// <param name="win"></param>
		public void Push(Window win) {
			if (win == null)
				throw new ArgumentNullException("win");
			if (_windowsStack.Contains(win))
				throw new ArgumentException("Window already exist in the stack", "win");

			win.ParentApplication = this;
			win.Pigments = new PigmentMap(this.Pigments,
										  win.PigmentOverrides);

			if (!win.IsSetup)
				win.OnSettingUp();
			_windowsStack.Add(win);
			win.OnAdded();
		}

		public int StateCount {
			get { return _windowsStack.Count; }
		}

		#endregion

		#region Protected Properties

		#endregion

		#region Protected Methods
		/// <summary>
		/// Called after Application.Start has been called.  Override and place application specific
		/// setup code here after calling base method.
		/// </summary>
		/// <param name="info"></param>
		protected virtual void Setup(ApplicationInfo info) {
			if (!string.IsNullOrEmpty(info.Font))
				TCODConsole.setCustomFont(info.Font, (int)info.FontFlags);

			FpsLimit = info.FpsLimit;
			_fpsFrameLength = FpsLimit == 0 ? 0 : MilliSecondsPerSecond / FpsLimit;
			_lastDrawMilli = 0;

			UpdatesPerSecondLimit = info.UpdatesPerSecondLimit;
			_upsFrameLength = UpdatesPerSecondLimit == 0 ? 0 : MilliSecondsPerSecond / UpdatesPerSecondLimit;
			_lastUpdateMilli = 0;

			_delayFps = FpsLimit > UpdatesPerSecondLimit;

			TCODSystem.setFps(FpsLimit);

			TCODConsole.initRoot(info.ScreenSize.Width, info.ScreenSize.Height, info.Title,
								 info.Fullscreen, info.RendererType);
			TCODConsole.setKeyboardRepeat(info.InitialDelay, info.IntervalDelay);

			TCODMouse.showCursor(true);

			if (SetupEventHandler != null)
				SetupEventHandler(this, EventArgs.Empty);

			Pigments = new PigmentMap(DefaultPigments.FrameworkDefaults,
									  info.Pigments);
		}


		/// <summary>
		/// Called each iteration of the main loop (each frame).  
		/// Override and add specific logic update code after calling base method.
		/// </summary>
		/// <param name="elapsedTime">Elapsed time in milliseconds since last time update was called</param>
		protected virtual void Update(uint elapsedTime) {
			foreach (var window in _windowsStack) {
				if (window.WindowState == WindowState.Quitting)
					window.OnRemoved();
			}

			_windowsStack.RemoveAll(win => win.WindowState == WindowState.Quitting);

			if (UpdateEventHandler != null)
				UpdateEventHandler(this, EventArgs.Empty);

			_windowsToUpdate.Clear();

			foreach (var window in _windowsStack) {
				_windowsToUpdate.Add(window);
			}

			foreach (var window in _windowsToUpdate) {
				if (window.IsActive)
					window.Update();
			}

			CurrentWindow.Input.Update(elapsedTime);
		}

		#endregion

		#region Private
		private int Run() {
			if (StateCount <= 0) {
				Window win = new Window(new WindowTemplate());
				Push(win);
			}

			while (!TCODConsole.isWindowClosed() && !IsQuitting) {
				var newUpdateMilli = TCODSystem.getElapsedMilli();
				var elapsedUpdateTime = newUpdateMilli - _lastUpdateMilli;
//				if (elapsedUpdateTime > upsFrameLength) {
					_lastUpdateMilli = newUpdateMilli;
					Update(elapsedUpdateTime);
//				}

				var newDrawMilli = TCODSystem.getElapsedMilli();
				var elapsedDrawTime = newDrawMilli - _lastDrawMilli;
//				if (elapsedDrawTime > fpsFrameLength) {
					_lastDrawMilli = newDrawMilli;
					Draw(elapsedDrawTime);
//				}
			}

			return 0;
		}


		private void Draw(uint elapsedTime) {
			TCODConsole.root.clear();
			foreach (var window in _windowsStack)
				window.OnDraw();
			TCODConsole.flush();
		}

		private uint _lastDrawMilli;
		private int _fpsFrameLength;

		private uint _lastUpdateMilli;
		private int _upsFrameLength;

		private const int MilliSecondsPerSecond = 1000;

		private readonly List<Window> _windowsStack;
		private readonly List<Window> _windowsToUpdate;

		private bool _delayFps;
		#endregion

		#region Dispose

		private bool alreadyDisposed;

		/// <summary>
		/// Default finalizer calls Dispose.
		/// </summary>
		~Application() {
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
				while (_windowsStack.Count > 0) {
					_windowsStack.ForEach(w => w.Dispose());

					_windowsStack.Clear();
				}
			alreadyDisposed = true;
		}

		#endregion
	}

	#endregion
}