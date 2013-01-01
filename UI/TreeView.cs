using System;
using System.Collections.Generic;
using System.Text;
using DEngine.Core;
using Ogui.Core;
using libtcod;

namespace Ogui.UI {
	#region ListBox Helper Classes
	/// <summary>
	/// Contains the label and tooltip text for each Listitem that will be added
	/// to a Listbox.
	/// </summary>
	public class TreeNode {
		/// <summary>
		/// Construct a ListItemData instance given the label and an optional tooltip.
		/// </summary>
		/// <param name="label"></param>
		/// <param name="toolTip"></param>
		public TreeNode(string label, string toolTip = null) {
			this.Label = label;
			this.TooltipText = toolTip;
			nodes = new List<TreeNode>();
			Parent = null;
			Depth = 0;
			expanded = true;
		}

		/// <summary>
		/// The label of this list item.
		/// </summary>
		public virtual string Label { get; set; }

		/// <summary>
		/// The optional tooltip text for this list item.
		/// </summary>
		public virtual string TooltipText { get; set; }

		private List<TreeNode> nodes;

		public IEnumerable<TreeNode> Nodes { get { return nodes; } }

		public TreeNode Parent { get; protected set; }

		public void AddChild(TreeNode node) {
			nodes.Add(node);
			node.Parent = this;
			node.Depth = this.Depth + 1;
		}

		public void RemoveChild(TreeNode node) {
			nodes.Remove(node);
			node.Parent = null;
		}

		public bool IsRoot {
			get { return Parent == null; }
		}

		public bool HasChildren {
			get { return nodes.Count > 0; }
		}

		public int Depth { get; private set; }

		private bool expanded;

		public bool Expanded {
			get { return expanded; }
			set {
				if (HasChildren)
					expanded = value;
			}
		}        
	}
	#endregion


	#region TreeViewTemplate
	/// <summary>
	/// This class builds on the Control Template, and adds options specific to a ListBox.
	/// </summary>
	public class TreeViewTemplate : ControlTemplate {
		/// <summary>
		/// Default constructor initializes properties to their defaults.
		/// </summary>
		public TreeViewTemplate() {
			Items = new List<TreeNode>();
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
		public List<TreeNode> Items { get; set; }

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
			if (AutoSizeOverride.Width > 0 && AutoSizeOverride.Height > 0) {
				return AutoSizeOverride;
			}

			int width = Title.Length;
			int height = 1;

			Queue<TreeNode> nodesToProcess = new Queue<TreeNode>();

			foreach (var node in Items) {
				nodesToProcess.Enqueue(node);
			}

			while (nodesToProcess.Count > 0) {
				var treeNode = nodesToProcess.Dequeue();

				if (treeNode.Label == null)
					treeNode.Label = "";

				if (Canvas.TextLength(treeNode.Label) + treeNode.Depth + 1 > width)
					width = Canvas.TextLength(treeNode.Label) + treeNode.Depth + 1;

				if (treeNode.HasChildren)
					foreach (var node in treeNode.Nodes) {
						nodesToProcess.Enqueue(node);
					}
				height++;
			}

			if (HasFrameBorder)
				width += 2;

			if (this.MinimumListBoxWidth > width)
				width = MinimumListBoxWidth;



			if (HasFrameBorder)
				if (FrameTitle)
					height += 1;
				else
					height += 3;

			return new Size(width, height);
		}

	}
	#endregion


	#region TreeView
	/// <summary>
	/// A ListBox control allows the selection of a single option among a list of
	/// options presented in rows.  The selection state of an item is persistant, and
	/// is marked as currently selected.
	/// </summary>
	public class TreeView : Control {
		#region Events

		/// <summary>
		/// Raised when an item has been selected by the left mouse button.
		/// </summary>
		public event EventHandler<EventArgs<TreeNode>> ItemSelected;

		#endregion
		#region Constructors

