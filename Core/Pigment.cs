﻿// Copyright (c) 2010-2013 Shane Baker, Quang-Minh Hoang
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
		public Pigment(Color foreground, Color background, TCODBackgroundFlag bgFlag) {
			_fgColor = foreground;
			_bgColor = background;
			this._bgFlag = bgFlag;
		}

		/// <summary>
		/// BGFlag defaults to TCODBackgroundFlag.Set
		/// </summary>
		public Pigment(Color foreground, Color background)
				: this(foreground, background, TCODBackgroundFlag.Set) {}

		/// <summary>
		/// Construct a Pigment given foreground and background colors and background flag.
		/// </summary>
		public Pigment(long foreground, long background, TCODBackgroundFlag bgFlag)
				: this(new Color(foreground), new Color(background), bgFlag) {}

		/// <summary>
		/// BGFlag defaults to TCODBackgroundFlag.Set
		/// </summary>
		public Pigment(long foreground, long background)
				: this(foreground, background, TCODBackgroundFlag.Set) {}

		#endregion

		#region Public Properties

		/// <summary>
		/// Get the foreground color
		/// </summary>
		public Color Foreground {
			get { return _fgColor; }
		}

		/// <summary>
		/// Get the background color
		/// </summary>
		public Color Background {
			get { return _bgColor; }
		}

		/// <summary>
		/// Get the background flag;
		/// </summary>
		public TCODBackgroundFlag BackgroundFlag {
			get { return _bgFlag; }
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

		private readonly Color _fgColor;
		private readonly Color _bgColor;
		private readonly TCODBackgroundFlag _bgFlag;

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
				_bgColor.Dispose();
				_fgColor.Dispose();
			}
			alreadyDisposed = true;
		}

		#endregion
	}
}