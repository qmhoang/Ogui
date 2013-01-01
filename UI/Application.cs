using System;
using System.Linq;
using System.Collections.Generic;
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
			FpsLimit = 60;
			InitialDelay = 100;
			IntervalDelay = 75;
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
		/// Any pigments added to this dictionary will override the defaults for this application
		/// and all child widgets.  Use this to set application-wide pigments.
		/// </summary>
		public PigmentAlternatives Pigments { get; set; }

		/// <summary>
		/// Limits the framerate per limit, defaults to 60
		/// </summary>
		public int FpsLimit { get; set; }

		public int InitialDelay { get; set; }

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
		private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
			windowStack = new List<Window>();
		}

		#endregion

		#region Public Properties

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

			while (windowStack.Count > 0) {
				windowStack.ForEach(w =>
				                    {
				                    	w.OnQuitting();
				                    	w.Dispose();
				                    });

				windowStack.Clear();
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
		public static Rect ScreenRect {
			get { return new Rect(Point.Origin, ScreenSize); }
		}


		/// <summary>
		/// Get the Application's current window.
		/// </summary>
		public Window CurrentWindow {
			get { return windowStack[windowStack.Count - 1]; }
		}


		private readonly List<Window> windowStack;

		/// <summary>
		/// Pushes a window into the top of the stack, which will immediately begin to receive
		/// framework messages.  The stack can be popped and the window below the current stack 
		/// will then be on top.    If the specified window
		/// has not yet received a SettingUp message (i.e. it has not already been set as
		/// an application window previously), then OnSettingUp will be called.
		/// </summary>
		/// <param name="win"></param>
		public void Push(Window win) {
			Logger.InfoFormat("Pushing window {0}", win.GetType());
			if (win == null)
				throw new ArgumentNullException("win");
			if (windowStack.Contains(win))
				throw new ArgumentException("Window already exist in the stack", "win");


//            Input = new InputManager(win);
			win.ParentApplication = this;
			win.Pigments = new PigmentMap(this.Pigments,
			                              win.PigmentOverrides);

			if (!win.isSetup)
				win.OnSettingUp();
			windowStack.Add(win);
			win.OnAdded();
		}

//        private void RemoveWindow(Window window) {
//            Logger.InfoFormat("Removing {0}", window.GetType());
//            windowStack.Remove(window);
//            window.OnRemoved();            
//        }
//
//        /// <summary>
//        /// Pop the current state and change into another one
//        /// </summary>
//        /// <param name="newState"></param>
//        private void ChangeState(Window newState) {
//            Logger.InfoFormat("Changing current window to {0}", newState.GetType());            
//            RemoveWindow(CurrentWindow);
//            Push(newState);
//        }

		public int StateCount {
			get { return windowStack.Count; }
		}

		#endregion

		#region Protected Properties

//        /// <summary>
//        /// Get the current InputManager for the current Window.
//        /// </summary>
//        protected InputManager Input { get; private set; }

		#endregion

		#region Protected Methods

		/// <summary>
		/// Called after Application.Start has been called.  Override and place application specific
		/// setup code here after calling base method.
		/// </summary>
		/// <param name="info"></param>
		protected virtual void Setup(ApplicationInfo info) {
			if (!string.IsNullOrEmpty(info.Font))
				TCODConsole.setCustomFont(info.Font,
				                          (int) info.FontFlags);

			TCODConsole.initRoot(info.ScreenSize.Width, info.ScreenSize.Height, info.Title,
			                     info.Fullscreen, TCODRendererType.SDL);
			TCODSystem.setFps(info.FpsLimit);
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
		protected virtual void Update() {
			foreach (var window in windowStack.Where(window => window.WindowState == WindowState.Quitting)) {
				Logger.InfoFormat("Removing {0}", window.GetType());
				window.OnRemoved();
			}

			windowStack.RemoveAll(win => win.WindowState == WindowState.Quitting);

			if (UpdateEventHandler != null)
				UpdateEventHandler(this, EventArgs.Empty);

			uint elapsed = TCODSystem.getElapsedMilli();

			foreach (var window in windowStack)
				if (window.IsActive)
					window.Update();

			CurrentWindow.Input.Update(elapsed);
		}

		#endregion

		#region Private

		private int Run() {
			if (StateCount <= 0) {
				Window win = new Window(new WindowTemplate());
				Push(win);
			}

			while (!TCODConsole.isWindowClosed() && !IsQuitting) {
				Update();
				Draw();
			}

			return 0;
		}


		private void Draw() {
			var reverse = new Stack<Window>();

			for (int i = windowStack.Count - 1; i >= 0; i--) {
				reverse.Push(windowStack[i]);
				if (!windowStack[i].IsPopup)
					break;
			}

			TCODConsole.root.clear();
			foreach (var window in reverse)
				window.OnDraw();
			TCODConsole.flush();
		}

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
				while (windowStack.Count > 0) {
					windowStack.ForEach(w => w.Dispose());

					windowStack.Clear();
				}
//                if(CurrentWindow != null)
//                    CurrentWindow.Dispose();
			alreadyDisposed = true;
		}

		#endregion
	}

	#endregion
}