		/// <summary>
		/// Construct a ListBox instance from the given template.
		/// </summary>
		/// <param name="template"></param>
		public TreeView(TreeViewTemplate template)
			: base(template) {
			Items = template.Items;
			Title = template.Title ?? "";

			CurrentSelected = -1;
			OwnerDraw = template.OwnerDraw;

			if (this.Size.Width < 3 || this.Size.Height < 3) {
				template.HasFrameBorder = false;
			}

			HasFrame = template.HasFrameBorder;
			useSmallVersion = template.FrameTitle;
			HilightWhenMouseOver = template.HilightWhenMouseOver;
			CanHaveKeyboardFocus = template.CanHaveKeyboardFocus;

			LabelAlignment = template.LabelAlignment;
			TitleAlignment = template.TitleAlignment;
			CurrentSelected = template.InitialSelectedIndex;

			mouseOverIndex = -1;

			Queue<TreeNode> nodesToProcess = new Queue<TreeNode>();

			numOfItems = 0;

			foreach (var node in Items) {
				nodesToProcess.Enqueue(node);
			}

			while (nodesToProcess.Count > 0) {
				var treeNode = nodesToProcess.Dequeue();

				if (treeNode.HasChildren)
					foreach (var node in treeNode.Nodes) {
						nodesToProcess.Enqueue(node);
					}

				numOfItems++;
			}

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
			get { return GetNode(CurrentSelected).Label; }
		}

		#endregion
		#region Public Methods

		/// <summary>
		/// Returns the label of the item with the specified index.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public string GetItemLabel(int index) {
			if (index < 0 || index >= numOfItems)
				throw (new ArgumentOutOfRangeException("index"));

			return GetNode(index).Label;
		}

		#endregion
		#region Protected Methods

		/// <summary>
		/// Draws the title and title frame.
		/// </summary>
		protected void DrawTitle() {
			if (useSmallVersion && HasFrame) {

			} else {
				if (!string.IsNullOrEmpty(Title)) {
					Canvas.PrintStringAligned(titleRect, Title, TitleAlignment,
						VerticalAlignment.Center);
				}

				if (HasFrame &&
					this.Size.Width > 2 &&
					this.Size.Height > 2) {
					int fy = titleRect.Bottom + 1;

					Canvas.SetDefaultPigment(DetermineFramePigment());
					Canvas.DrawHLine(1, fy, Size.Width - 2);
					Canvas.PrintChar(0, fy, (int)TCODSpecialCharacter.TeeEast);
					Canvas.PrintChar(Size.Width - 1, fy, (int)TCODSpecialCharacter.TeeWest);
				}
			}           
		}

		private void NagivateNodes(TreeNode root, Action<TreeNode> action) {
			action(root);

			if (root.Expanded)
				foreach (var treeNode in root.Nodes) {
					NagivateNodes(treeNode, action);
				}
		}

		/// <summary>
		/// Draws each of the items in the list.
		/// </summary>
		protected void DrawItems() {
//            Stack<TreeNode> nodesToProcess = new Stack<TreeNode>();
//            Queue<TreeNode> nodesToDraw = new Queue<TreeNode>();
//
//            var items = new List<TreeNode>(Items);
//            items.Reverse();
//            foreach (var node in items) {
//                nodesToProcess.Push(node);
//            }
//
//            while (nodesToProcess.Count > 0) {
//                var treeNode = nodesToProcess.Pop();
//
//                
//                if (treeNode.Expanded)
//                    foreach (var node in treeNode.Nodes) {
//                        nodesToProcess.Push(node);                        
//                    }
//
//                nodesToDraw.Enqueue(treeNode);
//            }
//
			int index = 0;
//
//            foreach (var treeNode in nodesToDraw) {
//                DrawItem(index, treeNode);
//                index++;
//            }

			foreach (var treeNode in Items) {
				NagivateNodes(treeNode, node => DrawItem(index++, node));
			}
		}
		
		/// <summary>
		/// Draws a single item with the given index.
		/// </summary>
		protected void DrawItem(int index, TreeNode item) {
			
			//string label = item.HasChildren ? (item.Expanded ? "-" : "+").PadRight(item.Depth + 1) + item.Label;
			StringBuilder str = new StringBuilder();
			if (!item.HasChildren)
				str.Append(" ".PadRight(item.Depth + 1));
			else
				str.Append(item.Expanded ? "-".PadLeft(item.Depth + 1) : "+".PadLeft(item.Depth + 1));

			str.Append(item.Label);

			if (index == CurrentSelected) {
				Canvas.PrintStringAligned(itemsRect.TopLeft.X,
					itemsRect.TopLeft.Y + index,
					str.ToString(),
					LabelAlignment,
					itemsRect.Size.Width,
					Pigments[PigmentType.ViewSelected]);
			} else if (index == mouseOverIndex) {
				Canvas.PrintStringAligned(itemsRect.TopLeft.X,
					itemsRect.TopLeft.Y + index,
					str.ToString(),
					LabelAlignment,
					itemsRect.Size.Width,
					Pigments[PigmentType.ViewHilight]);
			} else {
				Canvas.PrintStringAligned(itemsRect.TopLeft.X,
					itemsRect.TopLeft.Y + index,
					str.ToString(),
					LabelAlignment,
					itemsRect.Size.Width,
					Pigments[PigmentType.ViewNormal]);
			}
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

			if (index < 0 || index >= numOfItems) {
				index = -1;
			}
			return index;
		}

