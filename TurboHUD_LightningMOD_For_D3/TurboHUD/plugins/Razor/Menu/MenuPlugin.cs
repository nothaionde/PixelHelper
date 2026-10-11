namespace Turbo.Plugins.Razor.Menu
{
	using System;
	using System.Drawing;
	using System.Linq;
	using System.Collections.Generic;
	using System.Text; //StringBuilder
	using System.Windows.Forms; //MouseButtons
	
	using Turbo.Plugins.Default;
	//using Turbo.Plugins.Razor.Click;
	using Turbo.Plugins.Razor.Hotkey;
	using Turbo.Plugins.Razor.Movable;
	using Turbo.Plugins.Razor.Log;
	using Turbo.Plugins.Razor.Label;
	using Turbo.Plugins.Razor.Plugin;

	public class MenuPlugin : BasePlugin, IBeforeRenderHandler, IInGameTopPainter, ICustomizer, IMovable, ITextLogger, IPluginManifest, IHotkeyEventHandler, IMouseClickHandler, IMouseClickBlocker //, ILeftClickHandler, ILeftBlockHandler //, IBeforeRenderHandler
	{
		//IPluginManifest
		public string Name { get; set; } = "Menu Plugin System";
        public string Description { get; set; } = "Menu Plugin System is a framework for showing, hiding and docking Labels as interactive menu panels.";
		public string Version { get; set; }
		public List<string> Dependencies { get; set; } = new List<string>() {
			"Turbo.Plugins.Razor.Movable.MovableController",
			"Turbo.Plugins.Razor.Label.LabelController",
		};
		
		//MenuPlugin properties
		public string ConfigFileName { get; set; } = "MenuPluginConfig";
		public string ConfigFilePath { get; set; } = "User"; //@"plugins\User\";
		//public Func<bool> LeftClickBlockCondition { get; private set; }
		
		public IKeyEvent CloseHotkey { get; set; }
		
		//some styling shared by menu addons
		public float FontSize { get; set; } = 7f; //8f;
		public float TooltipFontSize { get; set; } = 6.5f;
		public IBrush BgBrush { get; set; }
		public IBrush BgBrushAlt { get; set; }
		public IBrush HighlightBrush { get; set; }
		public float MenuHeight { get; private set; }
		
		public Dictionary<string, IMenuDock> Docks { get; set; }
		public Dictionary<string, IMenuAddon> Addons { get; set; } = new Dictionary<string, IMenuAddon>();
		public Dictionary<string, MovableArea> PinnedAddons { get; set; } = new Dictionary<string, MovableArea>();
		public MovableController Mover { get; set; }
		public IMenuAddon CursorAddon { get; set; }
		public MovableArea PickedUpArea { get; private set; }
		
		public IBrush DropboxBrush { get; set; }
		public IBrush PinnedBrush { get; set; } 
		
		//public MenuButtonDecorator Pin { get; set; }
		public IFont PinFont { get; set; }
		public IFont PinnedFont { get; set; }
		public IFont PauseFont { get; set; }
		public IFont ResumeFont { get; set; }
		public IFont DisabledFont { get; set; }
		public string PinSymbol { get; set; } = "🖈"; //◢▟⇲📌
		public string ResetSymbol { get; set; } = "↺";
		public string PauseSymbol { get; set; } = "⏸";
		public string ResumeSymbol { get; set; } = "⏯️"; //"▶";
		public string TextPin { get; set; } = "Нажмите, чтобы закрепить это меню открытым";
		public string TextPinned { get; set; } = "Нажмите, чтобы открепить это меню";
		public string TextReset { get; set; } = "Нажмите, чтобы сбросить";
		public string TextPause { get; set; } = "Нажмите, чтобы приостановить";
		public string TextResume { get; set; } = "Нажмите, чтобы возобновить";
		public string TextPageNewer { get; set; } = "Нажмите, чтобы показать новее";
		public string TextPageOlder { get; set; } = "Нажмите, чтобы показать старые";
		public string TextSave { get; set; } = "Нажмите, чтобы сохранить в файл";
		public string TextSaveOn { get; set; } = "Нажмите, чтобы остановить сохранение в файл";
		public string TextSaveOff { get; set; } = "Нажмите, чтобы начать сохранение в файл";
		public string TextDeleteConfig { get; set; } = "Нажмите, чтобы удалить сохраненную конфигурацию";
		public ILabelDecorator PinHint { get; set; }
		public ILabelDecorator PinnedHint { get; set; }
		public ILabelDecorator PauseHint { get; set; }
		public ILabelDecorator ResumeHint { get; set; }
		public ILabelDecorator PageNewerHint { get; set; }
		public ILabelDecorator PageOlderHint { get; set; }
		public ILabelDecorator SaveHint { get; set; }
		public ILabelDecorator SaveOnHint { get; set; }
		public ILabelDecorator SaveOffHint { get; set; }
		public ILabelDecorator DeleteConfigHint { get; set; }
		public IFont TitleFont { get; set; }
		public IFont TooltipFont { get; set; }
		
		public float StandardGraphWidth { get; set; }
		public float StandardGraphHeight { get; set; }
		
		private StringBuilder TextBuilder;
		//private System.Func<float> CalculateMenuHeight;
		private IBrush DebugBrush;
		
		public int HoverDelay { get; set; } = 1250; //milliseconds //set to 0 if you don't want any delays on showing labeldelayeddecorator on hover
		private List<LabelDelayedDecorator> OverridenDelays = new List<LabelDelayedDecorator>();
		public System.Func<ILabelDecorator, bool> RenderYes { get; private set; } = (label) => true;
		public System.Func<ILabelDecorator, bool> RenderNo { get; private set; } = (label) => false;
		//public IFont HoverFont { get; set; }
		//private IWatch Hover;
		//private IMenuDecorator HoveredDecorator;
		//public MenuStringDecorator HoverTooltip { get; set; }
		//public IBrush SkillBorderLight { get; set; }
		//public IBrush SkillBorderDark { get; set; }
		
		private Dictionary<string, string[]> ConfigDock = new Dictionary<string, string[]>();
		//private Dictionary<string, string> ConfigAddon = new Dictionary<string, string>();
		private Dictionary<string, Tuple<bool, string>> CustomizeAddon = new Dictionary<string, Tuple<bool, string>>();
		
		private IMenuDock InsertionDock = null;
		private float InsertionX = 0;
		private int InsertionIndex = 0;
		
        public MenuPlugin()
        {
            Enabled = true;
            Order = -10000;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
			
			CloseHotkey = Hud.Input.CreateKeyEvent(true, SharpDX.DirectInput.Key.Escape, false, false, false);
			
			TextBuilder = new StringBuilder();
			
			DebugBrush = Hud.Render.CreateBrush(255, 190, 93, 239, 1);
			DropboxBrush = Hud.Render.CreateBrush(75, 69, 41, 255, 0);
			
			BgBrush = Hud.Render.CreateBrush(150, 0, 0, 0, 0);
			BgBrushAlt = Hud.Render.CreateBrush(150, 45, 45, 45, 0);
			HighlightBrush = Hud.Render.CreateBrush(150, 40, 71, 162, 0);
			PinnedBrush = Hud.Render.CreateBrush(255, 255, 0, 0, 0);
			
			PinFont = Hud.Render.CreateFont("tahoma", 10f, 255, 255, 255, 255, false, false, 135, 0, 0, 0, true);
			PinnedFont = Hud.Render.CreateFont("tahoma", 10f, 255, 255, 55, 55, false, false, 135, 0, 0, 0, true);
			PauseFont = Hud.Render.CreateFont("tahoma", 9f, 255, 55, 255, 55, false, false, 100, 0, 0, 0, true);
			ResumeFont = Hud.Render.CreateFont("tahoma", 9f, 255, 255, 55, 55, false, false, 100, 0, 0, 0, true);
			DisabledFont = Hud.Render.CreateFont("tahoma", 9f, 255, 155, 155, 155, false, false, 100, 0, 0, 0, true);
			TitleFont = Hud.Render.CreateFont("arial", 6.5f, 255, 255, 255, 255, false, false, 180, 0, 0, 0, true);
			TooltipFont = Hud.Render.CreateFont("tahoma", TooltipFontSize, 255, 255, 255, 255, false, false, 180, 0, 0, 0, true); //7f
			PinHint = CreateHint(TextPin); //new LabelStringDecorator(Hud, TextPin) { Font = TooltipFont };
			PinnedHint = CreateHint(TextPinned); //new LabelStringDecorator(Hud, TextPinned) { Font = TooltipFont };
			PauseHint = CreateHint(TextPause);
			ResumeHint = CreateHint(TextResume);
			PageNewerHint = CreateHint(TextPageNewer);
			PageOlderHint = CreateHint(TextPageOlder);
			SaveHint = CreateHint(TextSave);
			SaveOnHint = CreateHint(TextSaveOn);
			SaveOffHint = CreateHint(TextSaveOff);
			DeleteConfigHint = CreateHint(TextDeleteConfig);
			
			//prevent left clicks when trying to drag addon labels to different docks
			//LeftClickBlockCondition = () => Mover is object && Mover.EditMode && Docks.Values.Any(d => d.Visible && d.HoveredAddon is object && d.HoveredAddon.Label.Hovered); //Docks.Values.Any(d => d.Visible && d.HoveredAddon is object && d.HoveredAddon.Label.Hovered); //
			// && (!d.HoverCondition() || (Mover is object && Mover.EditMode))
			/*Pin = new MenuButtonDecorator(Hud)
			{
				Alignment = HorizontalAlign.Right,
				PinFont = Hud.Render.CreateFont("tahoma", 10f, 255, 185, 185, 185, false, false, false),
				PinnedFont = Hud.Render.CreateFont("tahoma", 10f, 255, 255, 55, 55, false, false, false),
				PinHint = "Click to (un)pin this menu open",
			};
			
			//Hover = Hud.Time.CreateWatch();
			HoverFont = Hud.Render.CreateFont("tahoma", 7f, 255, 255, 255, 255, false, false, false);
			HoverTooltip = new MenuStringDecorator() {
				TextFont = HoverFont,
				BackgroundBrush = BgBrush,
				SpacingLeft = 5,
				SpacingTop = 5,
				SpacingRight = 5,
				SpacingBottom = 5,
			};
			
			SkillBorderLight = Hud.Render.CreateBrush(200, 95, 95, 95, 1); //235, 227, 164 //138, 135, 109
			SkillBorderDark = Hud.Render.CreateBrush(150, 0, 0, 0, 1);*/
			
			//Root.TopLayer.BattleNetModalNotifications_main.ModalNotification.Content.List.Title //error popup ui path
			
			/*LabelDecorator.DebugBrush = Hud.Render.CreateBrush(255, 255, 0, 0, 1);
			//LabelDecorator.DebugBrush2 = Hud.Render.CreateBrush(255, 93, 0, 255, 1);
			LabelDecorator.DebugBrush2 = Hud.Render.CreateBrush(255, 0, 0, 255, 1);
			LabelDecorator.DebugBrush3 = Hud.Render.CreateBrush(255, 0, 255, 0, 1);
			LabelDecorator.DebugFont = Hud.Render.CreateFont("tahoma", 6f, 100, 255, 255, 255, false, false, 150, 0, 0, 0, true);*/

        }
		
		public void Customize()
		{
			Hud.TogglePlugin<BloodShardPlugin>(false);
			Hud.TogglePlugin<InventoryFreeSpacePlugin>(false);
			Hud.TogglePlugin<AttributeLabelListPlugin>(false);
		}
		
		public void BeforeRender()
		{
			if (Mover == null)
				return;
			
			foreach (IMenuAddon addon in Addons.Values)
			{
				if (!addon.Enabled)
					continue;
				
				if (addon is IMenuSaveHandler)
				{
					IMenuSaveHandler handler = (IMenuSaveHandler)addon;
					if (handler.SaveMode && handler.SaveCondition())
						handler.SaveToFile();
				}
			}
		}
		
		public void OnHotkeyEvent(IKeyEvent keyEvent)
		{
			if (keyEvent.Matches(CloseHotkey))
			{
				//Hud.Sound.Speak("test");
				foreach (IMenuDock dock in Docks.Values)
				{
					if (dock.Enabled && dock.HoveredAddon is object)
						dock.CloseMenu();
						//dock.HoveredAddon = null;
				}
			}
		}
		
        public void PaintTopInGame(ClipState clipState)
        {
			if (Docks is object && Docks.Any())
			{
				foreach (IMenuDock dock in Docks.Values)
				{
					if (dock.Enabled)
						dock.Paint(this, clipState);
				}
					
				//LabelDecorator.DebugWrite(string.Join(", ", PinnedAddons.Keys), Hud.Window.Size.Width*0.5f, Hud.Window.Size.Height*0.35f);
			}
			
			/*if (Mover is object)
				LabelDecorator.DebugWrite((Mover.Overlay.GetUiObstructingCursor() == null ? "clear" : "obstruction") + System.Environment.NewLine
			+ (Mover.Overlay._findUiObstructingCursor() == null ? "clear" : "obstruction")
			+ System.Environment.NewLine 
			+ "Stash: " + Hud.Inventory.StashMainUiElement.Rectangle + System.Environment.NewLine + "Inv: " + Hud.Inventory.InventoryMainUiElement.Rectangle + System.Environment.NewLine + "Inv - " + Hud.Window.CursorInsideRect(Hud.Inventory.InventoryMainUiElement.Rectangle.X, Hud.Inventory.InventoryMainUiElement.Rectangle.Y, Hud.Inventory.InventoryMainUiElement.Rectangle.Width, Hud.Inventory.InventoryMainUiElement.Rectangle.Height * 0.825f), Hud.Window.CursorX + 30, Hud.Window.CursorY + 30);*/
		}
		
		private void LoadConfig()
		{
			if (ConfigDock.Any())
			{
				foreach (IMenuDock dock in Docks.Values)
					dock.Addons.Clear();
				
				List<IMenuAddon> claimed = new List<IMenuAddon>(); //use to find addons that aren't claimed by any dock specifically, then apply their original config...
				foreach (KeyValuePair<string, string[]> pair in ConfigDock)
				{
					if (Docks.ContainsKey(pair.Key))
					{
						IMenuDock dock = Docks[pair.Key];
						List<IMenuAddon> addons = dock.Addons;
						int priority = 10;
						foreach (string addonId in pair.Value)
						{
							if (Addons.ContainsKey(addonId))
							{
								var addon = Addons[addonId];
								if (!claimed.Contains(addon) && !addons.Contains(addon))
								{
									addon.DockId = dock.Id;
									addon.Priority = priority;
									priority += 10;
									
									addons.Add(addon);
									claimed.Add(addon);
								}
							}
						}
					}
				}
				
				//repopulate with unclaimed addons
				foreach (IMenuAddon addon in Addons.Values.OrderBy(a => a.Priority))
				{
					if (!claimed.Contains(addon))
					{
						if (Docks.ContainsKey(addon.DockId))
						{
							IMenuDock dock = Docks[addon.DockId];
							addon.Priority = (dock.Addons.Count == 0 ? 10 : dock.Addons[dock.Addons.Count - 1].Priority + 10);
							dock.Addons.Add(addon);
						}
					}
				}
				
				ConfigDock.Clear();
			}
			
			if (CustomizeAddon.Any())
			{
				foreach (KeyValuePair<string, Tuple<bool, string>> pair in CustomizeAddon)
				{
					if (Addons.ContainsKey(pair.Key))
					{
						Addons[pair.Key].Enabled = pair.Value.Item1;
						Addons[pair.Key].Config = pair.Value.Item2;
						
						//debug
						//if (!string.IsNullOrEmpty(pair.Value.Item2))
						//	Hud.Sound.Speak(pair.Value.Item2);
						//if (ConfigAddon.ContainsKey(pair.Key))
						//	Addons[pair.Key].Config = ConfigAddon[pair.Key];
					}
				}
				
				CustomizeAddon.Clear();
				//ConfigAddon.Clear();
			}

			/*if (ConfigAddon.Any())
			{
				foreach (KeyValuePair<string, bool> pair in ConfigAddon)
				{
					if (Addons.ContainsKey(pair.Key))
						((IPlugin)Addons[pair.Key]).Enabled = pair.Value;
				}
				
				ConfigAddon.Clear();
			}*/
		}
		
		public bool MouseDown(MouseButtons button)
		{
			if (button == MouseButtons.Left && Mover is object && Mover.EditMode)
			{
				if (CursorAddon == null && Docks is object && Mover.CursorPluginArea == null) // && HoveredAddon is object
				{
					IMenuDock dock = Docks.Values.FirstOrDefault(d => d.Enabled && d.Visible && d.HoveredAddon is object && d.DropBox is object && Hud.Window.CursorInsideRect(d.DropBox.X, d.DropBox.Y, d.DropBox.Width, d.DropBox.Height));
					if (dock is object)
					{
						IMenuAddon addon = dock.HoveredAddon; //Docks.Values.FirstOrDefault(d => d.HoveredAddon is object)?.HoveredAddon;
						if (LabelDecorator.IsVisible(addon.Label))
						{
							CursorAddon = addon;
							PickedUpArea = Mover.CreateArea(
								this,
								"OnCursor", //addon.Id, //"Drag", //area name
								new System.Drawing.RectangleF(addon.Label.LastX, addon.Label.LastY, addon.Label.Width, MenuHeight), //Hud.Window.CursorX - hovered.Label.Width*0.5f, Hud.Window.CursorY - menuItem.Height*0.5f, menuItem.Width, menuItem.Height), //position + dimensions
								true, //enabled at start?
								false, //save to config file?
								ResizeMode.Off //resizable?				
							);
							PickedUpArea.DeleteOnDisable = true;
							
							Mover.PickUp(PickedUpArea);
						}
					}
				}
			}
			
			return true;
		}
        public bool MouseUp(MouseButtons button)
		{
			if (button == MouseButtons.Left)
			{
				if (InsertionDock is object && InsertionIndex > -1)
				{
					if (Docks.ContainsKey(CursorAddon.DockId))
						Docks[CursorAddon.DockId].RemoveMenu(CursorAddon);
					InsertionDock.InsertMenu(CursorAddon, InsertionIndex);
					
					Hud.TextLog.Queue(this, 3);
					
					InsertionDock = null;
				}
				
				if (CursorAddon is object)
				{
					CursorAddon = null;
					PickedUpArea.Enabled = false;
					PickedUpArea = null;
				}
			}

			return true;
		}

		public bool MouseClickShouldBeBlocked(MouseButtons button, int x, int y)
		{
			return button == MouseButtons.Left && Mover is object && Mover.EditMode && Docks.Values.Any(d => d.Visible && d.HoveredAddon is object && d.HoveredAddon.Label.Hovered);
		}
		
		public void OnRegister(MovableController mover)
		{
			Mover = mover;

			//var potionUI = Hud.Render.GetPlayerSkillUiElement(ActionKey.Heal);
			MenuHeight = (float)System.Math.Ceiling(Hud.Window.Size.Height - (Hud.Render.GetPlayerSkillUiElement(ActionKey.Heal).Rectangle.Bottom + 1)) + 1f;
			
			//init
			if (StandardGraphWidth == 0)
				StandardGraphWidth = Hud.Window.Size.Width - Hud.Render.MinimapUiElement.Rectangle.X - 5;
			if (StandardGraphHeight == 0)
				StandardGraphHeight = (Hud.Window.Size.Width - Hud.Render.MinimapUiElement.Rectangle.X - 5) * 0.7f;	
				
			foreach (string dockId in Docks.Keys)
				Docks[dockId].Id = dockId;
			
			foreach (IPlugin plugin in Hud.AllPlugins.Where(p => p is IMenuAddon))
			{
				//Hud.TextLog.Log("debug_IMenuAddon", "detected addon..." + plugin.GetType().Name, true, true);
				
				IMenuAddon addon = (IMenuAddon)plugin;
				string id = addon.GetType().Name;
				if (!Addons.ContainsKey(id) && Docks.ContainsKey(addon.DockId))
				{
					addon.Id = id;
					Addons.Add(id, addon);
					Docks[addon.DockId].AddMenu(addon);
					
					//Hud.TextLog.Log("debug_IMenuAddon", "registered " + plugin.GetType().Name, true, true);
				}
			}

			//load config
			LoadConfig();
			
			foreach (IMenuAddon addon in Addons.Values)
				addon.OnRegister(this);
			
			//Hud.TextLog.Queue(this);
		}

		public void PaintArea(MovableController mover, MovableArea area, float deltaX = 0, float deltaY = 0)
        {
			//Hud.Sound.Speak(area.Name);
				
			//if (area == PickedUpArea) //.Name == "Drag")
			//{
			//	float x = area.Rectangle.X + deltaX;
			//	float y = area.Rectangle.Y + deltaY;
			//	
			//	//calculate potential insertion point
			//	RectangleF rect = new RectangleF(x, y, area.Rectangle.Width, area.Rectangle.Height);
			//	InsertionDock = null;
			//	//InsertionX = 0;
			//	//InsertionIndex = -1;
			//	foreach (IMenuDock dock in Docks.Values.Where(d => d.Enabled))
			//	{
			//		if (rect.IntersectsWith(new RectangleF(dock.DropBox.X - MenuHeight, dock.DropBox.Y - MenuHeight, dock.DropBox.Width + MenuHeight*2, dock.DropBox.Height + MenuHeight*2)))
			//		//if (dock.DropBox.IntersectsWith(rect))
			//		{
			//			InsertionIndex = 0;
            //
			//			bool isHorizontal = dock is HorizontalMenuDock;
			//			if (dock.Addons.Count == 0)
			//			{
			//				InsertionDock = dock;
            //
			//				if (isHorizontal)
			//				{
			//					switch (dock.Alignment)
			//					{
			//						case HorizontalAlign.Left:
			//							InsertionX = dock.DropBox.X;
			//							break;
			//						case HorizontalAlign.Center:
			//							InsertionX = dock.DropBox.X + dock.DropBox.Width*0.5f;
			//							break;
			//						case HorizontalAlign.Right:
			//							InsertionX = dock.DropBox.Right;
			//							break;
			//					}
			//				}
			//				else //VerticalMenuDock
			//				{
			//					switch (dock.Alignment)
			//					{
			//						case HorizontalAlign.Left:
			//							InsertionX = dock.DropBox.Y;
			//							break;
			//						case HorizontalAlign.Center:
			//							InsertionX = dock.DropBox.Y + dock.DropBox.Height*0.5f;
			//							break;
			//						case HorizontalAlign.Right:
			//							InsertionX = dock.DropBox.Bottom;
			//							break;
			//					}
			//				}
			//			}
			//			else if (isHorizontal)
			//			{
			//				//InsertionX = area.Rectangle.X;
			//				InsertionX = -1; //dock.Addons[0].LastX;
			//				int oldIndex = -1;
			//				for (int i = 0; i < dock.Addons.Count; ++i) //IMenuAddon addon in dock.Addons)
			//				{
			//					var addon = dock.Addons[i];
			//					if (addon == CursorAddon)
			//					{
			//						oldIndex = i;
			//						//if (InsertionX == -1)
			//						//	InsertionX = area.Rectangle.X;
			//						//else
			//						//{
			//						/*	var oldDistance = System.Math.Abs(x - InsertionX);
			//							var newDistance = System.Math.Abs(x - area.Rectangle.X);
			//							if (oldDistance > newDistance)
			//								InsertionX = area.Rectangle.X;*/
			//						//}
			//						
			//						var oldDistance = System.Math.Abs(x - InsertionX);
			//						var newDistance = System.Math.Abs(x - area.Rectangle.X);
			//						if (oldDistance > newDistance) //cancel move if the original spot is the closest drop point
			//						{
			//							InsertionDock = null;
			//							InsertionX = (i == dock.Addons.Count - 1 ? area.Rectangle.X : -1); //draw arrow at the beginning of the next label if it isn't the last label, otherwise, draw insertion arrow at the beginning of original spot (only case in which we want to draw insertion arrow in original spot)
			//						}
			//					}
			//					else
			//					{
			//						var oldDistance = System.Math.Abs(x - InsertionX);
			//						var newDistance = System.Math.Abs(x - addon.Label.LastX);
			//						if (oldDistance > newDistance)
			//						{
			//							InsertionDock = dock;
			//							InsertionX = addon.Label.LastX;
			//							InsertionIndex = oldIndex > -1 ? i-1 : i;
			//						}
			//						
			//						//if this is the last addon, check if it is closer to the end than the front of this label
			//						if (i == dock.Addons.Count - 1)
			//						{
			//							var endDistance = System.Math.Abs(x - (addon.Label.LastX + (addon.LabelSize > 0 ? addon.LabelSize : addon.Label.Width))); //addon.Label.LastX + addon.Label.Width));
			//							if (newDistance > endDistance)
			//							{
			//								//System.Console.Beep(250, 200);
			//								InsertionX += addon.Label.Width;
			//								InsertionIndex = oldIndex > -1 ? i : i+1;
			//							}
			//						}
			//					}
			//					
			//					
			//				}
			//			}
			//			else //vertical dock
			//			{
			//				InsertionX = -1; //dock.Addons[0].LastX;
			//				int oldIndex = -1;
			//				for (int i = 0; i < dock.Addons.Count; ++i) //IMenuAddon addon in dock.Addons)
			//				{
			//					var addon = dock.Addons[i];
			//					if (addon == CursorAddon)
			//					{
			//						oldIndex = i;
			//						//if (InsertionX == -1)
			//						//	InsertionX = area.Rectangle.X;
			//						//else
			//						//{
			//						/*	var oldDistance = System.Math.Abs(x - InsertionX);
			//							var newDistance = System.Math.Abs(x - area.Rectangle.X);
			//							if (oldDistance > newDistance)
			//								InsertionX = area.Rectangle.X;*/
			//						//}
			//						
			//						var oldDistance = System.Math.Abs(y - InsertionX);
			//						var newDistance = System.Math.Abs(y - area.Rectangle.Y);
			//						if (oldDistance > newDistance) //cancel move if the original spot is the closest drop point
			//						{
			//							InsertionDock = null;
			//							InsertionX = (i == dock.Addons.Count - 1 ? area.Rectangle.Y : -1); //draw arrow at the beginning of the next label if it isn't the last label, otherwise, draw insertion arrow at the beginning of original spot (only case in which we want to draw insertion arrow in original spot)
			//						}
			//					}
			//					else
			//					{
			//						var oldDistance = System.Math.Abs(y - InsertionX);
			//						var newDistance = System.Math.Abs(y - addon.Label.LastY);
			//						if (oldDistance > newDistance)
			//						{
			//							InsertionDock = dock;
			//							InsertionX = addon.Label.LastY;
			//							InsertionIndex = oldIndex > -1 ? i-1 : i;
			//						}
			//						
			//						//if this is the last addon, check if it is closer to the end than the front of this label
			//						if (i == dock.Addons.Count - 1)
			//						{
			//							var endDistance = System.Math.Abs(y - (addon.Label.LastY + addon.Label.Height)); //addon.Label.LastX + addon.Label.Width));
			//							if (newDistance > endDistance)
			//							{
			//								//System.Console.Beep(250, 200);
			//								InsertionX += addon.Label.Height;
			//								InsertionIndex = oldIndex > -1 ? i : i+1;
			//							}
			//						}
			//					}
			//					
			//					
			//				}
			//			}
            //
			//			break;
			//		}
			//	}
            //
			//	//draw the dragged label after calculating the insertion point with CursorAddon.Label.LastX, otherwise this will change LastX before calculation
			//	//CursorAddon.Label?.Paint(Hud, x, y, area.Rectangle.Width, area.Rectangle.Height);
			//	CursorAddon.Label.Width = area.Rectangle.Width;
			//	CursorAddon.Label.Height = area.Rectangle.Height;
			//	CursorAddon.Label.Paint(x, y);
			//	
			//	//draw insertion spot visuals
			//	if (InsertionDock is object)
			//	{
			//		SharpDX.DirectWrite.TextLayout arrow = null;
			//		switch (InsertionDock.Expand)
			//		{
			//			case MenuExpand.Up:
			//				arrow = mover.SelectedFont.GetTextLayout("🠗");
			//				mover.SelectedFont.DrawText(arrow, InsertionX - arrow.Metrics.Width*0.5f, InsertionDock.DropBox.Y - arrow.Metrics.Height);
			//				
			//				//debug
			//				//arrow = mover.SelectedFont.GetTextLayout(InsertionIndex.ToString());
			//				//mover.SelectedFont.DrawText(arrow, InsertionX - arrow.Metrics.Width*0.5f, InsertionDock.DropBox.Y - arrow.Metrics.Height*2);
			//				break;
			//			case MenuExpand.Down:
			//				arrow = mover.SelectedFont.GetTextLayout("🠕");
			//				mover.SelectedFont.DrawText(arrow, InsertionX - arrow.Metrics.Width*0.5f, InsertionDock.DropBox.Bottom);
			//				break;
			//			case MenuExpand.Left:
			//				arrow = mover.SelectedFont.GetTextLayout("🠖");
			//				mover.SelectedFont.DrawText(arrow, InsertionDock.DropBox.X - arrow.Metrics.Width, InsertionX - arrow.Metrics.Height*0.5f);
			//				
			//				//debug
			//				//arrow = mover.SelectedFont.GetTextLayout(InsertionIndex.ToString());
			//				//mover.SelectedFont.DrawText(arrow, InsertionDock.DropBox.X - arrow.Metrics.Width*2, InsertionX - arrow.Metrics.Height*0.5f);
			//				break;
			//			case MenuExpand.Right:
			//				arrow = mover.SelectedFont.GetTextLayout("🠔");
			//				mover.SelectedFont.DrawText(arrow, InsertionDock.DropBox.Right, InsertionX - arrow.Metrics.Height*0.5f);
			//				break;
			//		}
			//	}
            //
			//}
			//else if (PinnedAddons.ContainsKey(area.Name))
			//{
			//	if (Addons.TryGetValue(area.Name, out var addon)) //addon.Label is object)
			//	{
			//		if (!addon.Enabled)
			//			return;
            //
			//		float x = area.Rectangle.X + deltaX;
			//		float y = area.Rectangle.Y + deltaY;
            //
			//		//if not being dragged
			//		if (deltaX == 0 && deltaY == 0) // && (menu.Height != area.Rectangle.Width || menu.Width != area.Rectangle.Width))
			//		{
			//			if (addon.Panel is object && LabelDecorator.IsVisible(addon.Panel))
			//			{
			//				bool hovered = addon.Panel.Hovered;
			//				
			//				//determine where the anchor for the area should be vertically
			//				IMenuDock dock = Docks[addon.DockId];
			//				if (dock.Expand == MenuExpand.Up && addon.Panel.Height != area.Rectangle.Height) //anchor is at the bottom
			//					y = area.Rectangle.Bottom - addon.Panel.Height;
			//				
			//				//determine where the anchor for the area should be horizontally
			//				if (dock.Alignment == HorizontalAlign.Right)
			//					x = area.Rectangle.Right + deltaX - addon.Panel.Width;
			//				
			//				//window boundary checking
			//				if (x + addon.Panel.Width > Hud.Window.Size.Width)
			//					x = Hud.Window.Size.Width - addon.Panel.Width;
			//				
			//				//change the area position or dimensions if needed
			//				area.SetRectangle(x, y, addon.Panel.Width, addon.Panel.Height); //this MovableArea function bypasses the sizing history save
			//				
			//				//paint
			//				addon.Panel.Paint(x, y);
			//				
			//				//title bar delay handling if hover state changed
			//				if (addon.Panel.Hovered != hovered)
			//				{
			//					ResetLabelDelays(addon.Panel);
			//					ToggleLabelDelays(addon.Panel, true, addon.Panel.Hovered);
			//				}
			//			}
			//		}
			//		else
			//		{
			//			addon.Panel.Paint(x, y);
			//		}
			//	}
			//}
			//else if (Addons.ContainsKey(area.Name))
			//{
			//	PinnedAddons.Add(area.Name, area);
			//}
		}
		
		public void Log(string path)
		{
			TextBuilder.Clear();
			TextBuilder.Append("/*\n\tThis file contains \"menu\" plugin priorities and addon settings.\n\tChange the file extension from .txt to .cs and move this file into the TurboHUD / plugins / User folder\n*/\n\n");
			TextBuilder.Append("namespace Turbo.Plugins.User\n{\n");
			TextBuilder.Append("\tusing Turbo.Plugins.Default;\n");
			TextBuilder.Append("\tusing Turbo.Plugins.Razor.Menu;\n\n"); //Turbo.Plugins.Razor.Menu //this.GetType().Namespace
			//TextBuilder.AppendFormat("\tpublic class {0} : BasePlugin, ICustomizer\n\t{\n", ConfigFileName); //can't use { or } in AppendFormat without escaping it
			TextBuilder.AppendFormat("\tpublic class {0} : BasePlugin, ICustomizer\n", ConfigFileName);
			TextBuilder.Append("\t{\n");
			TextBuilder.AppendFormat("\t\tpublic {0}() ", ConfigFileName);
			TextBuilder.Append("{ Enabled = true; }\n\n");
			TextBuilder.Append("\t\tpublic override void Load(IController hud) { base.Load(hud); }\n\n");
			TextBuilder.Append("\t\tpublic void Customize()\n\t\t{\n");
			//TextBuilder.AppendFormat("\t\t\tHud.RunOnPlugin<{0}>(plugin =>\n\t\t\t{\n", this.GetType().Name);
			TextBuilder.AppendFormat("\t\t\tHud.RunOnPlugin<{0}>(plugin =>\n", this.GetType().Name);
			TextBuilder.Append("\t\t\t{\n");
			//TextBuilder.Append(string.Format("\t\t\tHud.RunOnPlugin<{0}>(plugin =>\n\t\t\t{\n", this.GetType().Name)); //"\t\t\tHud.RunOnPlugin<MovableController>(plugin =>\n\t\t\t{\n"
			TextBuilder.Append("\t\t\t\t//ConfigureDock(string dockId, params string[])\n");
			TextBuilder.Append("\t\t\t\t//ConfigureAddon(string addonId, bool enabled, string config)\n");
			
			foreach (IMenuDock dock in Docks.Values)
			{
				if (dock.Addons.Count == 0)
					continue;
				
				TextBuilder.AppendFormat("\n\t\t\t\tplugin.ConfigureDock(\"{0}\", \"{1}\");\n", dock.Id, string.Join("\", \"", dock.Addons.Select(a => a.Id)));
				
				foreach (IMenuAddon addon in dock.Addons)
				{
					//TextBuilder.AppendFormat("\t\t\t\tplugin.ToggleAddon(\"{0}\", {1});\n", addon.Id, ((IPlugin)addon).Enabled.ToString().ToLower());
					TextBuilder.AppendFormat("\t\t\t\tplugin.ConfigureAddon(\"{0}\", {1}, \"{2}\");\n", addon.Id, addon.Enabled.ToString().ToLower(), addon.Config);
				}
			}

			TextBuilder.Append("\t\t\t});\n\t\t}\n\t}\n}");
			
			string filePath = (string.IsNullOrEmpty(ConfigFilePath) ? ConfigFileName : ConfigFilePath + "\\" + ConfigFileName) + ".cs";
			
			if (!string.IsNullOrEmpty(path) && !filePath.StartsWith(path, StringComparison.OrdinalIgnoreCase))
				filePath = path + @"\" + filePath;

			System.IO.File.WriteAllText(filePath, TextBuilder.ToString());
		}
		
		public void ConfigureDock(string dockId, params string[] addons)
		{
			if (!ConfigDock.ContainsKey(dockId))
				ConfigDock.Add(dockId, addons);
			else
				ConfigDock[dockId] = addons;
		}
		
		/*public void ToggleAddon(IMenuAddon addon, bool enabled) //string addonId, bool enabled)
		{
			if (((IPlugin)addon).Enabled != enabled)
				((IPlugin)addon).Enabled = enabled;
			
			Hud.TextLog.Queue(this);
		}*/
		
		public void ConfigureAddon(string addonId, bool enabled, string config)
		{
			// if (!CustomizeAddon.ContainsKey(addonId))
				// CustomizeAddon.Add(addonId, enabled);
			// else
				CustomizeAddon[addonId] = new Tuple<bool, string>(enabled, config);
			
			// if (!ConfigAddon.ContainsKey(addonId))
				// ConfigAddon.Add(addonId, config);
			// else
				//ConfigAddon[addonId] = config;
		}
		
		public void SaveConfig(IMenuAddon addon, string config)
		{
			addon.Config = config;
			Hud.TextLog.Queue(this, 3);
		}
		
		public void Save()
		{
			Hud.TextLog.Queue(this, 3);
		}
		
		public LabelStringDecorator CreatePin(IMenuAddon addon)
		{
			return new LabelStringDecorator(Hud, PinSymbol)
			{
				Hint = PinHint,
				Alignment = HorizontalAlign.Right,
				Font = PinFont,
				OnBeforeRender = (label) => {
					if (PinnedAddons.ContainsKey(addon.Id))
					{
						((LabelStringDecorator)label).Font = PinnedFont;
						((LabelStringDecorator)label).Hint = PinnedHint;
					}
					else
					{
						((LabelStringDecorator)label).Font = PinFont;
						((LabelStringDecorator)label).Hint = PinHint;
					}
					return true;
				},
				OnClick = (label) => PinAddon(addon), //this.PinAddon,
				//Hint = "Click to (un)pin this menu open",
				SpacingLeft = 5,
				SpacingRight = 5,
				//SpacingTop = -2
			};
		}
		
		public LabelStringDecorator CreateReset(System.Action<ILabelDecorator> resetFunc)
		{
			return new LabelStringDecorator(Hud, ResetSymbol)
			{
				Hint = CreateHint(TextReset),
				Alignment = HorizontalAlign.Right,
				Font = PinFont,
				OnClick = resetFunc, //(label) => PinAddon(addon), //this.PinAddon,
				//Hint = "Click to (un)pin this menu open",
				SpacingLeft = 5,
				SpacingRight = 5,
				SpacingTop = -3
			};
		}
		
		/*public LabelStringDecorator CreatePause(LabelGraphDecorator graph) //, Action<ILabelDecorator> clickFunc = null)
		{
			return new LabelStringDecorator(Hud) {
				Hint = PauseHint,
				Alignment = HorizontalAlign.Right, 
				SpacingLeft = 5, 
				SpacingRight = 5, 
				SpacingTop = -2,
				OnBeforeRender = (label) => {
					var lbl = (LabelStringDecorator)label;
					if (graph.IsPaused())
					{
						lbl.StaticText = ResumeSymbol; //"▶";
						lbl.Font = ResumeFont;
						lbl.Hint = ResumeHint;
					}
					else
					{
						lbl.StaticText = PauseSymbol; //"⏸";
						lbl.Font = PauseFont;
						lbl.Hint = PauseHint;
					}
					return true;
				},
				OnClick = (label) => graph.TogglePause(!graph.IsPaused()),
			};
		}*/
		
		public LabelStringDecorator CreateSave(IMenuSaveHandler handler)
		{
			return new LabelStringDecorator(Hud, "🖫") {
				Hint = SaveOffHint,
				//Font = PinFont,
				Alignment = HorizontalAlign.Right, 
				SpacingLeft = 5, 
				SpacingRight = 5, 
				//SpacingTop = -2,
				OnBeforeRender = (label) => {
					var lbl = (LabelStringDecorator)label;
					if (handler.SaveMode)
					{
						lbl.Font = PinnedFont;
						lbl.Hint = SaveOnHint;
					}
					else
					{
						lbl.Font = PinFont;
						lbl.Hint = SaveOffHint;
					}
					return true;
				},
				OnClick = (label) => handler.SaveMode = !handler.SaveMode,
			};
		}
		
		public LabelStringDecorator CreateSave(System.Action<ILabelDecorator> saveFunc)
		{
			return new LabelStringDecorator(Hud, "🖫") {
				Hint = SaveHint,
				Font = PinFont,
				OnClick = saveFunc,
				Alignment = HorizontalAlign.Right, 
				SpacingLeft = 5, 
				SpacingRight = 5, 
				//SpacingTop = -2,
			};
		}

		public LabelStringDecorator CreateDelete(string filepath)
		{
			int updateTick = 0;
			return new LabelStringDecorator(Hud, "❌") { //"ⓧ"
				Hint = DeleteConfigHint,
				Font = ResumeFont, //PinnedFont, //Hud.Render.CreateFont("tahoma", 13, 255, 255, 0, 0, false, false, true),
				Alignment = HorizontalAlign.Right, 
				SpacingLeft = 5, 
				SpacingRight = 5,
				SpacingTop = 2,
				Enabled = System.IO.File.Exists(filepath),
				OnBeforeRender = (label) => {
					int diff = Hud.Game.CurrentGameTick - updateTick;
					if (diff < 0 || diff > 120)
					{
						updateTick = Hud.Game.CurrentGameTick;
						label.Enabled = System.IO.File.Exists(filepath);
					}
					return label.Enabled;
				},
				OnClick = (label) => {
					System.IO.File.Delete(filepath);
					label.Enabled = System.IO.File.Exists(filepath);
				}
				//SpacingTop = -2,
			};

		}
		
		public LabelRowDecorator CreatePagingControls(LabelGraphDecorator graph)
		{
			/*return new LabelRowDecorator(Hud,
				new LabelStringDecorator(Hud, "◀") {Font = PauseFont, SpacingLeft = 5, SpacingRight = 5, SpacingTop = -2}, //left
				new LabelStringDecorator(Hud, "◀") {Font = PauseFont, SpacingLeft = 5, SpacingRight = 5, SpacingTop = -2}, //pause
				new LabelStringDecorator(Hud, "▶") {Font = PauseFont, SpacingLeft = 5, SpacingRight = 5, SpacingTop = -2} //right
			);*/
			return new LabelRowDecorator(Hud,
				new LabelStringDecorator(Hud, "⏪") {Hint = PageNewerHint, OnClick = (label) => graph.ShowNewer(), Font = PauseFont, SpacingRight = 5, SpacingTop = -2}, //left ◀ ⏪
				new LabelStringDecorator(Hud, PauseSymbol) {Hint = PauseHint, OnClick = (label) => graph.TogglePause(!graph.IsPaused()), Font = PauseFont, SpacingLeft = 5, SpacingRight = 5, SpacingTop = -2}, //pause ❚❚ ⏸️
				new LabelStringDecorator(Hud, "⏩") {Hint = PageOlderHint, OnClick = (label) => graph.ShowOlder(), Font = PauseFont, SpacingLeft = 5, SpacingTop = -2} //right ▶ ❯❯ ⏩
			) {
				Alignment = HorizontalAlign.Right,
				SpacingTop = 5,
				SpacingBottom = 5,
				OnBeforeRender = (label) => {
					var newer = (LabelStringDecorator)((LabelRowDecorator)label).Labels[0];
					var pause = (LabelStringDecorator)((LabelRowDecorator)label).Labels[1];
					var older = (LabelStringDecorator)((LabelRowDecorator)label).Labels[2];
					
					if (graph.IsNewestShown())
					{
						newer.Font = DisabledFont; //plugin.ResumeFont;
						newer.Hint = null;						
					}
					else
					{
						newer.Font = PauseFont;
						newer.Hint = PageNewerHint;
					}
					
					if (graph.IsPaused())
					{
						pause.StaticText = ResumeSymbol; //"▶"; ⏯️
						if (graph.IsAutoPaused is object && graph.IsAutoPaused())
						{
							pause.Font = DisabledFont;
							pause.Hint = null;
						}
						else
						{
							pause.Font = ResumeFont;
							pause.Hint = ResumeHint;
						}
					}
					else
					{
						pause.StaticText = PauseSymbol; //"⏸";
						pause.Font = PauseFont;
						pause.Hint = PauseHint;
					}
					
					if (graph.IsOldestShown())
					{
						older.Font = DisabledFont;
						older.Hint = null;
					}
					else
					{
						older.Font = PauseFont;
						older.Hint = PageOlderHint;
					}

					//if (graph.OldestTime.HasValue)
					LabelDecorator.DebugWrite("\nIsPaused? " + graph.IsPaused() + "\nIsOldestShown? " + graph.IsOldestShown() + "\nIsNewestShown? " + graph.IsNewestShown(), label.LastX + label.Width + 30, label.LastY); //graph.OldestTime.Value.ToString("T") + " vs " + (graph.PausedTime.HasValue ? graph.PausedTime.Value : Hud.Time.Now).ToString("T") + 
				
					
					return true;
				}
			};
		}
		
		public LabelStringDecorator CreateHint(string text)
		{
			return new LabelStringDecorator(Hud, text) {Font = TooltipFont, SpacingLeft = 3, SpacingRight = 3, SpacingTop = 3, SpacingBottom = 3};
		}

		public LabelStringDecorator CreateHint(System.Func<string> textFunc)
		{
			return new LabelStringDecorator(Hud, textFunc) {Font = TooltipFont, SpacingLeft = 3, SpacingRight = 3, SpacingTop = 3, SpacingBottom = 3};
		}
		
		public void PinAddon(IMenuAddon addon)
		{
			//can only get to this point if addon.Menu is object
			//if (addon.Menu.HoveredMenuItem is object && FindHoveredDecorator(addon.Menu.HoveredMenuItem) is MenuButtonDecorator) //addon.Menu is object && 
			//{
				//System.Console.Beep(150, 50);
				//Hud.Sound.Speak("Pin");
				
				if (PinnedAddons.ContainsKey(addon.Id)) //unpin
				{
					//Hud.Sound.Speak("Unpin "+addon.Id);
					//remove the area and revoke pin state
					//string id = addon.Id;
					//Mover.DeleteArea(PinnedAddons[addon.Id]); //this locks up menu plugin
					PinnedAddons[addon.Id].DeleteOnDisable = true;
					PinnedAddons[addon.Id].Enabled = false;
					PinnedAddons.Remove(addon.Id);
					
					addon.Label.Hint = null;
					
					ToggleLabelDelays(addon.Panel, false, true);
					
					//clear the flags
					//addon.Menu.HoveredMenuItem = null;
					//addon.Menu.HoveredDecorator = null;
					//Docks[addon.DockId].HoveredAddon = null;
				}
				else //if (addon.Menu.TitleBar is object)
				{
					//pin 
					//Hud.Sound.Speak("Pin "+addon.Id);
					//create new movable area
					MovableArea area = Mover.CreateArea(
						this,
						addon.Id, //area name
						new System.Drawing.RectangleF(addon.Panel.LastX, addon.Panel.LastY, addon.Panel.Width, addon.Panel.Height),
						true, //enabled at start?
						true, //save to config file?
						ResizeMode.Off//, //resizable?
						//ClipState.BeforeClip //AfterClip //draw above unpinned content
					);
					area.DeleteOnDisable = true;
					
					PinnedAddons.Add(addon.Id, area);
					//PinnedAddons.Add(addon.Id, null);
					
					//addon.Label.Hint = addon.LabelHint;
					
					ToggleLabelDelays(addon.Panel, true, true);
				}
			//}
			
		}
		
		public string MillisecondsToString(long milliseconds)
		{
			TimeSpan timeSpent = TimeSpan.FromMilliseconds(milliseconds);
			if (timeSpent.Days > 0)
				//TimeToLevel[i] = ValueToString(timeRequired.Days, ValueFormat.NormalNumberNoDecimal) + timeRequired.ToString("'d 'hh'h 'mm'm 'ss's'"); //text +=
				return ValueToString(timeSpent.Days, ValueFormat.NormalNumberNoDecimal) + timeSpent.ToString("'d 'hh'h 'mm'm 'ss's'"); //text +=
			if (timeSpent.Hours > 0)
				//TimeToLevel[i] = timeRequired.ToString("h'h 'mm'm 'ss's'");
				return timeSpent.ToString("h'h 'mm'm 'ss's'");
			if (timeSpent.Minutes > 0)
				//TimeToLevel[i] = timeRequired.ToString("m'm 'ss's'");
				return timeSpent.ToString("m'm 'ss's'");
			//else 
				//TimeToLevel[i] = timeRequired.ToString("%s's'");
			return timeSpent.ToString("%s's'");
		}

		private void ToggleLabelDelays(ILabelDecorator label, bool isPinned, bool isHovered)
		{
			if (label == null)
				return;
			
			if (label is LabelDelayedDecorator)
			{
				label.OnBeforeRender = isPinned ? (isHovered ? RenderYes : RenderNo) : null;
				
				if (OverridenDelays.Contains(label))
				{
					((LabelDelayedDecorator)label).Delay = 0;
					OverridenDelays.Remove(((LabelDelayedDecorator)label));
				}
				else if (((LabelDelayedDecorator)label).Delay == 0)
				{
					((LabelDelayedDecorator)label).Delay = HoverDelay;
					OverridenDelays.Add(((LabelDelayedDecorator)label));
				}
			}
			else if (label is ILabelDecoratorCollection)
			{
				foreach (var lbl in ((ILabelDecoratorCollection)label).Labels)
					ToggleLabelDelays(lbl, isPinned, isHovered);
			}
		}
		
		private void ResetLabelDelays(ILabelDecorator label)
		{
			if (label == null)
				return;
			
			if (label is LabelDelayedDecorator)
			{
				if (OverridenDelays.Contains(label))
					((LabelDelayedDecorator)label).Reset();
			}
			else if (label is ILabelDecoratorCollection)
			{
				foreach (var lbl in ((ILabelDecoratorCollection)label).Labels)
					ResetLabelDelays(lbl);
			}
		}
	}
}