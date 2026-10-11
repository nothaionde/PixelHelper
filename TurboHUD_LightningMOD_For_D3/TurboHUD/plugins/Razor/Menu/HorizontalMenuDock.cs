namespace Turbo.Plugins.Razor.Menu
{
	using SharpDX.DirectWrite; //TextLayout
	using System.Drawing; //RectangleF
	using System.Collections.Generic; //List, Dictionary
	using System.Linq; //Where
	
	using Turbo.Plugins.Default;
	using Turbo.Plugins.Razor.Movable;
	using Turbo.Plugins.Razor.Label;

	public class HorizontalMenuDock : IMenuDock
	//public class MenuDecorator : TopLabelDecorator
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
		public MenuExpand Expand { get; set; } = MenuExpand.Up;
		
		//custom conditions at which to draw this dock, leave blank for the default behavior of drawing at clipState == ClipState.BeforeClip
		public System.Func<ClipState, bool> IsDrawn { get; set; } = (cs) => cs == ClipState.BeforeClip;
		
		//custom conditions at which to draw menu panels, leave blank for the default behavior
		public System.Func<bool> HoverCondition { get; set; }
		
		//background brushes
		public IBrush BackgroundBrush { get; set; }
		public IBrush LabelBrush { get; set; }
		public IBrush LabelHoveredBrush { get; set; } //not implemented yet
		public IBrush LabelSelectedBrush { get; set; } //not implemented yet
		public IBrush LabelPinnedBrush { get; set; }

		public bool Visible { get; set; } = false;
		public IController Hud { get; set; }		
		public IBrush DebugBrush { get; set; }
		
		private bool ForceCloseMenu = false;
		private bool Clicked = false;
		private bool sorted = false;
		private bool fixedWidth = true;
		private bool fixedHeight = true;
		
		public HorizontalMenuDock(IController hud) 
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
			//				float mx = HoveredAddon.Label.LastX + HoveredAddon.Label.Width*0.5f - HoveredAddon.Panel.Width*0.5f;
			//				if (mx < 0)
			//					mx = 0;
			//				else if (mx + HoveredAddon.Panel.Width > Hud.Window.Size.Width)
			//					mx = Hud.Window.Size.Width - HoveredAddon.Panel.Width;
			//				
			//				HoveredAddon.Panel.Paint(mx, (Expand == MenuExpand.Up ? HoveredAddon.Label.LastY - HoveredAddon.Panel.Height : HoveredAddon.Label.LastY + DropBox.Height));
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
			//		
			//		if (ForceCloseMenu)
			//		{
			//			HoveredAddon = null;
			//			ForceCloseMenu = false;
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
			//float width = addons.Sum(a => (a.LabelSize > 0 ? a.LabelSize : a.Label.Width) + AddonSpacingStart + AddonSpacingEnd);
			//float height = DropBox.Height; //plugin.MenuHeight;
			//if (DropBox.Width < width)
			//	DropBox = new RectangleF(DropBox.X, DropBox.Y, width, DropBox.Height);
			//if (plugin.Mover is object && plugin.Mover.EditMode)
			//	plugin.DropboxBrush.DrawRectangle(DropBox);
			//
			//
			//
			//float x = DropBox.X;
			//float y = DropBox.Y;
			//if (Alignment == HorizontalAlign.Center)
			//	x += DropBox.Width*0.5f - width*0.5f;
			//else if (Alignment == HorizontalAlign.Right)
			//	x += DropBox.Width - width;
			//
			////LabelDecorator.DebugWrite(x.ToString("F0") + "," + y.ToString("F0"), x, y - 28);
			//
			//IMenuAddon hovered = null;
			//foreach (IMenuAddon addon in addons)
			//{
			//	float w = addon.Label.Width;
			//	if (addon.LabelSize > 0)
			//		w = addon.LabelSize;
			//	
			//	w += AddonSpacingStart + AddonSpacingEnd;
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
			//		//draw label
			//		//addon.Label.SpacingLeft += AddonSpacingStart;
			//		//addon.Label.SpacingRight += AddonSpacingEnd;
			//		//addon.Label.SpacingTop = 0;
			//		//addon.Label.SpacingBottom = 0;
			//		addon.Label.BackgroundBrush = HoveredAddon == addon ? (LabelHoveredBrush ?? LabelBrush) : LabelBrush;
			//		addon.Label.Height = height;
			//		addon.Label.Width = w;
			//		
			//		addon.Label.Paint(x, y);
			//		
			//		bool isPinned = plugin.PinnedAddons.ContainsKey(addon.Id);
			//		
			//		//draw pin indicator
			//		if (isPinned && LabelPinnedBrush is object)
			//			LabelPinnedBrush.DrawRectangle(x, (Expand == MenuExpand.Up ? y + height - 2 : y - 1), w, 3);
            //
			//		if (plugin.CursorAddon == null && HoveredAddon == addon && !isPinned && addon.Panel is object && LabelDecorator.IsVisible(addon.Panel))
			//		{
			//			if (clipState == ClipState.AfterClip && (addon.Panel.Hovered || plugin.Mover.Overlay.GetUiObstructingCursor() == null) && (HoverCondition == null || HoverCondition() || Clicked)) //Hud.Game.IsInTown || !Hud.Game.Me.InCombat
			//			{
			//				float mx = x + w*0.5f - addon.Panel.Width*0.5f;
			//				if (mx < 0)
			//					mx = 0;
			//				else if (mx + addon.Panel.Width > Hud.Window.Size.Width)
			//					mx = Hud.Window.Size.Width - addon.Panel.Width;
			//				
			//				addon.Panel.Paint(mx, (Expand == MenuExpand.Up ? y - addon.Panel.Height : y + height));
			//			}
			//			else if (!addon.Panel.Hovered && !addon.Label.Hovered)
			//				hovered = addon;
			//		}
            //
			//		if (addon.Label.Hovered || (addon.Panel is object && addon.Panel.Hovered))
			//			hovered = addon;
			//		
			//		//addon.Label.SpacingLeft -= AddonSpacingStart;
			//		//addon.Label.SpacingRight -= AddonSpacingEnd;
			//	}
			//	
			//	x += w;
			//}
			//
			//if (hovered != HoveredAddon)
			//	Clicked = false;
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
				//addon.Label.OnClick = ForceShowPanel;
			
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
			//addon.Label.OnClick = ForceShowPanel;
			
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
		
		public void DrawPanel(IMenuAddon addon)
		{
			if (LabelDecorator.IsVisible(addon.Panel))
			{
				//float x = addon.Label.LastX;
				//y = addon.Label.LastY;
				float mx = addon.Label.LastX + addon.Label.Width*0.5f - addon.Panel.Width*0.5f;
				if (mx < 0)
					mx = 0;
				else if (mx + addon.Panel.Width > Hud.Window.Size.Width)
					mx = Hud.Window.Size.Width - addon.Panel.Width;
				
				addon.Panel.Paint(mx, (Expand == MenuExpand.Up ? addon.Label.LastY - addon.Panel.Height : addon.Label.LastY + addon.Label.Height));
			}
		}
		
		public void ForceShowPanel(ILabelDecorator label)
		{
			Clicked = true;
			//Hud.Sound.Speak("Clicked");
		}
	}
}