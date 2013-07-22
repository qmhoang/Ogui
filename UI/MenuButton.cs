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
using System.Linq;
using DEngine.Core;

namespace Ogui.UI {
	public class MenuButtonTemplate : ButtonTemplate {
		public List<string> Items { get; set; }
		/// <summary>
		/// The initial index selected, defaults to 0, if greater than maximum number of items will be equal to maximum.  If negative, will be 0
		/// </summary>
		public int InitialSelection { get; set; }

		public MenuButtonTemplate() {
			Items = new List<string>();

			if (InitialSelection >= Items.Count)
				InitialSelection = Items.Count - 1;
			if (InitialSelection < 0)
				InitialSelection = 0;
		}

		/// <summary>
		/// Auto generates the size of the button based on the other options.
		/// </summary>
		/// <returns></returns>
		public override Size CalculateSize() {
			if (AutoSizeOverride.IsEmpty) {
				int len = Canvas.TextLength(Label) + Items.Max(s => Canvas.TextLength(s)) + 2;
				int width = len;
				int height = 1;

				if (HasFrameBorder) {
					width += 2;
					height += 2;
				}

				return new Size(Math.Max(width, MinimumWidth), height);
			} else
				return AutoSizeOverride;
		}
	}

	public class MenuButton : Button {
		/// <summary>
		/// Get the index of the item currently selected.
		/// </summary>
		public int CurrentSelected { get; protected set; }

		/// <summary>
		/// Get the label of the current selected item.
		/// </summary>
		public string CurrentSelectedData {
			get { return _items[CurrentSelected]; }
		}

		private string _labelSelection;
		private Rectangle _labelSelectionRect;
		private Menu _menu;
		private List<string> _items;

		public event EventHandler<MenuItemSelectedEventArgs> SelectionChanged;


		public MenuButton(MenuButtonTemplate template)
				: base(template) {
			_items = template.Items;
			_menu = new Menu(new MenuTemplate()
			                {
			                		TopLeftPos = template.CalculateRect().BottomLeft,
			                		Items = _items.Select(buttonLabel => new MenuItemData(buttonLabel)).ToList()
			                });

			_menu.ItemSelected += menu_ItemSelected;

			CurrentSelected = template.InitialSelection;
			_labelSelection = Label + ": " + _items[CurrentSelected];

			_labelSelectionRect = new Rectangle(Point.Origin, Size);

			if (template.HasFrameBorder &&
			    Size.Width > 2 &&
			    Size.Height > 2)
				_labelSelectionRect = _labelSelectionRect.Inflate(-1, -1);
		}

		protected override void Redraw() {
			base.Redraw();
			if (!OwnerDraw)
				Canvas.PrintStringAligned(_labelSelectionRect,
				                          _labelSelection,
				                          LabelAlignment,
				                          VAlignment);
		}

		protected internal override void OnMouseButtonDown(MouseData mouseData) {
			base.OnMouseButtonDown(mouseData);
			if (mouseData.MouseButton == MouseButton.LeftButton) {
				CurrentSelected = (CurrentSelected + 1) % _items.Count;
				menu_ItemSelected(this, new MenuItemSelectedEventArgs(CurrentSelected));
			} else if (mouseData.MouseButton == MouseButton.RightButton) {
				_menu.ScreenPosition = mouseData.Position;
				ParentWindow.AddControl(_menu);				
			}
		}

		private void menu_ItemSelected(object sender, MenuItemSelectedEventArgs e) {
			CurrentSelected = e.Index;
			_labelSelection = Label + ": " + _items[CurrentSelected];

			EventHandler<MenuItemSelectedEventArgs> handler = SelectionChanged;
			if (handler != null)
				handler(this, e);
		}
	}
}