namespace Turbo.Plugins.Razor.Menu
{
	using SharpDX.DirectWrite; //TextLayout
	using System.Drawing; //RectangleF
	using System.Collections.Generic; //List, Dictionary
	using System.Linq; //Where
	
	using Turbo.Plugins.Default;
	using Turbo.Plugins.Razor.Movable;
	using Turbo.Plugins.Razor.Label;

	public class VerticalMenuDock : IMenuDock
	{
		public string Id { get; set; }
		public bool Enabled { get; set; } = true;

		public List<IMenuAddon> Addons { get; set; } = new List<IMenuAddon>();
		public IMenuAddon HoveredAddon { get; private set; }
		
		public float AddonSpacingStart { get; set; } = 10f;
		public float AddonSpacingEnd { get; set; } = 10f;
		
		//x, y represent the topside anchor point
		public System.Func<RectangleF> Anchor { get; set; }
		public HorizontalAlign Alignment { get; set; } = HorizontalAlign.Center;
		public RectangleF DropBox { get; private set; }
		public MenuExpand Expand { get; set; } = MenuExpand.Left;
		
		//custom conditions at which to draw this dock, leave blank for the default behavior of drawing at clipState == ClipState.BeforeClip
		public System.Func<ClipState, bool> IsDrawn { get; set; } = (cs) => cs == ClipState.BeforeClip;

		//custom conditions at which to draw menu panels, leave blank for the default behavior
		public System.Func<bool> HoverCondition { get; set; }
		
		//background brushes
		public IBrush BackgroundBrush { get; set; }
		public IBrush LabelBrush { get; set; }
		public IBrush LabelHoveredBrush { get; set; }
		public IBrush LabelSelectedBrush { get; set; }
		public IBrush LabelPinnedBrush { get; set; }

		public bool Visible { get; set; } = false;
		public IController Hud { get; set; }		
		public IBrush DebugBrush { get; set; }
		
		private bool ForceCloseMenu = false;
		private bool Clicked = false;
		private bool sorted = false;
		private bool fixedWidth = true;
		private bool fixedHeight = true;
		

		public VerticalMenuDock(IController hud) 
		{
			Hud = hud;
			HoverCondition = () => (!Hud.Game.Me.InCombat && Hud.Game.Me.AnimationState == AcdAnimationState.Idle); //Hud.Game.IsInTown || !Hud.Game.Me.InCombat;
		}
		
