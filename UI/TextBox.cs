using System;
using System.Collections.Generic;
using DEngine.Core;
using Ogui.Core;

namespace Ogui.UI {
	public class TextBoxTemplate : PanelTemplate {
		public uint TextSpeed { get; set; }
	}

	public class TextBox : Panel {
		public TextBox(TextBoxTemplate template)
				: base(template) {
			//OwnerDraw = true;
			TextSpeed = template.TextSpeed;
			_buffer = new Queue<Atom>();
		}

		public int NumberOfLines { get; private set; }

		public int LineLength { get; private set; }

		public uint TextSpeed {
			get { return _textSpeed; }
			set {
				_textSpeed = value;

				if (ContainsSchedule(_typeSchedule)) {
					RemoveSchedule(_typeSchedule);

					_typeSchedule = new Schedule(TypeNextChar, _textSpeed);
					AddSchedule(_typeSchedule);
				}
			}
		}

		// bug must be called AFTER adding the control or there will be problems, need to fix
		public void AddText(string text, Pigment pigment = null) {
			string[] words = Explode(text);

			foreach (string word in words)
				if (!string.IsNullOrEmpty(word)) {
					if ((_currVirtualPos + Canvas.TextLength(word) >= LineLength) &&
					    (word[0] != '\n')) {
						_buffer.Enqueue(new Atom('\n'));
						_currVirtualPos = 0;
					}

					foreach (char c in word) {
						_buffer.Enqueue(new Atom(c, pigment));
						_currVirtualPos++;

						if (c == '\n')
							_currVirtualPos = 0;
					}

					if (_currVirtualPos != 0) {
						_buffer.Enqueue(new Atom(' ', pigment));
						_currVirtualPos++;
					}
				}
		}

		public void AddText(string text, Color foreground) {
			if (foreground == null)
				throw new ArgumentNullException("foreground");

			Pigment pigment = DetermineMainPigment().ReplaceForeground(foreground);

			AddText(text, pigment);
		}

		protected internal override void OnSettingUp() {
			base.OnSettingUp();

			Rectangle textRect;

			if (HasFrame)
				textRect = this.LocalRect.Inflate(-1, -1);
			else
				textRect = this.LocalRect;

			NumberOfLines = textRect.Size.Height;

			LineLength = textRect.Size.Width;

			_currLine = 0;
			_currPos = 0;
			_currVirtualPos = _currPos;

			_typeSchedule = new Schedule(TypeNextChar, TextSpeed);
			AddSchedule(_typeSchedule);

			_textCanvas = new Canvas(textRect.Size);
			_textCanvas.SetDefaultPigment(DetermineMainPigment());
			_textCanvas.Clear();

			_textCanvasPos = textRect.TopLeft;
		}


		protected override void Redraw() {
			base.Redraw();

			Canvas.Blit(_textCanvas, _textCanvasPos);
		}

		protected override Pigment DetermineMainPigment() {
			return base.DetermineMainPigment();
		}
		
		protected override void Dispose(bool isDisposing) {
			base.Dispose(isDisposing);

			if (_alreadyDisposed)
				return;
			if (isDisposing)
				if (_textCanvas != null)
					_textCanvas.Dispose();
			_alreadyDisposed = true;
		}

		private void TypeNextChar() {
			if (_buffer.Count == 0)
				return;

			Atom next = _buffer.Dequeue();

			if (next.c == '\n') {
				_currPos = 0;
				_currLine++;

				if (_currLine >= NumberOfLines) {
					this._textCanvas.Scroll(0, -1);
					_currLine--;
					_currPos = 0;
				}
			} else {
				if (_currPos < LineLength && _currLine < NumberOfLines) {
					_textCanvas.SetDefaultPigment(DetermineMainPigment());
					_textCanvas.PrintChar(_currPos, _currLine, next.c, next.pigment);
				}
				_currPos++;
			}
		}

		private string[] Explode(string text) {
			//text = text.Replace("\n", " ");
			text = text.Replace("\r", " ");
			text = text.Replace(".", ". ");
			text = text.Replace("\t", " ");
			text = text.Replace(",", ", ");
			text = text.Replace(";", "; ");
			text = text.Replace("  ", " ");

			string[] Words = text.Split(' ');

			return Words;
		}

		#region Private
		private bool _alreadyDisposed;
		private Schedule _typeSchedule;

		private int _currPos;
		private int _currLine;

		private Point _textCanvasPos;

		private Canvas _textCanvas;

		private int _currVirtualPos;

		private uint _textSpeed;
		
		private readonly Queue<Atom> _buffer;
		#endregion

		private class Atom {
			public Atom(char c, Pigment pigment = null) {
				this.c = c;
				this.pigment = pigment;
			}

			public char c;
			public Pigment pigment;
		}
	}
}