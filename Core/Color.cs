using System;
using libtcod;

namespace Ogui.Core {
	/// <summary>
	/// This class wraps a TCODColor in an immutable data type.  Provides nearly identical
	/// functionality as TCODColor.
	/// </summary>
	public class Color : IDisposable {
		#region Constructors

		/// <summary>
		/// Constructs a Color from specified tcodColor.  Makes a copy of the tcodColor instead
		/// of keeping a reference.
		/// </summary>
		/// <param name="tcodColor"></param>
		public Color(TCODColor tcodColor) {
			if (tcodColor == null)
				throw new ArgumentNullException("tcodColor");

			_red = tcodColor.Red;
			_green = tcodColor.Green;
			_blue = tcodColor.Blue;

			_color = new TCODColor(_red, _green, _blue);
		}

		/// <summary>
		/// Constructs a Color from the provided reg, green and blue values (0-255 for each)
		/// </summary>
		/// <param name="red"></param>
		/// <param name="green"></param>
		/// <param name="blue"></param>
		public Color(byte red, byte green, byte blue) {
			this._red = red;
			this._green = green;
			this._blue = blue;

			_color = new TCODColor(red, green, blue);
		}

		/// <summary>
		/// Construct color given 3 byte integer (ex. 0xFFFFFF = white)
		/// </summary>
		/// <param name="packedColor"></param>
		public Color(long packedColor) {
			long r, g, b;

			r = packedColor & 0xff0000;
			g = packedColor & 0x00ff00;
			b = packedColor & 0x0000ff;

			r = r >> 16;
			g = g >> 8;


			this._red = (byte) r;
			this._green = (byte) g;
			this._blue = (byte) b;

			_color = new TCODColor(_red, _green, _blue);
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Get the red value of color, 0-255
		/// </summary>
		public byte Red {
			get { return _red; }
		}

		/// <summary>
		/// Get the green value of color, 0-255
		/// </summary>
		public byte Green {
			get { return _green; }
		}

		/// <summary>
		/// Get the blue value of the color, 0-255
		/// </summary>
		public byte Blue {
			get { return _blue; }
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Scales saturation by given amount (0.0 --> 1.0)
		/// Returns new instance - original instance is unchanged
		/// </summary>
		public Color ScaleSaturation(float scale) {
			TCODColor ret = new TCODColor();

			float h, s, v;
			_color.getHSV(out h, out s, out v);

			ret.setHSV(h, s * scale, v);

			return new Color(ret);
		}

		/// <summary>
		/// Scales value (brightness) by given amount (0.0 --> 1.0)
		/// Returns new instance - original instance is unchanged
		/// </summary>
		/// <param name="scale"></param>
		/// <returns></returns>
		public Color ScaleValue(float scale) {
			TCODColor ret = new TCODColor();

			float h, s, v;
			_color.getHSV(out h, out s, out v);

			ret.setHSV(h, s, v * scale);

			return new Color(ret);
		}

		/// <summary>
		/// Replaces hue with given hue (0.0 --> 360.0)
		/// Returns new instance - original instance is unchanged
		/// </summary>
		/// <param name="hue"></param>
		/// <returns></returns>
		public Color ReplaceHue(float hue) {
			TCODColor ret = new TCODColor();

			float h, s, v;
			_color.getHSV(out h, out s, out v);

			ret.setHSV(hue, s, v);

			return new Color(ret);
		}

		/// <summary>
		/// Replaces saturation with given saturation (0.0 --> 1.0)
		/// Returns new instance - original instance is unchanged
		/// </summary>
		/// <param name="saturation"></param>
		/// <returns></returns>
		public Color ReplaceSaturation(float saturation) {
			TCODColor ret = new TCODColor();

			float h, s, v;
			_color.getHSV(out h, out s, out v);

			ret.setHSV(h, saturation, v);

			return new Color(ret);
		}

		/// <summary>
		/// Replaces value (brightness) with given value (0.0 --> 1.0)
		/// Returns new instance - original instance is unchanged
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public Color ReplaceValue(float value) {
			TCODColor ret = new TCODColor();

			float h, s, v;
			_color.getHSV(out h, out s, out v);

			ret.setHSV(h, s, value);

			return new Color(ret);
		}

		/// <summary>
		/// Returns hue (0.0 --> 360.0)
		/// </summary>
		/// <returns></returns>
		public float GetHue() {
			float h, s, v;
			_color.getHSV(out h, out s, out v);

			return h;
		}

		/// <summary>
		/// Returns saturation (0.0 --> 1.0)
		/// </summary>
		/// <returns></returns>
		public float GetSaturation() {
			float h, s, v;
			_color.getHSV(out h, out s, out v);

			return s;
		}

		/// <summary>
		/// Returns value (brightness) (0.0 --> 1.0)
		/// </summary>
		/// <returns></returns>
		public float GetValue() {
			float h, s, v;
			_color.getHSV(out h, out s, out v);

			return v;
		}

		/// <summary>
		/// Converts to a new instance of TCODColor
		/// </summary>
		/// <returns></returns>
		public TCODColor TCODColor {
			get { return new TCODColor(_color.Red, _color.Green, _color.Blue); }
		}

		/// <summary>
		/// Returns a string that will change the foreground color of the text to this color.
		/// </summary>
		/// <returns></returns>
		public string ForegroundCodeString {
			get { return CodeForeground + CodeString; }
		}

		/// <summary>
		/// Returns a string that will change the background color of the text to this color.
		/// </summary>
		/// <returns></returns>
		public string BackgroundCodeString {
			get { return CodeBackground + CodeString; }
		}

		/// <summary>
		/// Returns a string that will set the colors of the text back to default
		/// </summary>
		/// <returns></returns>
		public string DefaultColors {
			get { return Color.StopColorCode; }
		}

		private string CodeString {
			get {
				char r = (char) (Math.Max(this._red, (byte) 1));
				char g = (char) (Math.Max(this._green, (byte) 1));
				char b = (char) (Math.Max(this._blue, (byte) 1));

				string str = r.ToString() + g.ToString() + b.ToString();

				return str;
			}
		}

		/// Returns the foreground color code string.
		public static readonly string CodeForeground = "\x06";

		/// Returns the background color code string.
		public static readonly string CodeBackground = "\x07";

		/// Returns the stop color code string.
		public static readonly string StopColorCode = "\x08";

		public override string ToString() {
			return _red.ToString("x2") + _green.ToString("x2") + _blue.ToString("x2");
		}

		#endregion

		#region Private Fields

		private readonly byte _red, _green, _blue;
		private readonly TCODColor _color;

		#endregion

		#region Public Static

		/// <summary>
		/// Wrapper around TCODColor.Interpolate
		/// </summary>
		/// <param name="sourceColor"></param>
		/// <param name="destinationColor"></param>
		/// <param name="coefficient"></param>
		/// <returns></returns>
		public static Color Lerp(Color sourceColor, Color destinationColor, float coefficient) {
			TCODColor color = TCODColor.Interpolate(sourceColor.TCODColor,
			                                        destinationColor.TCODColor, coefficient);

			return new Color(color);
		}

		#endregion

		#region Dispose

		private bool alreadyDisposed;

		/// <summary>
		/// Default finalizer calls Dispose.
		/// </summary>
		~Color() {
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
				_color.Dispose();
			alreadyDisposed = true;
		}

		#endregion
	}
}