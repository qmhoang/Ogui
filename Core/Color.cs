using System;
using libtcod;

namespace Ogui.Core {
	/// <summary>
	/// This class wraps a TCODColor.  Provides nearly identical
	/// functionality as TCODColor.  Should autobox a TCODColor also.
	/// </summary>
	public sealed class Color : IDisposable {
		#region Constructors

		/// <summary>
		/// Constructs a Color from specified tcodColor.  Makes a copy of the tcodColor instead
		/// of keeping a reference.
		/// </summary>
		/// <param name="tcodColor"></param>
		public Color(TCODColor tcodColor) {
			if (tcodColor == null)
				throw new ArgumentNullException("tcodColor");

			color = tcodColor;
		}

		/// <summary>
		/// Constructs a Color from the provided reg, green and blue values (0-255 for each)
		/// </summary>
		/// <param name="red"></param>
		/// <param name="green"></param>
		/// <param name="blue"></param>
		public Color(byte red, byte green, byte blue) {
			color = new TCODColor(red, green, blue);
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
			
			color = new TCODColor(r, g, b);
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Get the red value of color, 0-255
		/// </summary>
		public byte Red { get { return color.Red; } }

		/// <summary>
		/// Get the green value of color, 0-255
		/// </summary>
		public byte Green { get { return color.Green; } }

		/// <summary>
		/// Get the blue value of the color, 0-255
		/// </summary>
		public byte Blue { get { return color.Blue; } }

		#endregion

		#region Public Methods

		public static implicit operator TCODColor(Color color) {
			return color.color;
		}

		/// <summary>
		/// Scales saturation by given amount (0.0 --> 1.0)
		/// </summary>
		public Color ScaleSaturation(float scale) {
			color.scaleHSV(scale, 1.0f);
			return this;		
		}

		/// <summary>
		/// Scales value (brightness) by given amount (0.0 --> 1.0)
		/// </summary>
		/// <param name="scale"></param>
		/// <returns></returns>
		public Color ScaleValue(float scale) {
			color.scaleHSV(1.0f, scale);
			return this;			
		}

		/// <summary>
		/// Replaces hue with given hue (0.0 --> 360.0)
		/// </summary>
		/// <param name="hue"></param>
		/// <returns></returns>
		public Color ReplaceHue(float hue) {
			color.setHue(hue);
			return this;
		}

		/// <summary>
		/// Replaces saturation with given saturation (0.0 --> 1.0)
		/// </summary>
		/// <param name="saturation"></param>
		/// <returns></returns>
		public Color ReplaceSaturation(float saturation) {
			color.setSaturation(saturation);
			return this;
		}

		/// <summary>
		/// Replaces value (brightness) with given value (0.0 --> 1.0)
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public Color ReplaceValue(float value) {
			color.setValue(value);
			return this;
		}

		/// <summary>
		/// Returns hue (0.0 --> 360.0)
		/// </summary>
		/// <value></value>
		public float Hue {
			get { return color.getHue(); }
			set { ReplaceHue(value); }
		}

		/// <summary>
		/// Returns saturation (0.0 --> 1.0)
		/// </summary>
		/// <value></value>
		public float Saturation {
			get { return color.getSaturation(); }
			set { ReplaceSaturation(value); }
		}

		/// <summary>
		/// Returns value (brightness) (0.0 --> 1.0)
		/// </summary>
		/// <value></value>
		public float Value {
			get { return color.getValue(); }
			set { ReplaceValue(value); }
		}

		/// <summary>
		/// Converts to a new instance of TCODColor
		/// </summary>
		/// <returns></returns>
		public TCODColor TCODColor {
			get { return color; }
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
				char r = (char) (Math.Max(this.Red, (byte) 1));
				char g = (char) (Math.Max(this.Green, (byte) 1));
				char b = (char) (Math.Max(this.Blue, (byte) 1));

				string str = r.ToString() + g.ToString() + b.ToString();

				return str;
			}
		}

		/// Returns the foreground color code string.
		public const string CodeForeground = "\x06";

		/// Returns the background color code string.
		public const string CodeBackground = "\x07";

		/// Returns the stop color code string.
		public const string StopColorCode = "\x08";

		public override string ToString() {
			return Red.ToString("x2") + Green.ToString("x2") + Blue.ToString("x2");
		}

		#endregion

		#region Private Fields

		private readonly TCODColor color;

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
		private void Dispose(bool isDisposing) {
			if (alreadyDisposed)
				return;
			if (isDisposing)
				color.Dispose();
			alreadyDisposed = true;
		}

		#endregion
	}
}