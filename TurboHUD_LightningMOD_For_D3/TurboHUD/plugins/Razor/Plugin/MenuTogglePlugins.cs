namespace Turbo.Plugins.Razor.Plugin
{
	using SharpDX.DirectWrite;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Text; //StringBuilder
	using System.IO; //File.WriteAllText, tier 3 only

	using Turbo.Plugins.Default;
	using Turbo.Plugins.Razor.Log;
	using Turbo.Plugins.Razor.Label;
	using Turbo.Plugins.Razor.Menu;
	using Turbo.Plugins.Razor.Plugin;

	public class MenuTogglePlugins : BasePlugin, IMenuAddon, ICustomizer, ITextLogger, IPluginManifest //, IMenuSaveHandler , IAfterCollectHandler , IInGameTopPainter, ILeftClickHandler, IRightClickHandler
	{
		//IPluginManifest
		public string Name { get; set; } //optional display name
        public string Description { get; set; } = "A menu addon for toggling plugins on and off dynamically in-game.";
		public string Version { get; set; } = "14-Sept-2021";
		public List<string> Dependencies { get; set; } = new List<string>() {
			"Turbo.Plugins.Razor.Menu.MenuPlugin",
		};
		
		//MenuTogglePlugins properties
		public string ConfigFileName { get; set; } = "CustomPluginEnablerOrDisabler";
		public string ConfigFilePath { get; set; } = "User"; //@"plugins\User\"; //tier 3 only
		public List<string> DefaultNamespaces { get; set; } = new List<string>() {"Turbo.Plugins.Default", "Turbo.Plugins.Enhanced"}; //, "Turbo.PluginsX"
		public List<string> Hidden { get; set; } = new List<string>() {
			//"Turbo.Plugins.Razor.Click.",
			"Turbo.Plugins.Razor.Hotkey.",
			"Turbo.Plugins.Razor.Label.",
			"Turbo.Plugins.Razor.Log.",
			"Turbo.Plugins.Razor.Menu.MenuPlugin",
			"Turbo.Plugins.Razor.Movable.",
			"Turbo.Plugins.Razor.Plugin.",
		};
		
		public List<PluginFilter> Filters { get; set; }
		public class PluginFilter {
			public string Name { get; set; }
			public Func<IPlugin, bool> Check { get; set; }
			public string ConfigFile { get; set; }
			public string ConfigFilePath { get; set; }
			
			public IPlugin[] Plugins { get; set; }
			public LabelRowDecorator Decorator { get; set; }
			public int Skip { get; set; }
		}
		private int MaxPluginsPerFilter;

		public List<IPluginManifest> Manifests { get; set; } = new List<IPluginManifest>(); //for retrofitting IPluginManifest to describe old plugins without altering their code
		private Dictionary<string, IPluginManifest> ManifestMap { get; set; } = new Dictionary<string, IPluginManifest>(); //for retrofitting IPluginManifest to describe old plugins without altering their code
		private Dictionary<string, List<string>> Dependents = new Dictionary<string, List<string>>(); //plugin, list of plugins that have declared (via IPluginManifest) that it requires it to be enabled
		
		//IMenuAddon
		public ILabelDecorator Label { get; set; }
		public ILabelDecorator LabelHint { get; set; }
		public float LabelSize { get; set; }
		public ILabelDecorator Panel { get; set; }

		public string Id { get; set; }
		public int Priority { get; set; } //the priority on the dock to show this addon (smaller to the left, higher to the right)
		public string DockId { get; set; }
		public string Config { get; set; }

		//MenuTogglePlugins 
		public IFont TextFont { get; set; }
		public IFont DefaultFont { get; set; }
		public IFont FadedFont { get; set; }
		public IFont EnabledFont { get; set; }
		public IFont DisabledFont { get; set; }
		public IFont TooltipSectionFont { get; set; }
		public IFont TooltipEnabledFont { get; set; }
		public IFont TooltipDisabledFont { get; set; }
		
		public IBrush HoveredBrush { get; set; }
		public IBrush DrawBrush { get; set; }
		public IBrush SelectedBrush { get; set; }
		public LabelStringDecorator HintActivate { get; set; }
		public LabelStringDecorator HintDeactivate { get; set; }
		public LabelColumnDecorator HintPerformance { get; set; }
		public LabelColumnDecorator HintManifest { get; set; }
		public LabelColumnDecorator HintPlugin { get; set; }
		
		public string TextEnabled { get; set; } = "✔️";
		public string TextDisabled { get; set; } = "❌";
		
		private IPlugin HoveredPlugin;
		private string HoveredPluginKey;
		private IPluginManifest HoveredPluginManifest;
		
		private IPluginManifest HoveredManifest;
		private LabelTableDecorator Table;
		private LabelRowDecorator SelectUI;
		private PluginFilter SelectedFilter;
		private PluginFilter SaveFilter;
		private HashSet<PluginFilter> SaveFilters = new HashSet<PluginFilter>();

		//private LabelStringDecorator SaveUI;
		private LabelStringDecorator DeleteUI;
		private bool DeleteUIVisible;
		private int FileCheckUpdateTick;
		private MenuPlugin Plugin;
		private Dictionary<string, bool> ToggleStates = new Dictionary<string, bool>();
		//private int ColumnCount;
		//private int PluginsPerColumn;
		private string ConfigFile;
		private StringBuilder TextBuilder;
		
        public MenuTogglePlugins()
        {
            Enabled = true;
			Priority = 30;
			DockId = "TopRight";
			Order = int.MaxValue - 1;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
			
			TextBuilder = new StringBuilder();
			
			Filters = new List<PluginFilter>() {
				new PluginFilter() {Name = "Custom Plugins", ConfigFile = "CustomPluginEnablerOrDisabler", Check = (plugin) => !Filters[1].Check(plugin) && !Filters[2].Check(plugin)}, //!(plugin is IMenuAddon) && 
				new PluginFilter() {Name = "Default Plugins", ConfigFile = "DefaultPluginEnablerOrDisabler", Check = (plugin) => DefaultNamespaces.Any(plugin.GetType().Namespace.StartsWith)}, //plugin.GetType().Namespace.StartsWith("Turbo.Plugins.Default") || plugin.GetType().Namespace.StartsWith("Turbo.Plugins.Enhanced")},	
				new PluginFilter() {Name = "Lightning Mod", ConfigFile = "LightningModPluginEnablerOrDisabler", Check = (plugin) => plugin.GetType().Namespace.StartsWith("Turbo.Plugins.LightningMod", StringComparison.OrdinalIgnoreCase)},
				//new PluginFilter() {Name = "All", ConfigFile = null, Check = (plugin) => true},
				//new PluginFilter() {Name = "Movable", ConfigFile = "MovablePluginEnablerOrDisabler", Check = (plugin) => plugin is Razor.Movable.IMovable},
			};
        }
		
		public void OnRegister(MenuPlugin plugin)
		{
			TextFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 0, 150, 225, false, false, true);
			DefaultFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 255, 255, 225, false, false, true);
			FadedFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 100, 100, 100, false, false, true);
			EnabledFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 0, 255, 0, false, false, true);
			DisabledFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 255, 0, 0, false, false, true); //170, 150, 120
			
			TooltipSectionFont = Hud.Render.CreateFont("tahoma", plugin.TooltipFontSize, 255, 0, 150, 225, true, false, true);
			TooltipEnabledFont = Hud.Render.CreateFont("tahoma", plugin.TooltipFontSize, 255, 0, 255, 0, true, false, true);
			TooltipDisabledFont = Hud.Render.CreateFont("tahoma", plugin.TooltipFontSize, 255, 255, 0, 0, true, false, true);
			
			HoveredBrush = plugin.HighlightBrush;
			DrawBrush = Hud.Render.CreateBrush(255, 255, 255, 255, 1f);
			SelectedBrush = Hud.Render.CreateBrush(255, 0, 150, 225, 1f);
			
			//Label
			Label = new LabelCanvasDecorator(Hud, (label, x, y) => {
				DrawBrush.DrawRectangle(x, y, plugin.MenuHeight*0.5f - 2, plugin.MenuHeight*0.5f - 2);
				DrawBrush.DrawRectangle(x+plugin.MenuHeight*0.25f, y+plugin.MenuHeight*0.25f, plugin.MenuHeight*0.5f - 2, plugin.MenuHeight*0.5f - 2);
			}) {ContentHeight = plugin.MenuHeight - 7, ContentWidth = plugin.MenuHeight - 7, SpacingLeft = 4, SpacingRight = 3, SpacingTop = 4, SpacingBottom = 3}; //new LabelStringDecorator(Hud, "🔌") {Font = TextFont}; //LabelTextureDecorator(Hud, Hud.Texture.GetTexture(363251, 0)) {TextureHeight = 38, ContentHeight = plugin.MenuHeight}; //2710494966 //new LabelStringDecorator(Hud, "⚙") {Font = TextFont};
			
			HintPerformance = new LabelColumnDecorator(Hud,
				new LabelStringDecorator(Hud, "Performance") {Font = TooltipSectionFont, Alignment = HorizontalAlign.Left},
				new LabelTableDecorator(Hud,
					new LabelRowDecorator(Hud,
						new LabelStringDecorator(Hud) {Font = plugin.TooltipFont, Alignment = HorizontalAlign.Right, SpacingLeft = 10, SpacingRight = 10},
						new LabelStringDecorator(Hud) {Font = plugin.TooltipFont, Alignment = HorizontalAlign.Left, SpacingLeft = 10, SpacingRight = 10}
					)
				) {
					SpacingTop = 5,
					SpacingBottom = 5,
					OnFillRow = (row, index) => {
						//if (SelectedFilter == null || Table.HoveredRow <= 0 || Table.HoveredRow >= SelectedFilter.Plugins.Length || index >= SelectedFilter.Plugins[Table.HoveredRow].PerformanceCounters.Count)
						if (HoveredPlugin == null || index >= HoveredPlugin.PerformanceCounters.Count)
							return false;
						
						KeyValuePair<string, IPerfCounter> pair = HoveredPlugin.PerformanceCounters.ElementAt(index); //SelectedFilter.Plugins[Table.HoveredRow + SelectedFilter.Skip].PerformanceCounters.ElementAt(index);
						((LabelStringDecorator)row.Labels[0]).StaticText = pair.Key;
						((LabelStringDecorator)row.Labels[1]).StaticText = pair.Value.LastValue.ToString("0.##");
						
						return true;
					}
				}
			) {Alignment = HorizontalAlign.Left};
			
			HintManifest = new LabelColumnDecorator(Hud,
				new LabelStringDecorator(Hud) {Font = TooltipSectionFont/*plugin.TooltipFont*/, SpacingBottom = 5}, //nameUI
				new LabelStringDecorator(Hud) {Font = plugin.TooltipFont, SpacingBottom = 5}, //descriptionUI
				new LabelStringDecorator(Hud, "Requires") {Font = TooltipSectionFont, Alignment = HorizontalAlign.Left}, //requiresUI
				new LabelTableDecorator(Hud, //requiresUI
					new LabelRowDecorator(Hud,
						new LabelStringDecorator(Hud) {Font = plugin.TooltipFont, Alignment = HorizontalAlign.Right, SpacingLeft = 10, SpacingRight = 10, SpacingTop = 2},
						new LabelStringDecorator(Hud) {Font = plugin.TooltipFont, Alignment = HorizontalAlign.Left, SpacingLeft = 10, SpacingRight = 10, SpacingTop = 2}
					)
				) {
					SpacingBottom = 5,
					OnFillRow = (row, index) => {
						if (HoveredPluginManifest == null)
							return false;
						
						if (index >= HoveredPluginManifest.Dependencies.Count)
							return false;
						
						//((LabelStringDecorator)row.Labels[0]).StaticText = pair.Key;
						((LabelStringDecorator)row.Labels[1]).StaticText = HoveredPluginManifest.Dependencies[index]; //((IPluginManifest)HoveredPlugin).Dependencies[index];
						
						return true;
					}
				},
				new LabelStringDecorator(Hud, "Required By") {Font = TooltipSectionFont, Alignment = HorizontalAlign.Left},
				new LabelTableDecorator(Hud, //requiresUI
					new LabelRowDecorator(Hud,
						new LabelStringDecorator(Hud) {Font = plugin.TooltipFont, Alignment = HorizontalAlign.Right, SpacingLeft = 10, SpacingRight = 10, SpacingTop = 2},
						new LabelStringDecorator(Hud) {Font = plugin.TooltipFont, Alignment = HorizontalAlign.Left, SpacingLeft = 10, SpacingRight = 10, SpacingTop = 2}
					)
				) {
					SpacingBottom = 5,
					OnFillRow = (row, index) => {
						if (HoveredPlugin == null)
							return false;
						
						if (!Dependents.ContainsKey(HoveredPluginKey))
							return false;
						
						var dep = Dependents[HoveredPluginKey];
						if (index >= dep.Count)
							return false;
						
						//((LabelStringDecorator)row.Labels[0]).StaticText = pair.Key;
						((LabelStringDecorator)row.Labels[1]).StaticText = dep[index];
						
						var plug = Hud.AllPlugins.FirstOrDefault(p => p.GetType().ToString().Equals(dep[index], StringComparison.OrdinalIgnoreCase));
						if (plug is object)
							((LabelStringDecorator)row.Labels[1]).Font = plug.Enabled ? TooltipEnabledFont : TooltipDisabledFont;
						
						return true;
					}
				}

			) {
				//SpacingTop = 5,
				//SpacingBottom = 5,
				OnBeforeRender = (label) => {
					if (HoveredPlugin == null)
						return false;
					
					//does this plugin have a manifest associated with it?
					if (HoveredPluginManifest is object)
					{
						var nameUI = (LabelStringDecorator)((LabelColumnDecorator)label).Labels[0];
						nameUI.StaticText = HoveredPluginManifest.Name ?? HoveredPlugin.GetType().Name;
						//nameUI.Font = (HoveredPlugin.Enabled ? );
						
						var descriptionUI = (LabelStringDecorator)((LabelColumnDecorator)label).Labels[1];
						descriptionUI.StaticText = HoveredPluginManifest.Description;
						descriptionUI.Enabled = !string.IsNullOrEmpty(descriptionUI.StaticText);
						
						//requiresUI
						if (HoveredPluginManifest.Dependencies is object && HoveredPluginManifest.Dependencies.Count > 0)
						{
							((LabelColumnDecorator)label).Labels[2].Enabled = true;
							((LabelColumnDecorator)label).Labels[3].Enabled = true;
						}
						else
						{
							((LabelColumnDecorator)label).Labels[2].Enabled = false;
							((LabelColumnDecorator)label).Labels[3].Enabled = false;
						}
					}
					else
					{
						var nameUI = (LabelStringDecorator)((LabelColumnDecorator)label).Labels[0];
						nameUI.StaticText = HoveredPlugin.GetType().Name;
						
						//descriptionUI = (LabelStringDecorator)((LabelColumnDecorator)label).Labels[1];
						((LabelColumnDecorator)label).Labels[1].Enabled = false;
						
						//requiresUI
						((LabelColumnDecorator)label).Labels[2].Enabled = false;
						((LabelColumnDecorator)label).Labels[3].Enabled = false;
					}

					//requiredbyUI
					if (Dependents.ContainsKey(HoveredPluginKey))
					{
						((LabelColumnDecorator)label).Labels[4].Enabled = true;
						((LabelColumnDecorator)label).Labels[5].Enabled = true;
					}
					else
					{
						((LabelColumnDecorator)label).Labels[4].Enabled = false;
						((LabelColumnDecorator)label).Labels[5].Enabled = false;
					}
					
					return true;
				}
			};
			
			HintActivate = plugin.CreateHint("Click to Activate");
			HintDeactivate = plugin.CreateHint("Click to Deactivate");
			HintActivate.Font = TooltipEnabledFont;
			HintDeactivate.Font = TooltipDisabledFont;
			
			HintPlugin = new LabelColumnDecorator(Hud,
				HintManifest,
				HintPerformance,
				HintActivate
			) {
				SpacingTop = 5, 
				SpacingBottom = 5,
				SpacingLeft = 5,
				SpacingRight = 5,
				OnBeforeRender = (label) => {
					if (SelectedFilter == null || Table.HoveredRow < 0 || Table.HoveredRow + SelectedFilter.Skip >= SelectedFilter.Plugins.Length)
					{
						HoveredPlugin = null;
						HoveredPluginKey = null;
						HoveredPluginManifest = null;
						return false;
					}
					
					var hovered = SelectedFilter.Plugins[Table.HoveredRow + SelectedFilter.Skip];
					if (HoveredPlugin != hovered)
					{
						HoveredPlugin = hovered; //SelectedFilter.Plugins[Table.HoveredRow + SelectedFilter.Skip];
						HoveredPluginKey = HoveredPlugin.GetType().ToString();
						HoveredPluginManifest = hovered is IPluginManifest ? (IPluginManifest)hovered : (ManifestMap.ContainsKey(HoveredPluginKey) ? ManifestMap[HoveredPluginKey] : null); //FirstOrDefault(m => m is PluginManifest && HoveredPluginKey.Equals(((PluginManifest)m).Path, StringComparison.OrdinalIgnoreCase));
					}
					//((ILabelDecoratorCollection)label).Labels[0].Enabled = HoveredPlugin is IPluginManifest || Dependents.ContainsKey(HoveredPluginKey);

					return true;
				}
			};
			
			Table = new LabelTableDecorator(Hud, 
				new LabelRowDecorator(Hud, 
					new LabelStringDecorator(Hud, TextEnabled) {Font = EnabledFont, SpacingTop = 3, SpacingBottom = 3, SpacingLeft = 5},
					new LabelStringDecorator(Hud) {Font = TextFont, Alignment = HorizontalAlign.Right}, //count
					new LabelAlignedDecorator(Hud,
						new LabelStringDecorator(Hud) {Font = FadedFont, Alignment = HorizontalAlign.Left}, //namespace
						new LabelStringDecorator(Hud) {Font = TextFont, Alignment = HorizontalAlign.Left}, //plugin name
						new LabelStringDecorator(Hud, "🗎") {/*Hint = HintManifest, */Font = DefaultFont, Alignment = HorizontalAlign.Left, SpacingLeft = 3,SpacingRight = 5}//, //plugin name //📊📈
						//new LabelStringDecorator(Hud, "📊") {Hint = HintPerformance, Font = TextFont, SpacingLeft = 3} //plugin name //📊📈
					) {Alignment = HorizontalAlign.Left, SpacingTop = 3, SpacingBottom = 3, SpacingLeft = 3, SpacingRight = 3}
				) {Hint = HintPlugin, OnClick = OnClick}
			) {
				BackgroundBrush = plugin.BgBrush,
				HoveredBrush = plugin.HighlightBrush,
				SpacingLeft = 10, 
				SpacingRight = 10,
				SpacingBottom = 5,
				FillWidth = false,
				Count = () => MaxPluginsPerFilter, //Filters.Count > 0 ? Filters.Max(f => f.Plugins.Length) : 0, //SelectedFilter is object ? SelectedFilter.Plugins.Length : 0, //optional, let the table know in advance how many rows of data it needs to render, it will try to partition the table to fit it all on screen
				OnFillRow = (row, index) => {
					if (SelectedFilter == null)
						return false;
					
					var i = index + SelectedFilter.Skip;
					if (i >= SelectedFilter.Plugins.Length) // || index >= Table.MaxRowsPerColumn*Table.MaxColumnsPerPage)
						return false;
					
					if (Table.MaxColumnsPerPage > 1 && index >= Table.MaxRowsPerColumn*Table.MaxColumnsPerPage)
						return false;
					
					var addon = SelectedFilter.Plugins[i++];
					LabelStringDecorator statusUI = (LabelStringDecorator)row.Labels[0];
					LabelStringDecorator countUI = (LabelStringDecorator)row.Labels[1];
					LabelStringDecorator namespaceUI = (LabelStringDecorator)((ILabelDecoratorCollection)row.Labels[2]).Labels[0];
					LabelStringDecorator nameUI = (LabelStringDecorator)((ILabelDecoratorCollection)row.Labels[2]).Labels[1]; //row.Labels[1];
					
					//string nameSpace = addon.GetType().Namespace;
					//nameSpace = nameSpace.Remove(0, 14); //removeString.Length //nameSpace.IndexOf("Turbo.Plugins.");
					countUI.StaticText = i.ToString() + ".";
					
					Type type = addon.GetType();
					namespaceUI.StaticText = type.Namespace.Substring(14) + "."; //type.Namespace.Substring(13) +  //addon.GetType().Namespace.Remove(0, 14) + "."; //nameSpace + ".";
					nameUI.StaticText = type.Name; //addon.GetType().Name; //+ " (" + nameSpace + ")";
					
					bool enabled = addon.Enabled;
					if (enabled)
					{
						statusUI.StaticText = TextEnabled;
						statusUI.Font = EnabledFont;
						HintPlugin.Labels[2] = HintDeactivate; //statusUI.Hint = HintDeactivate;
						//nameUI.StaticText = addon.GetType().Name;
						countUI.Font = EnabledFont;
						nameUI.Font = EnabledFont;
						//nameUI.Hint = HintDeactivate;
						//row.Hint = HintDeactivate;
						
						//((LabelStringDecorator)((LabelRowDecorator)row.Labels[1]).Labels[2].Hint).StaticText = string.Join(System.Environment.NewLine, addon.PerformanceCounters.Keys); //row.Labels[1];
					}
					else
					{
						statusUI.StaticText = TextDisabled;
						statusUI.Font = DisabledFont;
						HintPlugin.Labels[2] = HintActivate; //statusUI.Hint = HintActivate;
						//nameUI.StaticText = addon.GetType().Name;
						countUI.Font = DisabledFont;
						nameUI.Font = DisabledFont;
						//nameUI.Hint = HintActivate;
						//row.Hint = HintActivate;
					}
					
					//readme
					((ILabelDecoratorCollection)row.Labels[2]).Labels[2].Enabled = addon is IPluginManifest || ManifestMap.ContainsKey(type.ToString());
					
					return true;
				}
			};
			
			if (Filters.Count > 0)
			{
				var plugins = Hud.AllPlugins.Where(p => !(p is IMenuAddon) && !Hidden.Any(p.GetType().ToString().StartsWith));
				foreach (var filter in Filters)
					filter.Plugins = plugins.Where(p => filter.Check(p)).OrderBy(p => p.GetType().ToString()).ToArray();
				
				//debug paging
				//Filters.Add(new PluginFilter() {Name = "Test", Plugins = Enumerable.Repeat(this, 600).ToArray()});
				
				SelectedFilter = Filters[0];
				MaxPluginsPerFilter = Filters.Max(f => f.Plugins.Length);
			}
			
			//string filepath = string.Format("{0}\\{1}.cs", !string.IsNullOrEmpty(SelectedFilter.ConfigFilePath) ? SelectedFilter.ConfigFilePath : "plugins\\User", SelectedFilter.ConfigFile);
			DeleteUIVisible = true;
			DeleteUI = new LabelStringDecorator(Hud, "❌") { //"ⓧ"
				Hint = plugin.DeleteConfigHint, /*new LabelStringDecorator(Hud, () => {
					var filepath = string.Format("{0}\\{1}.cs", !string.IsNullOrEmpty(SelectedFilter.ConfigFilePath) ? SelectedFilter.ConfigFilePath : "plugins\\User", SelectedFilter.ConfigFile);
					return filepath + " (" + System.IO.File.Exists(filepath) + ")";
				}) {Font = TextFont},*/ 
				Font = plugin.ResumeFont, //PinnedFont, //Hud.Render.CreateFont("tahoma", 13, 255, 255, 0, 0, false, false, true),
				Alignment = HorizontalAlign.Right, 
				SpacingLeft = 5, 
				SpacingRight = 5,
				SpacingTop = 2, //plugin.CreateDelete(filepath); //plugin.CreateDelete(ConfigFile); //CreateSave();
				OnBeforeRender = (label) => {
					int diff = Hud.Game.CurrentGameTick - FileCheckUpdateTick;
					if (diff < 0 || diff > 120)
					{
						FileCheckUpdateTick = Hud.Game.CurrentGameTick;
						DeleteUIVisible = System.IO.File.Exists(string.Format("{0}\\{1}.cs", !string.IsNullOrEmpty(SelectedFilter.ConfigFilePath) ? SelectedFilter.ConfigFilePath : "plugins\\User", SelectedFilter.ConfigFile));
						//label.Enabled = System.IO.File.Exists(string.Format("{0}\\{1}.cs", !string.IsNullOrEmpty(SelectedFilter.ConfigFilePath) ? SelectedFilter.ConfigFilePath : "plugins\\User", SelectedFilter.ConfigFile));
					}
					return DeleteUIVisible; //visible; //label.Enabled;
				},
				OnClick = (label) => {
					var file = string.Format("{0}\\{1}.cs", !string.IsNullOrEmpty(SelectedFilter.ConfigFilePath) ? SelectedFilter.ConfigFilePath : "plugins\\User", SelectedFilter.ConfigFile);
					System.IO.File.Delete(file);
					//label.Enabled = System.IO.File.Exists(file);
				}
			};

			SelectUI = new LabelRowDecorator(Hud) {BackgroundBrush = plugin.BgBrush, HoveredBrush = plugin.HighlightBrush};
			foreach (var filter in Filters)
			{
				if (filter.Plugins.Length == 0)
					continue;
				
				filter.Decorator = new LabelRowDecorator(Hud,
					new LabelStringDecorator(Hud, filter.Name) {Font = TextFont, SpacingBottom = 3},
					new LabelStringDecorator(Hud, (filter.Plugins.Length > 0 ? " ("+filter.Plugins.Length+")" : string.Empty)) {Font = plugin.TitleFont},
					new LabelStringDecorator(Hud, "◀") {OnClick = (label) => {if (filter != SelectedFilter) return; filter.Skip -= Table.MaxColumnsPerPage*Table.MaxRowsPerColumn; if (filter.Skip < 0) filter.Skip = 0;}, Font = DefaultFont, HoveredFont = TextFont, SpacingBottom = 3, SpacingLeft = 5, SpacingRight = 2}, //OnBeforeRender = (label) => {visible(label) && filter.Skip > 0}, 
					new LabelStringDecorator(Hud, "▶") {OnClick = (label) => {if (filter != SelectedFilter) return; filter.Skip += Table.MaxColumnsPerPage*Table.MaxRowsPerColumn; if (filter.Skip > filter.Plugins.Length) filter.Skip -= Table.MaxColumnsPerPage*Table.MaxRowsPerColumn;}, Font = DefaultFont, HoveredFont = TextFont, SpacingBottom = 3, SpacingLeft = 5, SpacingRight = 2}//, //OnBeforeRender = (label) => {visible(label) && filter.Skip > 0}, 
					//new LabelStringDecorator(Hud, "▶") {OnClick = (label) => {if (filter != SelectedFilter) return; filter.Skip += Table.MaxColumnsPerPage*Table.MaxRowsPerColumn; if (filter.Skip > filter.Plugins.Length) filter.Skip = filter.Plugins.Length;}, Font = DefaultFont, HoveredFont = TextFont, SpacingBottom = 3, SpacingLeft = 2, SpacingRight = 3} //OnBeforeRender = (label) => visible(label) && filter.Skip + Table.MaxColumnsPerPage*Table.MaxRowsPerColumn <= filter.Plugins.Length, 
				) {
					BorderBrush = SelectedFilter == filter ? SelectedBrush : null,
					SpacingLeft = 10, 
					SpacingRight = 10,
					OnClick = SelectFilter,
					OnBeforeRender = (label) => {
						LabelStringDecorator backArrow = (LabelStringDecorator)((ILabelDecoratorCollection)label).Labels[2];
						LabelStringDecorator forwardArrow = (LabelStringDecorator)((ILabelDecoratorCollection)label).Labels[3];
						
						if (filter == SelectedFilter && filter.Plugins.Length > Table.MaxColumnsPerPage * Table.MaxRowsPerColumn)
						{
							backArrow.Enabled = true;
							forwardArrow.Enabled = true;
						
							if (filter.Skip > 0)
							{
								backArrow.Font = DefaultFont;
								backArrow.HoveredFont = TextFont;
							}
							else
							{
								backArrow.Font = FadedFont;
								backArrow.HoveredFont = null;
							}
							
							if (filter.Skip + Table.MaxColumnsPerPage*Table.MaxRowsPerColumn < filter.Plugins.Length)
							{
								forwardArrow.Font = DefaultFont;
								forwardArrow.HoveredFont = TextFont;
							}
							else
							{
								forwardArrow.Font = FadedFont;
								forwardArrow.HoveredFont = null;
							}
							
							//Hud.TextLog.Log("_toggle", filter.Skip.ToString() + " + (" + Table.MaxColumnsPerPage + " * " + Table.MaxRowsPerColumn + ") < " + filter.Plugins.Length, false, true);
							
							/*if (filter == SelectedFilter)
							{
								backArrow.HoveredFont = TextFont;
								forwardArrow.HoveredFont = TextFont;
							}
							else
							{
								backArrow.HoveredFont = null;
								forwardArrow.HoveredFont = null;
							}*/
						}
						else
						{
							backArrow.Enabled = false;
							forwardArrow.Enabled = false;
						}

						return true;
					}
				};
				
				/*if (filter.Plugins.Length > Table.MaxColumnsPerPage * Table.MaxRowsPerColumn)
				{
					var visible = (label) => filter.Plugins.Length > Table.MaxColumnsPerPage * Table.MaxRowsPerColumn;
					filter.Decorator.Labels.Add(new LabelStringDecorator(Hud, "◀") {OnBeforeRender = visible, Font = TextFont, SpacingBottom = 3, SpacingLeft = 5, SpacingRight = 5});
					filter.Decorator.Labels.Add(new LabelStringDecorator(Hud, "▶") {OnBeforeRender = visible, Font = TextFont, SpacingBottom = 3, SpacingLeft = 5, SpacingRight = 5});
				}*/

				SelectUI.Labels.Add(filter.Decorator);
			}

			
			Panel = new LabelColumnDecorator(Hud,
				new LabelDelayedDecorator(Hud,
					new LabelAlignedDecorator(Hud, 
						new LabelStringDecorator(Hud, () => "TOGGLE PLUGINS") {Font = plugin.TitleFont, SpacingLeft = 15, SpacingRight = 15},
						DeleteUI, //plugin.CreateDelete(ConfigFile), //
						plugin.CreateSave(Save)//, //SaveUI,
						//plugin.CreatePin(this)
					)
				) {BackgroundBrush = plugin.BgBrush},
				/*new LabelAlignedDecorator(Hud,
					
					//DeleteUI,
					plugin.CreateSave(Save)//, //SaveUI,
				) {BackgroundBrush = plugin.BgBrush, SpacingBottom = 4},*/
				SelectUI,
				Table
			);
			
			//remove all manifests of plugins that are not installed
			Manifests.RemoveAll(m => m is PluginManifest && !Hud.AllPlugins.Any(p => p.GetType().ToString().StartsWith(((PluginManifest)m).Path, StringComparison.OrdinalIgnoreCase))); //Contains requires NET 6+ //StringComparison.CurrentCultureIgnoreCase
			
			//add manifests that are embedded in plugins
			Manifests.AddRange(Hud.AllPlugins.Where(p => p is IPluginManifest && !Manifests.Contains((IPluginManifest)p)).Cast<IPluginManifest>());
			
			//map dependencies
			foreach (IPluginManifest manifest in Manifests) //Hud.AllPlugins.Where(p => p is IPluginManifest).Cast<IPluginManifest>())
			{
				string name = manifest is PluginManifest && !string.IsNullOrEmpty(((PluginManifest)manifest).Path) ? ((PluginManifest)manifest).Path : manifest.GetType().ToString();
				ManifestMap[name] = manifest;
				
				if (manifest.Dependencies == null || manifest.Dependencies.Count == 0)
					continue;
				
				for (int i = 0; i < manifest.Dependencies.Count; ++i) //string dep in manifest.Dependences)
				{
					string dep = manifest.Dependencies[i];
					if (string.IsNullOrEmpty(dep))
						continue;
					
					//assume this exists because there would be a compiler error otherwise
					IPlugin plug = Hud.AllPlugins.FirstOrDefault(p => p.GetType().ToString().StartsWith(dep, StringComparison.OrdinalIgnoreCase)); //Contains requires NET 6+ //StringComparison.CurrentCultureIgnoreCase
					if (plug == null)
					{
						//Hud.Sound.Speak("dependency not found: " + dep);
						continue;
					}
					
					if (plug is IPluginManifest)
					{
						//add all of that plugin's dependencies too
						manifest.Dependencies.AddRange(((IPluginManifest)plug).Dependencies.Where(d => !manifest.Dependencies.Contains(d)));
						//if (!manifest.Dependencies.Contains(plugdep))
						//	manifest.Dependencies.Add(plugdep);
					}
					
					dep = plug.GetType().ToString();
					if (Dependents.ContainsKey(dep))
						Dependents[dep].Add(name);
					else
						Dependents[dep] = new List<string>() {name};
				}
				
				manifest.Dependencies.Sort();
			}
			
			//if toggle states is empty, then snapshot hasn't been run yet
			//if (ToggleStates.Count == 0)
			//	Snapshot();
			
			Plugin = plugin;
		}
		
		public void Customize()
		{
			Snapshot();
		}
		
		public void SelectFilter(ILabelDecorator label)
		{
			//string selected = ((LabelStringDecorator)((LabelRowDecorator)label).Labels[0]).StaticText;
			if (SelectedFilter.Decorator == label)
				return;

			SelectedFilter.Decorator.BorderBrush = null;
			
			SelectedFilter = Filters.FirstOrDefault(f => f.Decorator == label);
			if (SelectedFilter is object)
				SelectedFilter.Decorator.BorderBrush = SelectedBrush;
		
			//force DeleteUI update
			FileCheckUpdateTick = 0;
		}
		
		public void OnClick(ILabelDecorator label)
		{
			//SaveUI.Enabled = true;
			//DeleteUI.Enabled = File.Exists(ConfigFile);
			if (SelectedFilter == null)
				return;
			
			if (Table.HoveredRow > -1 && Table.HoveredRow + SelectedFilter.Skip < SelectedFilter.Plugins.Length) //Table.HoveredRow > -1 && Table.HoveredCol > -1) //&& 
			{
				//Hud.Sound.Speak("Row: " + Table.HoveredRow + ", Column: " + col + ", Index: " + index); //(int)Math.Floor((float)col / (float)PluginsPerColumn));
				IPlugin addon = SelectedFilter.Plugins[Table.HoveredRow + SelectedFilter.Skip];
				addon.Enabled = !addon.Enabled;
				
				//write config file
				//Hud.TextLog.Queue(this); //calls Log() at the appropriate time
				if (!string.IsNullOrEmpty(SelectedFilter.ConfigFile)) //&& !SaveFilters.Contains(SelectedFilter)) 
				{
					//SaveFilter = SelectedFilter;
					if (SaveFilters.Add(SelectedFilter)) //; //HashSet.Add does uniqueness checking
						Hud.TextLog.Queue(this, 3);
				}
			}
		}
		
		public void Save(ILabelDecorator label)
		{
			//SaveUI.Enabled = false;
			//if (SaveFilter == null)
			if (SelectedFilter == null)
				return;
			
			//Hud.Sound.Speak("save");
			//Plugin.Save();
			
			if (!string.IsNullOrEmpty(SelectedFilter.ConfigFile) && SaveFilters.Add(SelectedFilter)) //; //HashSet.Add does uniqueness checking
				Hud.TextLog.Queue(this, 3);

			//SaveFilters.Add(SelectedFilter);
			//Hud.TextLog.Queue(this, 3);
		}
		
		public void Snapshot()
		{
			//save the state of all plugins before the config is applied
			foreach (IPlugin plugin in Hud.AllPlugins)
				ToggleStates[plugin.GetType().ToString()] = plugin.Enabled;
		}
		
		public void Log(string path)
		{
			foreach (var filter in SaveFilters)
			{
				//Hud.Sound.Speak("log");
				TextBuilder.Clear();
				TextBuilder.Append("/*\n\tThis file was generated by Razor\\Menu\\MenuTogglePlugins.cs and contains custom plugin toggle states.\n*/\n\n");
				TextBuilder.Append("namespace Turbo.Plugins.User\n{\n");
				TextBuilder.Append("\tusing Turbo.Plugins.Default;\n");
				TextBuilder.Append("\tusing Turbo.Plugins.Razor.Util;\n\n");
				//TextBuilder.Append("\tusing Turbo.Plugins.Razor;\n\n");
				//TextBuilder.AppendFormat("\tpublic class {0} : BasePlugin, ICustomizer\n\t{\n", ConfigFileName); //can't use { or } in AppendFormat without escaping it
				TextBuilder.AppendFormat("\tpublic class {0} : BasePlugin, ICustomizer\n", filter.ConfigFile);
				TextBuilder.Append("\t{\n");
				TextBuilder.AppendFormat("\t\tpublic {0}() ", filter.ConfigFile);
				TextBuilder.Append("{ Enabled = true; Order = int.MaxValue; }\n\n"); //apply these defaults last
				//TextBuilder.Append("\t\tpublic override void Load(IController hud) { base.Load(hud); }\n\n");
				TextBuilder.Append("\t\tpublic void Customize()\n\t\t{\n");
				//TextBuilder.Append("\t\t\tHud.GetPlugin<Razor.Plugin.MenuTogglePlugins>().Snapshot();\n");
				
				foreach (IPlugin plugin in filter.Plugins) //Addons)
				{
					//string nameSpace = plugin.GetType().Namespace;
					//nameSpace = nameSpace.Remove(0, 14);
					//string path = typeof(plugin).ToString();
					
					//TextBuilder.AppendFormat("\t\t\tHud.TogglePlugin<{0}.{1}>({2});\n", nameSpace, plugin.GetType().Name, plugin.Enabled.ToString().ToLower()); // "\t\t\t\tplugin.ConfigureDock(\"{0}\", \"{1}\");\n", dock.Id, string.Join("\", \"", dock.Addons.Select(a => a.Id)));
					string key = plugin.GetType().ToString();
					if (!ToggleStates.ContainsKey(key) || ToggleStates[key] != plugin.Enabled)
						TextBuilder.AppendFormat("\t\t\tHud.TryTogglePlugin(\"{0}\", {1});\n", key, plugin.Enabled.ToString().ToLower()); //.Remove(0, 6) //plugin.GetType().ToString().Remove(0, 14), plugin.Enabled.ToString().ToLower()); // "\t\t\t\tplugin.ConfigureDock(\"{0}\", \"{1}\");\n", dock.Id, string.Join("\", \"", dock.Addons.Select(a => a.Id)));
				}
				
				TextBuilder.Append("\n\t\t}\n\t}\n}");

				//tier 2
				//Hud.TextLog.Log(ConfigFileName, TextBuilder.ToString(), false, false);
				
				//tier 3 only
				//File.WriteAllText(ConfigFile, TextBuilder.ToString());
				string filePath = filter.ConfigFile + ".cs";
				if (!string.IsNullOrEmpty(filter.ConfigFilePath))
					filePath = filter.ConfigFilePath + @"\" + filePath;
				else if (!string.IsNullOrEmpty(ConfigFilePath))
					filePath = ConfigFilePath + @"\" + filePath;
				
				if (!string.IsNullOrEmpty(path) && !filePath.StartsWith(path, StringComparison.OrdinalIgnoreCase))
					filePath = path + @"\" + filePath;
				
				File.WriteAllText(filePath, TextBuilder.ToString()); //string.Format("{0}\\{1}.cs", !string.IsNullOrEmpty(path) + !string.IsNullOrEmpty(filter.ConfigFilePath) ? filter.ConfigFilePath : ConfigFilePath/*"plugins\\User"*/, filter.ConfigFile), TextBuilder.ToString());
			}
			
			SaveFilters.Clear();
		}
	}
}