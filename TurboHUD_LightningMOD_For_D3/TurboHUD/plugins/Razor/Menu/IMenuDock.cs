namespace Turbo.Plugins.Razor.Menu
{
	using Turbo.Plugins.Default;
	
	public interface IMenuDock
	{
		string Id { get; set; }
		bool Enabled { get; set; }
		bool Visible { get; set; }
		
		float AddonSpacingStart { get; set; }
		float AddonSpacingEnd { get; set; }

		System.Collections.Generic.List<IMenuAddon> Addons { get; set; }
		IMenuAddon HoveredAddon { get; }
		
		System.Func<System.Drawing.RectangleF> Anchor { get; set; }
		System.Func<ClipState, bool> IsDrawn { get; set; }
		System.Func<bool> HoverCondition { get; set; }
		System.Drawing.RectangleF DropBox { get; }
		
		HorizontalAlign Alignment { get; set; } //Alignment dictates which way the menus are laid out relevative to the anchor point
		MenuExpand Expand { get; set; } //direction that the menus open up
		
		//background brushes
		IBrush BackgroundBrush { get; set; }
		IBrush LabelBrush { get; set; }
		IBrush LabelHoveredBrush { get; set; }
		IBrush LabelSelectedBrush { get; set; }
		
		void Paint(MenuPlugin plugin, ClipState clipState);
		bool AddMenu(IMenuAddon addon);
		void InsertMenu(IMenuAddon addon, int index);
		void RemoveMenu(IMenuAddon addon);
		void CloseMenu();
		void DrawPanel(IMenuAddon addon);
	}
}