		public void Paint(MenuPlugin plugin, ClipState clipState)
		{
			//if (plugin.Mover == null)
			//	return;
			//
			//if (!IsDrawn(clipState))
			//{
			//	if (clipState == ClipState.AfterClip)
			//	{
			//		if (DropBox is object && HoveredAddon is object && HoveredAddon.Panel is object && plugin.CursorAddon == null)
			//		{
			//			bool isPinned = plugin.PinnedAddons.ContainsKey(HoveredAddon.Id);
			//			if (!isPinned && LabelDecorator.IsVisible(HoveredAddon.Panel) && (HoveredAddon.Panel.Hovered || plugin.Mover.Overlay.GetUiObstructingCursor() == null) && (HoverCondition == null || HoverCondition() || Clicked)) //Hud.Game.IsInTown || !Hud.Game.Me.InCombat
			//			{
			//				float my = HoveredAddon.Label.LastY + HoveredAddon.Panel.Height > Hud.Window.Size.Height ? 
			//					Hud.Window.Size.Height - HoveredAddon.Panel.Height : 
			//					HoveredAddon.Label.LastY; //- AddonSpacingStart;
			//				
			//				HoveredAddon.Panel.Paint((Expand == MenuExpand.Left ? DropBox.X - HoveredAddon.Panel.Width : DropBox.X + DropBox.Width), my); //HoveredAddon.Label.LastX
			//			}
            //
			//			if (!HoveredAddon.Label.Hovered && !HoveredAddon.Panel.Hovered)
			//				HoveredAddon = null;
			//		}
			//		else if (Clicked) //mouse moved out of the panel and label area, reset the menu-show-override permission
			//		{
			//			//Hud.Sound.Speak("unclick");
			//			Clicked = false;
			//		}
			//	}
			//	
			//	return;
			//}
			//
			//var addons = Addons.Where(a => a.Enabled && LabelDecorator.IsVisible(a.Label));
			//if (!plugin.Mover.EditMode && !addons.Any())
			//{
			//	Visible = false;
			//	return;
			//}
            //
			////sort all the dock menus
			//if (!sorted)
			//{
			//	Addons.Sort((a, b) => a.Priority.CompareTo(b.Priority)); //Menus.OrderBy(m => m.Priority)
			//	sorted = true;
			//}
			//
			//DropBox = Anchor();
			//Visible = true;
			//
			////var addons = Addons.Where(a => LabelDecorator.IsVisible(a.Label));
			//float width = DropBox.Width; //addons.Max(a => (a.LabelSize > 0 ? a.LabelSize : a.Label.Width));
			//float height = addons.Sum(a => a.Label.Height + AddonSpacingStart + AddonSpacingEnd); //DropBox.Height; //plugin.MenuHeight;
			//if (DropBox.Height < height)
			//	DropBox = new RectangleF(DropBox.X, DropBox.Y, DropBox.Width, height);
			//if (plugin.Mover is object && plugin.Mover.EditMode)
			//	plugin.DropboxBrush.DrawRectangle(DropBox);
			//
			//float x = DropBox.X;
			//float y = DropBox.Y;
			//if (Alignment == HorizontalAlign.Center)
			//	y += DropBox.Height*0.5f - height*0.5f;
			//else if (Alignment == HorizontalAlign.Right) //bottom
			//	y += DropBox.Height - height;
			//
			//IMenuAddon hovered = null;
			//foreach (IMenuAddon addon in addons)
			//{
			//	float h = addon.Label.Height; //+ AddonSpacingStart + AddonSpacingEnd;
            //
			//	if (plugin.CursorAddon != addon)
			//	{
			//		//hook up click listener only if combat is blocking a panel from appearing
			//		if (!HoverCondition()) //Hud.Game.Me.InCombat)
			//		{
			//			if (addon.Label.OnClick != ForceShowPanel)
			//				addon.Label.OnClick = ForceShowPanel;
			//		}
			//		else if (addon.Label.OnClick is object)
			//			addon.Label.OnClick = null;
            //
			//		addon.Label.SpacingTop += AddonSpacingStart;
			//		addon.Label.SpacingBottom += AddonSpacingEnd;
			//		addon.Label.BackgroundBrush = HoveredAddon == addon ? (LabelHoveredBrush ?? LabelBrush) : LabelBrush; //LabelBrush;
			//		addon.Label.Width = DropBox.Width;
			//		
			//		//addon.Label.Paint(x, y);
			//		PaintLabel(addon.Label, x, y);
			//		
			//		bool isPinned = plugin.PinnedAddons.ContainsKey(addon.Id);
			//		
			//		//draw pin indicator
			//		if (isPinned && LabelPinnedBrush is object)
			//			LabelPinnedBrush.DrawRectangle((Expand == MenuExpand.Left ? x + DropBox.Width - 2 : x - 1), y, 3, h);
			//		
			//		if (plugin.CursorAddon == null && HoveredAddon == addon && !isPinned && addon.Panel is object && LabelDecorator.IsVisible(addon.Panel))
			//		{
			//			if (clipState == ClipState.AfterClip && (addon.Panel.Hovered || plugin.Mover.Overlay.GetUiObstructingCursor() == null) && (HoverCondition == null || HoverCondition() || Clicked)) //Hud.Game.IsInTown || !Hud.Game.Me.InCombat
			//			{
			//				float my = y + addon.Panel.Height > Hud.Window.Size.Height ? 
			//					Hud.Window.Size.Height - addon.Panel.Height : 
			//					y;
			//				
			//				addon.Panel.Paint((Expand == MenuExpand.Left ? x - addon.Panel.Width : x + DropBox.Width), my);
			//			}
			//			else if (!addon.Panel.Hovered && !addon.Label.Hovered)
			//				hovered = addon;
			//		}
            //
			//		if (addon.Label.Hovered || (addon.Panel is object && addon.Panel.Hovered))
			//			hovered = addon;
			//		
			//		addon.Label.SpacingTop -= AddonSpacingStart;
			//		addon.Label.SpacingBottom -= AddonSpacingEnd;
			//	}
            //
			//	y += h;
			//}
			//
			//if (hovered != HoveredAddon)
			//{
			//	//Hud.Sound.Speak("unclicked 2");
			//	Clicked = false;
			//}
			//
			//if (ForceCloseMenu)
			//{
			//	HoveredAddon = null;
			//	ForceCloseMenu = false;
			//}
			//else
			//	HoveredAddon = hovered;
		}
		
		public bool AddMenu(IMenuAddon addon)
		{
			if (!Addons.Contains(addon))
			{
				//if the first menu item doesn't have an action associated with it and its length is greater than 1, create pin functionality
				//if (addon.Menu is object && addon.Menu.MenuList.Count > 1 && !addon.Menu.MenuActions.ContainsKey(addon.Menu.MenuList[0]))
				//	addon.Menu.MenuActions.Add(addon.Menu.MenuList[0], this.PinAddon);
				//if (!addon.DockId.Equals(Id))
				addon.DockId = Id;
				Addons.Add(addon);
				sorted = false; //trigger a resorting before the next draw
				
				return true;
			}
			
			return false;
		}
		
