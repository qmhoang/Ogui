using System;
using libtcod;

namespace Ogui.Core {
	/// <summary>
	/// Stores forground color, background color, and background flag in a convenient
	/// single immutable data type
	/// </summary>
	public class Pigment : IDisposable {
		#region Constructors

		/// <summary>
		/// Construct a Pigment given foreground and background colors and background flag
		/// </summary>
		/// <param name="foreground"></param>
		/// <param name="background"></param>
		/// <param name="bgFlag"></param>
		public Pigment(Color foreground, Color background, TCODBackgroundFlag bgFlag = TCODBackgroundFlag.Set) {
			fgColor = foreground;
			bgColor = background;
			this.bgFlag = bgFlag;
		}

		/// <summary>
		/// Construct a Pigment given foreground and background colors and background flag.
		/// </summary>
		public Pigment(long foreground, long background, TCODBackgroundFlag bgFlag = TCODBackgroundFlag.Set)
				: this(new Color(foreground), new Color(background), bgFlag) {}

		#endregion

		#region Public Properties

		/// <summary>
		/// Get the foreground color
		/// </summary>
		public Color Foreground {
			get { return fgColor; }
		}

		/// <summary>
		/// Get the background color
		/// </summary>
		public Color Background {
			get { return bgColor; }
		}

		/// <summary>
		/// Get the background flag;
		/// </summary>
		public TCODBackgroundFlag BackgroundFlag {
			get { return bgFlag; }
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Swaps a Pigments's foreground and background.  Returns a new Pigment instance,
		/// this instance is unchanged.
		/// </summary>
		/// <returns></returns>
		public Pigment Invert() {
			return new Pigment(Background, Foreground);
		}

		/// <summary>
		/// Returns a new Pigment by replacing the foreground color.  This isntance remains
		/// unchanged.
		/// </summary>
		/// <param name="newFGColor"></param>
		/// <returns></returns>
		public Pigment ReplaceForeground(Color newFGColor) {
			return new Pigment(newFGColor, Background);
		}

		/// <summary>
		/// Returns a new Pigment by replacing the background color.  This isntance remains
		/// unchanged.
		/// </summary>
		/// <param name="newBGColor"></param>
		/// <returns></returns>
		public Pigment ReplaceBackground(Color newBGColor) {
			return new Pigment(Foreground, newBGColor);
		}

		/// <summary>
		/// Returns a new Pigment by replacing the background flag.  This isntance remains
		/// unchanged.
		/// </summary>
		/// <param name="newBGFlag"></param>
		/// <returns></returns>
		public Pigment ReplaceBGFlag(TCODBackgroundFlag newBGFlag) {
			return new Pigment(Foreground, Background, newBGFlag);
		}

		/// <summary>
		/// Returns the embedded string code for this color.
		/// <note>Embedded colors are currently not working correctly</note>
		/// </summary>
		/// <returns></returns>
		public string GetCode() {
			string str = string.Format("{0}{1}",
			                           Foreground.ForegroundCodeString,
			                           Background.BackgroundCodeString);

			return str;
		}

		public override string ToString() {
			return string.Format("{0},{1}", Foreground.ToString(), Background.ToString());
		}

		#endregion

		#region Private Fields

		private readonly Color fgColor;
		private readonly Color bgColor;
		private readonly TCODBackgroundFlag bgFlag;

		#endregion

		#region Dispose

		private bool alreadyDisposed;

		/// <summary>
		/// Default finalizer calls Dispose.
		/// </summary>
		~Pigment() {
			Dispose(false);
		}

		/// <summary>
		/// Safely dispose this object and its contents.
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
			if (isDisposing) {
				bgColor.Dispose();
				fgColor.Dispose();
			}
			alreadyDisposed = true;
		}

		#endregion
	}
}