		#endregion
		#region Message Handlers

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

			if (mouseOverIndex != -1) {
				var node = GetNode(mouseOverIndex);
				TooltipText = node != null ? node.TooltipText : null;
			} else {
				TooltipText = null;
			}
		}

		
		/// <summary>
		/// Detects which, if any, item has been selected by a left mouse button.  Override
		/// to add custom handling.
		/// </summary>
		/// <param name="mouseData"></param>
		protected internal override void OnMouseButtonUp(MouseData mouseData) {
			base.OnMouseButtonDown(mouseData);

			if (mouseOverIndex != -1) {
				var node = GetNode(mouseOverIndex);
				if (node.HasChildren)
					node.Expanded = !node.Expanded;

				if (CurrentSelected == mouseOverIndex) {                        
					OnItemSelected(node);
				}

				CurrentSelected = mouseOverIndex;
			}
		}

		/// <summary>
		/// Called when one of the items in the list has been selected with the left mouse
		/// button.  Base method triggers appropriate event.  Override to add custom handling.
		/// </summary>        
		protected virtual void OnItemSelected(TreeNode node) {
			if (ItemSelected != null) {
				ItemSelected(this, new EventArgs<TreeNode>(node));
			}
		}

		#endregion
		#region Private

		private List<TreeNode> Items;
		private int mouseOverIndex;
		private Rect titleRect;
		private Rect itemsRect;
		private int numberItemsDisplayed;
		private bool useSmallVersion;
		private int numOfItems;

		private void CalcMetrics(TreeViewTemplate template) {
			int nitms = numOfItems;
			int expandTitle = 0;

			int delta = Size.Height - nitms - 1;
			if (template.HasFrameBorder && !template.FrameTitle)
				delta -= 3;

			numberItemsDisplayed = numOfItems;
			if (delta < 0) {
				numberItemsDisplayed += delta;
			} else if (delta > 0) {
				expandTitle = delta;
			}

			int titleWidth = Size.Width;

			int titleHeight = 1 + expandTitle;

			if (Title != "") {
				if (template.HasFrameBorder) {
					titleRect = new Rect(Point.One,
											new Size(titleWidth - 2, titleHeight));

				} else {
					titleRect = new Rect(Point.Origin,
						new Size(titleWidth, titleHeight));
				}
			}

			int itemsWidth = Size.Width;
			int itemsHeight = numberItemsDisplayed;

			if (template.HasFrameBorder) {
				if (template.FrameTitle) {
					itemsRect = new Rect(Point.One, new Size(itemsWidth - 2, itemsHeight));
				} else
					itemsRect = new Rect(titleRect.BottomLeft.Shift(0, 2), new Size(itemsWidth - 2, itemsHeight));
			} else {
				itemsRect = new Rect(titleRect.BottomLeft.Shift(0, 1),
					new Size(itemsWidth, itemsHeight));
			}
		}    
	
		private TreeNode GetNode(int index) {
			if (index == 0)
				return Items[index];
			if (index > numOfItems)
				throw new ArgumentException("out of range", "index");

			TreeNode target = null;
			int[] i = {0};

			foreach (var treeNode in Items) {
				NagivateNodes(treeNode, node => { if (index == i[0]) target = node;
													i[0]++;
				});
			}

//            int i = 0;
//            Queue<TreeNode> nodes = new Queue<TreeNode>();
//            foreach (var node in Items) {
//                nodes.Enqueue(node);
//            }
//
//            while (nodes.Count > 0) {
//                var treeNode = nodes.Dequeue();
//
//                if (i == index)
//                    return treeNode;
//
//                if (treeNode.HasChildren && treeNode.Expanded)
//                    foreach (var node in treeNode.Nodes) {
//                        nodes.Enqueue(node);
//                    }
//
//                i++;
//            }

			return target;
		}

		#endregion
	}
	#endregion
}
