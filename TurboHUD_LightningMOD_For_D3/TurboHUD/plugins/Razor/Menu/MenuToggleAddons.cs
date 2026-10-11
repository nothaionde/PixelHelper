namespace Turbo.Plugins.Razor.Menu
{
	using SharpDX.DirectWrite;
	using System;
	using System.Drawing;
	using System.Linq;
	using System.Collections.Generic;

	using Turbo.Plugins.Default;
	using Turbo.Plugins.Razor.Log;
	using Turbo.Plugins.Razor.Label;

	public class MenuToggleAddons : BasePlugin, IMenuAddon//, IMenuSaveHandler /*, IAfterCollectHandler , IInGameTopPainter, ILeftClickHandler, IRightClickHandler*/
	{
		public string Namespace { get; set; } = "Turbo.Plugins.Default";
		public int MaxPluginsPerColumn { get; set; } = 40;
		
		public ILabelDecorator Label { get; set; }
		public ILabelDecorator LabelHint { get; set; }
		public float LabelSize { get; set; }
		public ILabelDecorator Panel { get; set; }
		//public MenuButtonDecorator Pin { get; set; } //optional, set only if the menu is pinnable, set to the pin decorator used in the menu

		public string Id { get; set; }
		public int Priority { get; set; } //the priority on the dock to show this addon (smaller to the left, higher to the right)
		public string DockId { get; set; }
		public string Config { get; set; }

		public IFont TextFont { get; set; }
		public IFont FadedFont { get; set; }
		public IFont EnabledFont { get; set; }
		public IFont DisabledFont { get; set; }
		public IFont HiddenFont { get; set; }
		
		public IBrush HoveredBrush { get; set; }
		public ILabelDecorator HintActivate { get; set; }
		public ILabelDecorator HintDeactivate { get; set; }
		
		public string TextEnabled { get; set; } = "✔️";
		public string TextDisabled { get; set; } = "❌";
		
		private IBrush DebugBrush1;
		private IBrush DebugBrush2;
		private IBrush DebugBrush3;
		private IFont DebugFont;
		
		private List<IMenuAddon> Addons;
		private LabelTableDecorator Table;
		private LabelStringDecorator SaveUI;
		private MenuPlugin Plugin;
		private int ColumnCount;
		private int PluginsPerColumn;
		
        public MenuToggleAddons()
        {
            Enabled = true;
			Priority = 10;
			DockId = "TopRight";
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
        }
		
		public void OnRegister(MenuPlugin plugin)
		{
			//LabelSize = Hud.Window.Size.Height * 0.05f;
			//Pin = plugin.CreatePin(); //enable pin handling
			//Pin.Alignment = HorizontalAlign.Right;
			
			TextFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 255, 255, 225, false, false, true);
			FadedFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 200, 200, 200, false, false, true);
			EnabledFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 0, 255, 0, false, false, true);
			DisabledFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 255, 0, 0, false, false, true); //170, 150, 120
			HiddenFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 0, 0, 0, 0, false, false, true); //170, 150, 120
			
			DebugBrush1 = Hud.Render.CreateBrush(255, 255, 0, 0, 1);
			DebugBrush2 = Hud.Render.CreateBrush(255, 0, 0, 255, 1);
			DebugBrush3 = Hud.Render.CreateBrush(255, 0, 255, 0, 1);
			DebugFont = Hud.Render.CreateFont("tahoma", 6f, 100, 255, 255, 255, false, false, 150, 0, 0, 0, true);
			
			HoveredBrush = plugin.HighlightBrush;
			
			Addons = plugin.Addons.Values.Where(a => a != this).OrderBy(a => a.Id).ToList(); //Hud.AllPlugins.Where(p => p is IPlugin && p.GetType().Namespace == Namespace).OrderBy(p => p.GetType().Name).ToList(); //plugin.Addons.Values.Where(a => a != this).OrderBy(a => a.Id).ToList();
			
			//Label
			Label = new LabelStringDecorator(Hud, "☰") {Font = TextFont}; //⚙ //new LabelTextureDecorator(Hud, Hud.Texture.GetTexture(363251, 0)) {TextureHeight = 38, ContentHeight = plugin.MenuHeight, BackgroundBrush = Hud.Render.CreateBrush(255, 0, 0, 255, 0)}; //2710494963 //new LabelStringDecorator(Hud, "⚙") {Font = TextFont};
			
			HintActivate = plugin.CreateHint("Нажмите, чтобы активировать");
			HintDeactivate = plugin.CreateHint("Нажмите, чтобы деактивировать");
			
			//figure out how many columns will be needed to fit all the plugins on the screen
			/*ColumnCount = (int)Math.Ceiling((float)Addons.Count / (float)MaxPluginsPerColumn);
			PluginsPerColumn = (int)Math.Floor((float)Addons.Count / (float)ColumnCount); //distribute them as evenly as possible
			LabelRowDecorator r = new LabelRowDecorator(Hud, 
				//new LabelStringDecorator(Hud, "2") {Font = TextFont, Hint = new LabelStringDecorator(Hud, "tooltip!") {Font = TextFont} },
				new LabelStringDecorator(Hud, TextEnabled) {Font = EnabledFont, OnClick = OnClick, SpacingTop = 3, SpacingBottom = 3}, 
				new LabelStringDecorator(Hud, "name") {Alignment = HorizontalAlign.Left, Font = TextFont, OnClick = OnClick, SpacingTop = 3, SpacingBottom = 3}
			);
			if (ColumnCount > 1)
			{
				for (int i = 1; i < ColumnCount; ++i)
				{
					r.Labels.Add(new LabelStringDecorator(Hud, TextEnabled) {Font = EnabledFont, OnClick = OnClick, SpacingLeft = 10, SpacingTop = 3, SpacingBottom = 3});
					r.Labels.Add(new LabelStringDecorator(Hud, "name") {Alignment = HorizontalAlign.Left, Font = TextFont, OnClick = OnClick, SpacingTop = 3, SpacingBottom = 3});
				}
			}
			
			int hoveredRow = -1;
			int hoveredCol = -1;*/
			var genericPin = plugin.CreatePin(this);
			genericPin.SpacingLeft = 0;
			genericPin.SpacingRight = 0;
			genericPin.SpacingTop = 0;
			genericPin.OnBeforeRender = null;
			genericPin.OnClick = (label) => {
				if (Table.HoveredRow > -1 && Table.HoveredRow < Addons.Count)
				{
					var addon = Addons[Table.HoveredRow];
					if (addon.Panel is object)
					{
						if (addon.Panel.LastX == 0 && addon.Panel.LastY == 0)
						{
							//need to force LastX and LastY to be set
							plugin.Docks[addon.DockId].DrawPanel(addon);
						}

						plugin.PinAddon(addon);
					}
				}
			};
			
			Table = new LabelTableDecorator(Hud, 
				new LabelRowDecorator(Hud, 
					//new LabelStringDecorator(Hud, "2") {Font = TextFont, Hint = new LabelStringDecorator(Hud, "tooltip!") {Font = TextFont} },
					genericPin,
					new LabelStringDecorator(Hud, TextEnabled) {Font = EnabledFont, SpacingTop = 3, SpacingBottom = 3, SpacingRight = 3},
					//new LabelStringDecorator(Hud, plugin.PinSymbol) {Font = plugin.PinFont, OnClick = OnClick, SpacingTop = 3, SpacingBottom = 3},
					//new LabelStringDecorator(Hud, "name") {Alignment = HorizontalAlign.Left, Font = TextFont, OnClick = OnClick, SpacingTop = 3, SpacingBottom = 3}
					new LabelAlignedDecorator(Hud,
						new LabelStringDecorator(Hud) {Font = EnabledFont, Alignment = HorizontalAlign.Left}, //plugin name
						new LabelStringDecorator(Hud) {Font = FadedFont, Alignment = HorizontalAlign.Left, SpacingLeft = 5} //namespace
					) {SpacingTop = 3, SpacingBottom = 3, SpacingLeft = 3, SpacingRight = 3} //Alignment = HorizontalAlign.Left, 
				) {OnClick = OnClick}
			) {
				BackgroundBrush = plugin.BgBrush,
				HoveredBrush = plugin.HighlightBrush,
				SpacingLeft = 10, 
				SpacingRight = 10,
				SpacingBottom = 5,
				//Hint = new LabelStringDecorator(Hud, "2tooltip!") {Font = TextFont},
				//OnClick = (lbl) => Hud.Sound.Speak("2"),
				FillWidth = false,
				/*OnBeforeRender = (label) => { //have to read these values in before they get reset before the OnFillRow loop
					hoveredRow = ((LabelTableDecorator)label).HoveredRow;
					hoveredCol = ((LabelTableDecorator)label).HoveredCol;
					return true;
				},*/
				Count = () => Addons.Count,
				OnFillRow = (row, index) => {
					if (index >= Addons.Count)
						return false;
							
					var addon = Addons[index];
					//LabelDecorator.DebugWrite((2*j).ToString() + " vs " + row.Labels.Count, Hud.Window.Size.Width*0.5f, Hud.Window.Size.Height*0.5f);
					LabelStringDecorator pinUI = (LabelStringDecorator)row.Labels[0];
					LabelStringDecorator statusUI = (LabelStringDecorator)row.Labels[1];
					//LabelStringDecorator nameUI = (LabelStringDecorator)row.Labels[2];
					LabelStringDecorator nameUI = (LabelStringDecorator)((ILabelDecoratorCollection)row.Labels[2]).Labels[0];
					LabelStringDecorator namespaceUI = (LabelStringDecorator)((ILabelDecoratorCollection)row.Labels[2]).Labels[1];
					
					string nameSpace = addon.GetType().Namespace;
					//int k = nameSpace.IndexOf("Turbo.Plugins.");
					//if (k > -1)
					nameSpace = nameSpace.Remove(0, 14); //removeString.Length
					namespaceUI.StaticText = "(" + nameSpace + ")";
					nameUI.StaticText = addon.Id; //+ " (" + nameSpace + ")";
					
					bool enabled = addon.Enabled;
					if (enabled)
					{
						statusUI.StaticText = TextEnabled;
						statusUI.Font = EnabledFont;
						statusUI.Hint = HintDeactivate;
						//nameUI.StaticText = addon.GetType().Name;
						nameUI.Font = EnabledFont;
						//nameUI.Hint = HintDeactivate;
						row.Hint = HintDeactivate;
					}
					else
					{
						statusUI.StaticText = TextDisabled;
						statusUI.Font = DisabledFont;
						statusUI.Hint = HintActivate;
						//nameUI.StaticText = addon.GetType().Name;
						nameUI.Font = DisabledFont;								
						//nameUI.Hint = HintActivate;
						row.Hint = HintActivate;
					}
					
					if (plugin.PinnedAddons.ContainsKey(addon.Id))
					{
						pinUI.Font = plugin.PinnedFont;
						pinUI.Hint = plugin.PinnedHint;
					}
					else if (addon.Panel is object)
					{
						pinUI.Font = plugin.DisabledFont;
						pinUI.Hint = plugin.PinHint;
					}
					else
					{
						pinUI.Font = HiddenFont;
						//pinUI.Font = plugin.DisabledFont;
						pinUI.Hint = null;
					}

					
					return true;
				}
			};
			
			//SaveUI = plugin.CreateSave();
			//SaveUI.Enabled = false;
			//SaveUI.OnClick = Save;
			
			//DeleteUI = plugin.CreateDelete(plugin.ConfigFilePath + plugin.ConfigFileName); //plugin.CreateSave();
			/*DeleteUI.StaticText = "ⓧ"; //x encircled
			DeleteUI.Font = Hud.Render.CreateFont("tahoma", 12, 255, 255, 0, 0, false, false, true);
			DeleteUI.Enabled = File.Exists(ConfigFile);
			DeleteUI.Hint = plugin.CreateHint("Delete Saved Config File");
			DeleteUI.OnClick = (label) => {
				File.Delete(ConfigFile);
				DeleteUI.Enabled = File.Exists(ConfigFile); //false;
			};*/
			
			Panel = new LabelColumnDecorator(Hud,
				new LabelDelayedDecorator(Hud,
					new LabelAlignedDecorator(Hud, 
						new LabelStringDecorator(Hud, "МЕНЮ ДОПОЛНЕНИЙ (" + Addons.Count + ")") {Font = plugin.TitleFont, SpacingLeft = 15, SpacingRight = 15},
						new LabelStringDecorator(Hud, "👁") {Hint = plugin.CreateHint("Toggle debug display"), Font = DisabledFont, Alignment = HorizontalAlign.Right, OnClick = (label) => {
							if (LabelDecorator.DebugBrush is object)
							{
								((LabelStringDecorator)label).Font = DisabledFont;
								LabelDecorator.DebugBrush = null;
								LabelDecorator.DebugBrush2 = null;
								LabelDecorator.DebugBrush3 = null;
								LabelDecorator.DebugFont = null;
							}
							else
							{
								((LabelStringDecorator)label).Font = EnabledFont;
								LabelDecorator.DebugBrush = DebugBrush1;
								LabelDecorator.DebugBrush2 = DebugBrush2;
								LabelDecorator.DebugBrush3 = DebugBrush3;
								LabelDecorator.DebugFont = DebugFont;
							}
						}},
						plugin.CreateDelete(plugin.ConfigFilePath + plugin.ConfigFileName + ".cs"),
						plugin.CreateSave(Save)//, //SaveUI,
						//plugin.CreatePin(this)
					)
				) {BackgroundBrush = plugin.BgBrush},
				Table
			);
			
			Plugin = plugin;
		}
		
		public void OnClick(ILabelDecorator label)
		{
			//SaveUI.Enabled = true;
			
			if (Table.HoveredRow > -1 && Table.HoveredRow < Addons.Count)
			{
				//Hud.Sound.Speak("Row: " + Table.HoveredRow + ", Column: " + col + ", Index: " + index); //(int)Math.Floor((float)col / (float)PluginsPerColumn));
				IPlugin addon = (IPlugin)Addons[Table.HoveredRow];
				addon.Enabled = !addon.Enabled;
				
				//write config file
				//Hud.TextLog.Queue(this); //calls Log() at the appropriate time
				Plugin.Save();
				
			}
		}
		
		public void Save(ILabelDecorator label)
		{
			//SaveUI.Enabled = false;
			
			//Hud.Sound.Speak("save");
			Plugin.Save();
		}
	}
}