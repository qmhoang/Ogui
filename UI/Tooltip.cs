using System;
using DEngine.Core;

namespace Ogui.UI
{
	/// <summary>
	/// Used in TooltipEvent.  Test property can be set to
	/// override the displayed text.
	/// </summary>
	public class TooltipEventArgs : EventArgs
	{
 
		/// <summary>
		/// Construct a TooltipEventArgs with specified text string and position
		/// </summary>
		/// <param name="text"></param>
		/// <param name="position"></param>
		public TooltipEventArgs(string text, Point position)
		{
			this.Text = text;
			this.mousePosition = position;
		}
 

 
		/// <summary>
		/// Set this to override the displayed text.  
		/// </summary>
		public string Text { get; set; }

 
		readonly Point mousePosition;
		/// <summary>
		/// Get the mouse position related to the Tooltip event, in screen space
		/// coordinates.  This is typically the
		/// origin point of a hover action.
		/// </summary>
		public Point MousePosition { get { return mousePosition; } }
 
	}

	internal class Tooltip : IDisposable
	{
 
		public Tooltip(string text, Point sPos, Window parentWindow)
		{
			size = new Size(Canvas.TextLength(text) + 2, 3);
			this.parentWindow = parentWindow;

			AutoPosition(sPos);

			canvas = new Canvas(size);

			canvas.SetDefaultPigment(parentWindow.Pigments[PigmentType.Tooltip]);
			canvas.PrintFrame("");

			canvas.PrintString(1, 1, text);
		}
 

 
		public void DrawToScreen()
		{
			canvas.ToScreenAlpha(sPos, parentWindow.TooltipFGAlpha, 
				parentWindow.TooltipBGAlpha);
		}
 

 
		private void AutoPosition(Point nearPos)
		{
			sPos = parentWindow.AutoPosition(nearPos.Shift(2,2), size);
		}
 

 
		private Canvas canvas;
		private Size size;
		private Point sPos;
		private Window parentWindow;
 
		#region Dispose
		private bool alreadyDisposed;

		~Tooltip()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool isDisposing)
		{
			if (alreadyDisposed)
				return;
			if (isDisposing)
			{
				if(canvas != null)
					canvas.Dispose();
			}
			alreadyDisposed = true;
		}
		#endregion
	}
}
