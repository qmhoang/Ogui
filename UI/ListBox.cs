using System;
using System.Collections.Generic;
using System.Linq;
using DEngine.Core;
using Ogui.Core;
using libtcod;

namespace Ogui.UI {

	#region ListBox Helper Classes

	/// <summary>
	/// This is the argument sent as part of a ListBox.ItemSelected event.
	/// </summary>
	public class ListItemSelectedEventArgs : EventArgs {
		/// <summary>
		/// Construct a ListItemSelectedEventArgs object with the specified item index number.
		/// </summary>
		/// <param name="index"></param>
		public ListItemSelectedEventArgs(int index) {
			Index = index;
		}

		/// <summary>
		/// The index of the selected item.
		/// </summary>
		public int Index { get; private set; }
	}

	/// <summary>
	/// Contains the label and tooltip text for each Listitem that will be added
	/// to a Listbox.
	/// </summary>
	public class ListItemData {
		/// <summary>
		/// Construct a ListItemData instance given the label and an optional tooltip.
		/// </summary>
		/// <param name="label"></param>
		/// <param name="toolTip"></param>
		public ListItemData(string label, string toolTip = null) {
			this.Label = label;
			this.TooltipText = toolTip;
		}

		/// <summary>
		/// The label of this list item.
		/// </summary>
		public string Label { get; set; }

		/// <summary>
		/// The optional tooltip text for this list item.
		/// </summary>
		public string TooltipText { get; set; }
	}

	#endregion

	#region ListBoxTemplate

	/// <summary>
	/// This class builds on the Control Template, and adds options specific to a ListBox.
	/// </summary>
	public class ListBoxTemplate : ControlTemplate {
		/// <summary>
		/// Default constructor initializes properties to their defaults.
		/// </summary>
		public ListBoxTemplate() {
			Items = new List<ListItemData>();
			Title = "";
			LabelAlignment = HorizontalAlignment.Left;
			TitleAlignment = HorizontalAlignment.Center;
			InitialSelectedIndex = 0;
			CanHaveKeyboardFocus = false;
			HilightWhenMouseOver = false;
			HasFrameBorder = true;
			FrameTitle = false;
		}

		/// <summary>
		/// The list of ListItemData elements that will be included in the list box.  Defaults
		/// to an empty list.
		/// </summary>
		public List<ListItemData> Items { get; set; }

		/// <summary>
		/// The horizontal alignment of the item labels.  Defaults to left.
		/// </summary>
		public HorizontalAlignment LabelAlignment { get; set; }

		/// <summary>
		/// The horiontal alignment of the title. Defaults to left.
		/// </summary>
		public HorizontalAlignment TitleAlignment { get; set; }

		/// <summary>
		/// The title string, defaults to ""
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// The list box width if larger than the calculated width.  Defaults to 0.
		/// </summary>
		public int MinimumListBoxWidth { get; set; }

		/// <summary>
		/// Which item index will be selected initially.  Defaults to 0.
		/// </summary>
		public int InitialSelectedIndex { get; set; }

		/// <summary>
		/// Specifies if this control can receive the keyboard focus.  Defaults to false.
		/// </summary>
		public bool CanHaveKeyboardFocus { get; set; }

		/// <summary>
		/// Specifies if this control is drawn in hilighted colors when under the mouse pointer.
		/// Defaults to false.
		/// </summary>
		public bool HilightWhenMouseOver { get; set; }

		/// <summary>
		/// Use this to manually size the ListBox.  If this is empty (the default), then the
		/// ListBox will autosize.
		/// </summary>
		public Size AutoSizeOverride { get; set; }

		/// <summary>
		/// If true, a frame will be drawn around the listbox and between the title and list
		/// of items.  If autosizing, the required space for the frame element will be added.
		/// Defaults to true.
		/// </summary>
		public bool HasFrameBorder { get; set; }

		/// <summary>
		/// Smaller version which folds the title into the frame. If there is no frames,
		/// it has no affect on the listbox.  Defaults to false.
		/// </summary>
		public bool FrameTitle { get; set; }


		/// <summary>
		/// Calculates the ListBox size based on the properties of this template.
		/// </summary>
		/// <returns></returns>
		public override Size CalculateSize() {
			if (AutoSizeOverride.Width > 0 && AutoSizeOverride.Height > 0)
				return AutoSizeOverride;

			int width = Title.Length;
			foreach (ListItemData i in Items) {
				if (i.Label == null)
					i.Label = "";

				if (Canvas.TextLength(i.Label) > width)
					width = Canvas.TextLength(i.Label);
			}

			width += 2;

			if (HasFrameBorder)
				width += 2;

			if (this.MinimumListBoxWidth > width)
				width = MinimumListBoxWidth;

			int height = Items.Count + 1;

			if (HasFrameBorder)
				if (FrameTitle)
					height += 1;
				else
					height += 3;

			return new Size(width, height);
		}
	}

	#endregion

	#region ListBox

	/// <summary>
	/// A ListBox control allows the selection of a single option among a list of
	/// options presented in rows.  The selection state of an item is persistant, and
	/// is marked as currently selected.
	/// </summary>
	public class ListBox : Control {
		#region Events