		public void InsertMenu(IMenuAddon addon, int index) //, RectangleF rect)
		{
			//can drag around existing bar to change priority
			//if (Addons.Contains(addon))
			//	Addons.Remove(addon);
			//else
			addon.DockId = Id;
			
			//insert
			if (index >= Addons.Count)
				Addons.Add(addon);
			else
				Addons.Insert(index, addon);
				
			//redo the priorities
			var priority = 10;
			foreach (IMenuAddon a in Addons)
			{
				a.Priority = priority;
				priority += 10;
			}

			//sorted = false; //trigger a resorting before the next draw
			//HoveredAddon = null;
		}
		
		public void RemoveMenu(IMenuAddon addon)
		{
			//if (HoveredAddon == addon)
			//	HoveredAddon = null;
			Addons.Remove(addon);
			addon.DockId = null;
		}
		
		public void CloseMenu()
		{
			ForceCloseMenu = true;
		}
		
		//how this dock potentially selects a subset of the label for rendering to fit the vertical space
		//last label in a collection is the displayed element
		//can use Enabled flag to "hide" a custom vertical menu mode display and only show it when it is displayed by a vertical menu
		public void PaintLabel(ILabelDecorator label, float x, float y)
		{
			//save settings
			//var tmpSpacingTop = label.SpacingTop;
			//var tmpSpacingBottom = label.SpacingBottom;
			//var tmpSpacingLeft = label.SpacingLeft;
			//var tmpSpacingRight = label.SpacingRight;
			//var tmpBrush = label.BackgroundBrush;

			//temporary settings
			//label.SpacingLeft = 0;
			//label.SpacingRight = 0;
			//label.SpacingTop = AddonSpacingStart;
			//label.SpacingBottom = AddonSpacingEnd;
			//label.BackgroundBrush = LabelBrush; //null; //
					
			//draw label
			//label.Paint(x, y);
					
			//restore previous settings
			//label.SpacingTop = tmpSpacingTop;
			//label.SpacingBottom = tmpSpacingBottom;
			//label.SpacingLeft = tmpSpacingLeft;
			//label.SpacingRight = tmpSpacingRight;
			//label.BackgroundBrush = tmpBrush;
			
			//collapse the label
			ILabelDecoratorCollection ldc = null;
			bool wasEnabled = false;
			if (label is ILabelDecoratorCollection)
			{
				ldc = (ILabelDecoratorCollection)label;
				if (ldc.Labels.Count > 1)
				{
					wasEnabled = ldc.Labels[ldc.Labels.Count - 1].Enabled;
					
					for (int i = 0; i < ldc.Labels.Count; ++i) // - 1
						ldc.Labels[i].Enabled = false;
					
					ldc.Labels[ldc.Labels.Count - 1].Enabled = true; //enable last label if it was hidden before (can use Enabled flag to "hide" a vertical menu mode display and only show it when it is displayed by a vertical menu)
				}
				else
					ldc = null;
					
			}
			
			label.Resize();
			label.Width = DropBox.Width; //width;
			label.Paint(x + DropBox.Width*0.5f - label.Width*0.5f, y);
			
			if (ldc is object)
			{
				for (int i = 0; i < ldc.Labels.Count - 1; ++i)
					ldc.Labels[i].Enabled = true;
				ldc.Labels[ldc.Labels.Count - 1].Enabled = wasEnabled; //disable last label if it was disabled before vertical menu render (can use Enabled flag to "hide" a vertical menu mode display and only show it when it is displayed by a vertical menu)
			}
		}
		
		public void DrawPanel(IMenuAddon addon)
		{
			if (LabelDecorator.IsVisible(addon.Panel))
			{
				//float y = addon.Label.LastY;
				//y = addon.Label.LastY;
				//float mx = x + addon.Label.Width*0.5f - addon.Panel.Width*0.5f;
				float my = addon.Label.LastY + addon.Panel.Height > Hud.Window.Size.Height ? Hud.Window.Size.Height - addon.Panel.Height : addon.Label.LastY; //- AddonSpacingStart;
				addon.Panel.Paint((Expand == MenuExpand.Left ? DropBox.X - addon.Panel.Width : DropBox.X + DropBox.Width), my);
			}
		}
		
		public void ForceShowPanel(ILabelDecorator label)
		{
			Clicked = true;
			//Hud.Sound.Speak("Clicked");
		}
	}
}