		/// <summary>
		/// Raised when an item has been selected by the left mouse button.
		/// </summary>
		public event EventHandler<ListItemSelectedEventArgs> ItemSelected;

		#endregion

		#region Constructors

		/// <summary>
		/// Construct a ListBox instance from the given template.
		/// </summary>
		/// <param name="template"></param>
		public ListBox(ListBoxTemplate template)
				: base(template) {
			Items = template.Items;
			Title = template.Title;
			if (Title == null)
				Title = "";

			CurrentSelected = -1;
			OwnerDraw = template.OwnerDraw;

			if (this.Size.Width < 3 || this.Size.Height < 3)
				template.HasFrameBorder = false;

			HasFrame = template.HasFrameBorder;
			useSmallVersion = template.FrameTitle;
			HilightWhenMouseOver = template.HilightWhenMouseOver;
			CanHaveKeyboardFocus = template.CanHaveKeyboardFocus;

			LabelAlignment = template.LabelAlignment;
			TitleAlignment = template.TitleAlignment;
			CurrentSelected = template.InitialSelectedIndex;

			mouseOverIndex = -1;
			topIndex = 0;

			CalcMetrics(template);
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// The horizontal alignment of the item labels.
		/// </summary>
		public HorizontalAlignment LabelAlignment { get; set; }

		/// <summary>
		/// The horiontal alignment of the title.
		/// </summary>
		public HorizontalAlignment TitleAlignment { get; set; }

		/// <summary>
		/// The title string.
		/// </summary>
		public string Title { get; private set; }

		/// <summary>
		/// Get the index of the item currently selected.
		/// </summary>
		public int CurrentSelected { get; protected set; }

		/// <summary>
		/// Get the label of the current selected item.
		/// </summary>
		public string CurrentSelectedData {
			get { return Items[CurrentSelected].Label; }
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Returns the label of the item with the specified index.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public string GetItemLabel(int index) {
			if (index < 0 || index >= Items.Count)
				throw (new ArgumentOutOfRangeException("index"));

			return Items[index].Label;
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Draws the title and title frame.
		/// </summary>
		protected void DrawTitle() {
			if (!useSmallVersion || !HasFrame) {
				if (!string.IsNullOrEmpty(Title)) {
					Canvas.PrintStringAligned(titleRect, Title, TitleAlignment,
					                          VerticalAlignment.Center);
				}

				if (HasFrame &&
				    this.Size.Width > 2 &&
				    this.Size.Height > 2) {
					int fy = titleRect.Bottom;

					Canvas.SetDefaultPigment(DetermineFramePigment());
					Canvas.DrawHLine(1, fy, Size.Width - 2);
					Canvas.PrintChar(0, fy, (int) TCODSpecialCharacter.TeeEast);
					Canvas.PrintChar(Size.Width - 1, fy, (int) TCODSpecialCharacter.TeeWest);
				}
			}
		}


		/// <summary>
		/// Draws each of the items in the list.
		/// </summary>
		protected void DrawItems() {
			for (int i = topIndex; i < numberItemsDisplayed + topIndex; i++)
				DrawItem(i);

		}


		/// <summary>
		/// Draws a single item with the given index.
		/// </summary>
		/// <param name="index"></param>
		protected void DrawItem(int index) {
			ListItemData item = Items[index];

			if (index == CurrentSelected) {
				Canvas.PrintStringAligned(itemsRect.TopLeft.X,
				                          itemsRect.TopLeft.Y + index - topIndex,
				                          item.Label,
				                          LabelAlignment,
										  itemsRect.Size.Width - (HasFrame ? 1 : 0),
				                          Pigments[PigmentType.ViewSelected]);

				Canvas.PrintChar(itemsRect.TopRight.X - 1,
				                 itemsRect.TopLeft.Y + index - topIndex,
				                 (int) TCODSpecialCharacter.ArrowWest,
				                 Pigments[PigmentType.ViewSelected]);
			} else if (index == mouseOverIndex)
				Canvas.PrintStringAligned(itemsRect.TopLeft.X,
										  itemsRect.TopLeft.Y + index - topIndex,
				                          item.Label,
				                          LabelAlignment,
										  itemsRect.Size.Width - (HasFrame ? 1 : 0),
				                          Pigments[PigmentType.ViewHilight]);
			else
				Canvas.PrintStringAligned(itemsRect.TopLeft.X,
										  itemsRect.TopLeft.Y + index - topIndex,
				                          item.Label,
				                          LabelAlignment,
										  itemsRect.Size.Width - (HasFrame ? 1 : 0),
				                          Pigments[PigmentType.ViewNormal]);
		}


		/// <summary>
		/// Returns the index of the item that contains the provided point, specified in local
		/// space coordinates.  Returns -1 if no items are at that position.
		/// </summary>
		/// <param name="lPos"></param>
		/// <returns></returns>
		protected int GetItemAt(Point lPos) {
			int index = -1;

			if (itemsRect.Contains(lPos)) {
				int i = lPos.Y - itemsRect.Top;
				index = i;
			}

			if (index < 0 || index >= Items.Count)
				index = -1;
			return index + topIndex;
		}

		#endregion

		#region Message Handlers

		protected internal override void OnSettingUp() {
			base.OnSettingUp();

			if (Items.Count > numberItemsDisplayed) {
				var height = Size.Height;
				var topLeftPos = ScreenRect.TopRight.Shift(-1, 0);
				if (HasFrame) {
					height -= 2;
//					topLeftPos.X--;
//					topLeftPos.Y++;
					topLeftPos = topLeftPos.Shift(-1, 1);
				}
				if (!useSmallVersion) {
					height -= 2;
//					topLeftPos.Y += 2;
					topLeftPos = topLeftPos.Shift(0, 2);
				}
				scrollBar = new VScrollBar(new VScrollBarTemplate()
				                           {
				                           		Height = height,
				                           		MinimumValue = 0,
				                           		MaximumValue = height,
				                           		StartingValue = 0,
				                           		TopLeftPos = topLeftPos,
				                           		SpinDelay = 100,
				                           		SpinSpeed = 100,
				                           });
				scrollBar.ValueChanged += scrollBar_ValueChanged;
			}

		}

		protected internal override void OnAdded() {
			base.OnAdded();
			if (scrollBar != null)
				ParentWindow.AddControl(scrollBar);
		}

		protected internal override void OnRemoved() {
			base.OnRemoved();
			ParentWindow.RemoveControl(scrollBar);
		}

		/// <summary>
		/// Draws the title and items.  Override to add custom drawing code.
		/// </summary>
		protected override void Redraw() {
			base.Redraw();

			DrawTitle();
			DrawItems();
		}

		protected override void DrawFrame(Pigment pigment = null) {
			if (this.Size.Width > 2 && this.Size.Height > 2)
				Canvas.PrintFrame(useSmallVersion ? Title : null, pigment);
		}

		/// <summary>
		/// Base method detects if the mouse is over one of the items, and changes state
		/// accordingly.  Override to add custom handling.
		/// </summary>
		/// <param name="mouseData"></param>
		protected internal override void OnMouseMoved(MouseData mouseData) {
			base.OnMouseMoved(mouseData);

			Point lPos = ScreenToLocal(mouseData.Position);

			mouseOverIndex = GetItemAt(lPos);

			if (mouseOverIndex != -1)
				TooltipText = Items[mouseOverIndex].TooltipText;
			else
				TooltipText = null;
		}
		
		/// <summary>
		/// Detects which, if any, item has been selected by a left mouse button.  Override
		/// to add custom handling.
		/// </summary>
		/// <param name="mouseData"></param>
		protected internal override void OnMouseButtonDown(MouseData mouseData) {
			base.OnMouseButtonDown(mouseData);

			if (mouseOverIndex != -1)
				if (CurrentSelected != mouseOverIndex) {
					CurrentSelected = mouseOverIndex;
					OnItemSelected(CurrentSelected);
				}
		}


		/// <summary>
		/// Called when one of the items in the list has been selected with the left mouse
		/// button.  Base method triggers appropriate event.  Override to add custom handling.
		/// </summary>
		/// <param name="index"></param>
		protected virtual void OnItemSelected(int index) {
			if (ItemSelected != null)
				ItemSelected(this, new ListItemSelectedEventArgs(index));
		}

		#endregion

		#region Private

		private List<ListItemData> Items;
		private int mouseOverIndex;
		private Rectangle titleRect;
		private Rectangle itemsRect;
		private int numberItemsDisplayed;
		private bool useSmallVersion;

		private int topIndex;
		private VScrollBar scrollBar;
		
		private void CalcMetrics(ListBoxTemplate template) {
			int numItems = Items.Count;
			int expandTitle = 0;

			int delta = Size.Height - numItems - 1;
			if (template.HasFrameBorder) {
				if (template.FrameTitle)
					delta -= 1;
				else
					delta -= 3;
			}

			numberItemsDisplayed = Items.Count;
			if (delta < 0)
				numberItemsDisplayed += delta;
			else if (delta > 0)
				expandTitle = delta;

			int titleWidth = Size.Width;

			int titleHeight = 1 + expandTitle;

			if (Title != "")
				if (template.HasFrameBorder)
					titleRect = new Rectangle(Point.One,
					                     new Size(titleWidth - 2, titleHeight));
				else
					titleRect = new Rectangle(Point.Origin,
					                     new Size(titleWidth, titleHeight));

			int itemsWidth = Size.Width;
			int itemsHeight = numberItemsDisplayed;

			if (template.HasFrameBorder)
				if (template.FrameTitle)
					itemsRect = new Rectangle(Point.One, new Size(itemsWidth - 2, itemsHeight));
				else
					itemsRect = new Rectangle(titleRect.BottomLeft.Shift(0, 1), new Size(itemsWidth - 2, itemsHeight));
			else
				itemsRect = new Rectangle(titleRect.BottomLeft,
				                     new Size(itemsWidth, itemsHeight));
		}

		#endregion

		void scrollBar_ValueChanged(object sender, EventArgs e) {
			topIndex = scrollBar.CurrentValue;
		}
	}

	#endregion
}