namespace Turbo.Plugins.Razor.RunStats
{
	using SharpDX.DirectWrite;
	using System;
	using System.Drawing;
	using System.Linq;
	using System.Collections.Generic;
	//using Newtonsoft.Json;
	//using System.Text.Json;
	//using System.Runtime.InteropServices;

	using Turbo.Plugins.Default;
	using Turbo.Plugins.Razor.Label;
	using Turbo.Plugins.Razor.Menu;
	using Turbo.Plugins.Razor.Movable;
	using Turbo.Plugins.Razor.Util; //Hud.Sno.GetExpToNextLevel

	public class MenuPools : BasePlugin, IMenuAddon, IAfterCollectHandler, INewAreaHandler, IMovable //IInGameTopPainter /*, ICustomizer, ILeftClickHandler, IRightClickHandler*/
	{
		public bool ShowOnPortraits { get; set; } = true;
		public bool HideDefaultPlugin { get; set; } = true;
		public string TextRift { get; set; } = "Рифт ";
		public string TextAct { get; set; } = "Акт ";
		
		public IFont TextFont { get; set; }
		public IFont PortraitFont { get; set; }
		public IFont FadedFont { get; set; }
		public IFont EnabledFont { get; set; }
		public IFont DisabledFont { get; set; }
		
		//public string TextEnabled { get; set; } = "✔️";
		//public string TextDisabled { get; set; } = "❌";
		
		public ILabelDecorator Label { get; set; }
		public ILabelDecorator LabelHint { get; set; }
		public float LabelSize { get; set; }
		public ILabelDecorator Panel { get; set; }

		public string Id { get; set; }
		public int Priority { get; set; } //the priority on the dock to show this addon (smaller to the left, higher to the right)
		public string DockId { get; set; }
		public string Config { get; set; }

		//private List<IMenuAddon> Addons;
		//private MenuPlugin Plugin;
		public class HeroInfo {
			public uint Id { get; set; }
			public string Name { get; set; }
			public HeroClass Class { get; set; }
			public bool Male { get; set; }
			public double Pools { get; set; }
			public int Season { get; set; } //Hud.Game.Me.Hero.Seasonal
			public bool Seasonal { get; set; }
			public bool Hardcore { get; set; }
			
			public HeroInfo(IPlayer player)
			{
				Id = player.HeroId;
				Name = player.Hero.Name;
				Season = player.Hero.Season;
				Seasonal = player.Hero.Seasonal;
				Hardcore = player.HeroIsHardcore; //player.Hero.Hardcore;
				Class = player.HeroClassDefinition.HeroClass;
				Male = player.Hero.IsMale;
			}
			
			public HeroInfo(string input)
			{
				//id : name : class : gender : pools : hardcore : season : seasonal
				string[] split = input.Split(':'); //, StringSplitOptions.RemoveEmptyEntries);
				
				if (uint.TryParse(split[0], out uint id))
					Id = id;

				Name = split[1];
				
				if (uint.TryParse(split[2], out uint cls))
					Class = (HeroClass)cls;
				
				Male = split[3] == "0" ? true : false;
				
				if (double.TryParse(split[4], out double pools))
					Pools = pools;
				
				Hardcore = split[5] == "1" ? true : false;
				
				if (int.TryParse(split[6], out int season))
					Season = season;
				
				Seasonal = split[7] == "1" ? true : false;
			}
			
			public override string ToString()
			{
				//id : name : class : gender : pools : hardcore : season : seasonal
				return string.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}:{7}", Id, Name, (uint)Class, Male ? 0 : 1, Pools, Hardcore ? 1 : 0, Season, Seasonal ? 1 : 0);
			}
		}
		public Dictionary<uint, double> BonusPools { get; set; } = new Dictionary<uint, double>();
		public Dictionary<uint, HeroInfo> Heroes { get; set; } = new Dictionary<uint, HeroInfo>();
		private List<uint> HeroIndex; //sorted by name, which is not unique
		private IPlayer[] PartyIndex; //sorted by name, which is not unique
		
		public class PoolInfo {
			public string Id { get; set; }
			public bool Used { get; set; }
			public uint WorldId { get; set; }
			public int Act { get; set; } //0 = in rift, -1 = story mode
			public int WorldSno { get; set; }
			public ISnoArea Area { get; set; }
			public uint SceneId { get; set; }
			public bool AreaFound { get; set; }
			//public uint Location { get; set; }
			//public uint AreaSno { get; set; }
			//public bool LocationUpdated { get; set; }
			public IWorldCoordinate Coordinate { get; set; }
			public /*LocationInfo.*/WaypointInfo Waypoint { get; set; }
			
			public PoolInfo(IMarker marker = null)
			{
				if (marker is object)
				{
					Id = marker.Id;
					Used = marker.IsUsed;
					WorldId = marker.WorldId;
					Coordinate = marker.FloorCoordinate;
				}
			}
			
			public override string ToString()
			{
				return string.Format("P [Act = {0}, WorldSno = {1}, Area = {2}, AreaFound = {3}, {4}]", Act, WorldSno, Area.NameLocalized, AreaFound, Waypoint);
			}
		}
		private Dictionary<string, PoolInfo> Pools = new Dictionary<string, PoolInfo>();
		private PoolInfo[] PoolIndex;
		private LocationInfo LocationPlugin;
		private bool IsRiftOpen;
		
		private LabelRowDecorator PortraitUI;
		private LabelTableDecorator TableMe;
		private LabelTableDecorator TableParty;
		private LabelTableDecorator TablePools;
		private ILabelDecorator TitleBarParty;
		private ILabelDecorator TitleBarPools;
		private ILabelDecorator FoundCountUI;
		private MenuPlugin Plugin;
		
		private uint[] HeroKeys;
		private uint[] BonusPoolKeys;
		
        public MenuPools()
        {
            Enabled = true;
			Priority = 30;
			DockId = "TopCenter";
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
			//Hud.Sound.Speak(RuntimeInformation.FrameworkDescription);
        }
		
		/*public void Customize()
		{
			if (HideDefaultPlugin)
				Hud.TogglePlugin<ExperienceOverBarPlugin>(false);
		}*/
		
		public void OnNewArea(bool newGame, ISnoArea area)
		{
			if (newGame)
			{
				//Hud.Sound.Speak("new game");
				//CurrentGameId = Hud.Game.GetId();
				Pools.Clear();
				IsRiftOpen = false;
			}
			/*else
			{
				var id = Hud.Game.GetId();
				if (id != CurrentGameId)
				{
					CurrentGameId = id;
					//Pools.Clear();
					Hud.Sound.Speak("game id mismatch");
					Hud.TextLog.Log("_game_id_mismatch", id + " vs " + CurrentGameId, true, true);
				}
			}*/
		}
		
		public void OnRegister(MenuPlugin plugin)
		{
			if (HideDefaultPlugin)
				Hud.TogglePlugin<ExperienceOverBarPlugin>(false);
			
			TextFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 235, 207, 52, false, false, true);
			PortraitFont = Hud.Render.CreateFont("tahoma", plugin.FontSize - 1f, 255, 235, 207, 52, false, false, 100, 0, 0, 0, true);
			FadedFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 150, 150, 150, false, false, true);
			EnabledFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 0, 255, 0, false, false, true);
			DisabledFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 255, 0, 0, false, false, true); //170, 150, 120
			
			//Addons = new List<IMenuAddon>(plugin.Addons.Values/*.Where(a => a != this)*/.OrderBy(a => a.Id));
			FoundCountUI = new LabelStringDecorator(Hud, () => {
				int count = Pools.Count(pair => !pair.Value.Used);
				//Pools.Count > 0 ? "(+" + Pools.Count +")" : string.Empty) {Font = plugin.TitleFont};
				if (count > 0)
					return "(+" + count +")";
				return null;
			}) {Font = plugin.TitleFont};
			
			//Label
			Label = new LabelRowDecorator(Hud,
				new LabelStringDecorator(Hud, () => Heroes.ContainsKey(Hud.Game.Me.HeroId) ? Heroes[Hud.Game.Me.HeroId].Pools.ToString("0.##") : "0") {FuncCacheDuration = 1, Font = TextFont, SpacingRight = 1},
				FoundCountUI,
				new LabelTextureDecorator(Hud, Hud.Texture.GetTexture(376779, 0)) {TextureHeight = 38, ContentHeight = plugin.MenuHeight, ContentWidth = 20}
			);
			
			PortraitUI = new LabelRowDecorator(Hud,
				new LabelStringDecorator(Hud) {Font = PortraitFont, SpacingLeft = 3, SpacingRight = 1},
				new LabelTextureDecorator(Hud, Hud.Texture.GetTexture(376779, 0)) {TextureHeight = 30, ContentHeight = plugin.MenuHeight-4, ContentWidth = 14}
			) {BackgroundBrush = plugin.BgBrush};
			//PortraitUI.Resize();
			
			if (!string.IsNullOrWhiteSpace(Config))
			{
				ShowOnPortraits = Config[0] == '1' ? true : false;
				
				if (Config.Length > 2)
				{
					string[] infos = Config.Substring(2).Split(';');
					foreach (string str in infos)
					{
						HeroInfo info = new HeroInfo(str);
						if (info.Id > 0 && !Heroes.ContainsKey(info.Id))
							Heroes.Add(info.Id, info);
					}
				}
			}
			
			TableMe = new LabelTableDecorator(Hud,
				new LabelRowDecorator(Hud,
					new LabelRowDecorator(Hud,
						new LabelTextureDecorator(Hud, Hud.Texture.GetTexture(640380019)) {TextureHeight = plugin.MenuHeight, ContentHeight = plugin.MenuHeight, ContentWidth = plugin.MenuHeight}, //hardcore
						new LabelTextureDecorator(Hud, Hud.Texture.GetTexture(364000500)) {TextureHeight = plugin.MenuHeight, ContentHeight = plugin.MenuHeight, ContentWidth = plugin.MenuHeight}, //seasonal
						new LabelTextureDecorator(Hud) {TextureHeight = plugin.MenuHeight - 4, ContentHeight = plugin.MenuHeight, ContentWidth = plugin.MenuHeight, SpacingTop = -1, SpacingBottom = 1} //character portrait
					) {Alignment = HorizontalAlign.Right, SpacingLeft = 10, SpacingRight = 8},
					new LabelStringDecorator(Hud) {Font = TextFont, Alignment = HorizontalAlign.Left, SpacingTop = 3, SpacingBottom = 3}, //name
					new LabelStringDecorator(Hud) {Font = TextFont, Alignment = HorizontalAlign.Left, SpacingLeft = 10, SpacingRight = 10, SpacingTop = 3, SpacingBottom = 3}, //pool count
					new LabelStringDecorator(Hud, "❌") {
						Hint = plugin.CreateHint("Delete Data"), 
						Font = DisabledFont, 
						OnClick = DeleteHero, 
						Alignment = HorizontalAlign.Left, 
						SpacingLeft = 10, 
						SpacingRight = 10, 
						SpacingTop = 3, 
						SpacingBottom = 3
					} //delete option
				)
			) {
				//SpacingLeft = 10, 
				//SpacingRight = 10,
				BackgroundBrush = plugin.BgBrush,
				HoveredBrush = plugin.HighlightBrush,
				OnBeforeRender = (label) => {HeroKeys = Heroes.Keys.ToArray(); return true;},
				OnFillRow = (row, i) => {
					if (i >= Heroes.Count) //HeroIndex.Count)
						return false;
					
					//uint id = HeroIndex[i];
					HeroInfo info = Heroes[HeroKeys[i]]; //.ElementAt(i);
					//KeyValuePair<uint, HeroInfo> info = Heroes.ElementAt(i);
					
					//if (Hud.Game.Me.Hero.Season != info.Season && Hud.Game.Me.Hero.Seasonal != info.Seasonal)
					//only show this data if it is of the same classification of character
					if (Hud.Game.Me.HeroIsHardcore != info.Hardcore || Hud.Game.Me.Hero.Seasonal != info.Seasonal)
					{
						row.Enabled = false;
						return true;
					}

					row.Enabled = true;
					
					if (HeroKeys[i] == Hud.Game.Me.HeroId)
					{
						row.BackgroundBrush = plugin.HighlightBrush;
						((LabelStringDecorator)row.Labels[3]).StaticText = " ";
						row.Labels[3].OnClick = null;
					}
					else
					{
						row.BackgroundBrush = null;
						((LabelStringDecorator)row.Labels[3]).StaticText = "❌";
						row.Labels[3].OnClick = DeleteHero;
					}
						
					((LabelRowDecorator)row.Labels[0]).Labels[0].Enabled = info.Hardcore;
					((LabelRowDecorator)row.Labels[0]).Labels[1].Enabled = info.Seasonal;
					((LabelTextureDecorator)((LabelRowDecorator)row.Labels[0]).Labels[2]).Texture = Hud.Texture.GetHeroHead(info.Class, info.Male);
					
					LabelStringDecorator label = (LabelStringDecorator)row.Labels[1];
					label.StaticText = info.Name;
					label.Font = Hud.Render.GetHeroFont(info.Class, plugin.FontSize, true, false, true);
					
					label = (LabelStringDecorator)row.Labels[2];
					label.StaticText = info.Pools.ToString("0.##"); //BonusPools[info.Key].ToString("0.##");
					//}
					
					return true;
				}
			};
			
			TableParty = new LabelTableDecorator(Hud,
				new LabelRowDecorator(Hud,
					new LabelStringDecorator(Hud) {Font = TextFont, SpacingTop = 3, SpacingBottom = 3, SpacingLeft = 10},
					new LabelStringDecorator(Hud) {Font = TextFont, SpacingTop = 3, SpacingBottom = 3, SpacingLeft = 10, SpacingRight = 10}
				)
			) {
				//SpacingLeft = 10, 
				//SpacingRight = 10,
				BackgroundBrush = plugin.BgBrush,
				HoveredBrush = plugin.HighlightBrush,
				/*OnBeforeRender = (label) => {
					if (Hud.Game.NumberOfPlayersInGame > 1)
						PartyIndex = Hud.Game.Players.Where(p => !p.IsMe).OrderBy(p => p.BattleTagAbovePortrait).ToArray();
					return true;
				},*/
				OnBeforeRender = (label) => {BonusPoolKeys = BonusPools.Keys.ToArray(); return true;},
				OnFillRow = (row, i) => {
					//if (PartyIndex == null || i >= PartyIndex.Length)
					//	return false;
					//IPlayer player = PartyIndex[i];
					if (i >= BonusPools.Count)
						return false;

					//double data = BonusPools[BonusPoolKeys[i]]; //.ElementAt(i);
					//KeyValuePair<uint, double> data = BonusPools.ElementAt(i);
					IPlayer player = Hud.Game.Players.FirstOrDefault(p => p.HeroId == BonusPoolKeys[i]);
					if (player is object)// && !player.IsMe)
					{
						row.Enabled = true;
						
						LabelStringDecorator label = (LabelStringDecorator)row.Labels[0];
						label.StaticText = player.BattleTagAbovePortrait;
						label.Font = Hud.Render.GetHeroFont(player.HeroClassDefinition.HeroClass, plugin.FontSize, true, false, true);
						
						label = (LabelStringDecorator)row.Labels[1];
						label.StaticText = BonusPools[player.HeroId].ToString("0.##");
					}
					else
					{
						row.Enabled = false;
						BonusPools.Remove(BonusPoolKeys[i]); //data.Key);
					}
					
					return true;
				}
			};
			
			TitleBarParty = new LabelStringDecorator(Hud, "PARTY") {Font = plugin.TitleFont, BackgroundBrush = plugin.BgBrushAlt, SpacingTop = 3, SpacingBottom = 5}; /*new LabelAlignedDecorator(Hud,
				new LabelStringDecorator(Hud, "PARTY") {Font = plugin.TitleFont, SpacingTop = 3, SpacingBottom = 3},
				new LabelStringDecorator(Hud, "👁") {
					Hint = plugin.CreateHint("Toggle portrait display"), 
					Font = EnabledFont, 
					Alignment = HorizontalAlign.Right, 
					SpacingTop = -2, 
					SpacingBottom = 2, 
				}
			) {BackgroundBrush = plugin.BgBrushAlt, SpacingTop = 5, SpacingBottom = 3};*/
			
			TitleBarPools = new LabelRowDecorator(Hud,
				new LabelStringDecorator(Hud, "НАЙДЕНО ЧАШ ОТРАЖЕНИЙ") {Font = plugin.TitleFont, SpacingTop = 5, SpacingBottom = 3},
				FoundCountUI
			) {BackgroundBrush = plugin.BgBrushAlt};
			
			TablePools = new LabelTableDecorator(Hud,
				new LabelRowDecorator(Hud,
					new LabelStringDecorator(Hud) {Font = TextFont, SpacingTop = 2, SpacingBottom = 2},
					new LabelStringDecorator(Hud) {Font = TextFont, Alignment = HorizontalAlign.Left, SpacingLeft = 10, SpacingTop = 2, SpacingBottom = 2}
				)
			) {
				SpacingBottom = 5,
				BackgroundBrush = plugin.BgBrush,
				SpacingLeft = 10, 
				SpacingRight = 10,
				HoveredBrush = plugin.HighlightBrush,
				OnBeforeRender = (label) => {
					PoolIndex = Pools.Count > 0 ? Pools.Values.OrderBy(p => p.Act).ToArray() : null;
					return true;
				},
				OnFillRow = (row, i) => {
					if (PoolIndex == null || i >= PoolIndex.Length)
						return false;

					PoolInfo info = PoolIndex[i]; //Pools.ElementAt(i);
					LabelStringDecorator label = (LabelStringDecorator)row.Labels[0];
					label.StrikeOut = info.Used;
					label.Font = info.Used ? FadedFont : TextFont;
					label.StaticText = info.Act == 0 ? TextRift : TextAct + Hud.ToRoman(info.Act);
					
					label = (LabelStringDecorator)row.Labels[1];
					label.StrikeOut = info.Used;
					label.Font = info.Used ? FadedFont : TextFont;

					if (info.Waypoint is object)
					{
						label.StaticText = LocationPlugin.GetDisplayedName(info.Waypoint.Area);
						var loc = LocationPlugin.GetDisplayedName(info.Area);
						if (loc != label.StaticText)
							label.StaticText = string.IsNullOrEmpty(label.StaticText) ? "?? - " + loc : label.StaticText + " - " + loc;
					}
					else
						label.StaticText = info.Area.NameLocalized;
					
					return true;
				}
			};
			
			//Menu
			Panel = new LabelColumnDecorator(Hud, 
				new LabelDelayedDecorator(Hud,
					new LabelAlignedDecorator(Hud, 
						new LabelStringDecorator(Hud, "БОНУСНЫЕ ЧАШИ ОПЫТА") {Font = plugin.TitleFont, SpacingLeft = 15, SpacingRight = 15},
						new LabelStringDecorator(Hud, "👁") {
							Hint = plugin.CreateHint("Toggle portrait display"), 
							Font = EnabledFont, 
							Alignment = HorizontalAlign.Right, 
							SpacingTop = -2, 
							SpacingBottom = 2,
							OnBeforeRender = (label) => { 
								((LabelStringDecorator)label).Font = ShowOnPortraits ? EnabledFont : DisabledFont; 
								return true;
							},
							OnClick = (label) => {
								ShowOnPortraits = !ShowOnPortraits;
								Config = GenerateConfig(); //Config = ShowOnPortraits ? "1" : "0";
								plugin.Save(); //save to file
							}
						},
						plugin.CreatePin(this)
					)
				) {BackgroundBrush = plugin.BgBrush},
				/*new LabelAlignedDecorator(Hud,
					//ViewLogButton,
					new LabelRowDecorator(Hud,
						new LabelStringDecorator(Hud, "Δ") {Font = TextFont, SpacingRight = 3},
						new LabelDeltaDecorator(Hud, () => LastSeenDamageReduction) {Font = TextFont, SpacingRight = 3}
					) {Hint = plugin.CreateHint("Change"), Alignment = HorizontalAlign.Left},
					new LabelStringDecorator(Hud, "DR:") {Hint = plugin.CreateHint("Current Damage Reduction"), Font = TextFont, SpacingLeft = 15, SpacingRight = 3},
					CurrentDR, //new LabelStringDecorator(Hud, () => (LastSeenDamageReduction * 100).ToHumanReadable(4) + "%") {Hint = plugin.CreateHint("Current Damage Per Second"), Font = LineCurDPS.Font, SpacingRight = 15},
					//pauseButton
					plugin.CreatePagingControls(Graph)
				) {BackgroundBrush = plugin.BgBrush, SpacingLeft = 10, SpacingRight = 10, SpacingTop = 2, SpacingBottom = 4}//,*/
				TableMe,
				
				TitleBarParty,
				TableParty,
				
				TitleBarPools,
				TablePools
			) {
				OnBeforeRender = (label) => {
					//hide or show pool count for party members
					//if (PartyIndex is object && PartyIndex.Length > 0)
					if (Hud.Game.NumberOfPlayersInGame > 1) //Heroes.Count < BonusPools.Count)
					{
						TitleBarParty.Enabled = true;
						TableParty.Enabled = true;
						
						//sync table widths
						/*if (TableMe is object && TableMe.RowWidths is object)
						{
							for (int i = 0; i < TableParty.RowWidths.Length; ++i)
							{
								float max = (float)Math.Max(TableMe.RowWidths[i], TableParty.RowWidths[i]);
								TableMe.RowWidths[i] = max;
								TableParty.RowWidths[i] = max;
							}
						}*/
					}
					else
					{
						TitleBarParty.Enabled = false;
						TableParty.Enabled = false;
					}
					
					//hide or show pools tracker
					if (Pools.Count > 0)
					{
						TitleBarPools.Enabled = true;
						TablePools.Enabled = true;
					}
					else
					{
						TitleBarPools.Enabled = false;
						TablePools.Enabled = false;
					}
					return true;
				}
			};
			//new LabelStringDecorator(Hud, "Test") {Font = TextFont, SpacingLeft = 15, SpacingRight = 15};
			
			Plugin = plugin;
			LocationPlugin = Hud.GetPlugin<LocationInfo>();
		}
		
		public void OnRegister(MovableController mover)
		{
			var rect = Hud.Game.Me.PortraitUiElement.Rectangle;
			((LabelStringDecorator)PortraitUI.Labels[0]).StaticText = "8.88"; //initialized for sizing purposes
			PortraitUI.Resize();
			
			mover.CreateArea(
				this,
				"PortraitAnchor", //area name
				new System.Drawing.RectangleF(5, rect.Y + rect.Height*0.2f, PortraitUI.Width, PortraitUI.Height), //player.PortraitUiElement.Rectangle.Bottom - player.PortraitUiElement.Rectangle.Height*0.21f); //player.PortraitUiElement.Rectangle.X - 10 // - ((LabelTextureDecorator)PortraitUI.Labels[1]).Width
				ShowOnPortraits, //enabled at start?
				true, //save to config file?
				ResizeMode.Off //resizable
			);
		}
		
		public void AfterCollect()
		{
			if (!Hud.Game.IsInGame)
				return;
			
			foreach (IPlayer player in Hud.Game.Players)
			{
				if (player.IsMe)
				{
					bool save = false;
					if (!Heroes.ContainsKey(player.HeroId))
					{
						save = true;
						Heroes.Add(player.HeroId, new HeroInfo(player));
					}
					
					decimal xpToNext = player.CurrentLevelNormal < player.CurrentLevelNormalCap ? (decimal)Hud.Sno.GetExpToNextLevel(player) : (decimal)player.ParagonExpToNextLevel;
					double pools = xpToNext > 0 ? (double)((decimal)player.BonusPoolRemaining / xpToNext) * 10 : 0;
					if (save || pools != Heroes[player.HeroId].Pools)
					{
						Heroes[player.HeroId].Pools = pools;
						Config = GenerateConfig(); //(ShowOnPortraits ? "1" : "0") + "!" + string.Join(";", Heroes.Values);// + JsonConvert.SerializeObject(Heroes);
						Plugin.Save();
					}
				}
				else if (player.HasValidActor && player.CoordinateKnown)
				{
					decimal xpToNext = player.CurrentLevelNormal < player.CurrentLevelNormalCap ? (decimal)Hud.Sno.GetExpToNextLevel(player) : (decimal)player.ParagonExpToNextLevel;
					double pools = xpToNext > 0 ? (double)((decimal)player.BonusPoolRemaining / xpToNext) * 10 : 0;
					BonusPools[player.HeroId] = pools;
				}

			}
			
			if (Hud.Game.IsInTown || Hud.Game.SpecialArea == SpecialArea.ChallengeRiftHub || Hud.Game.SpecialArea == SpecialArea.ChallengeRift)
				return;
			
			
			foreach (var marker in Hud.Game.Markers)
			{
				if (Hud.Game.Me.WorldId != marker.WorldId) //player's area data hasn't been updated yet
					break;
				
				if (marker.IsPoolOfReflection)
				{
					try {
						//uint sno = Hud.Game.Me.SnoArea is object ? Hud.Game.Me.SnoArea.Sno : 0;
						if (Pools.ContainsKey(marker.Id)) //update existing pool info
						{
							PoolInfo info = Pools[marker.Id];
							info.Used = marker.IsUsed;
							//Pools[marker.Id].Used = marker.IsUsed;
							
							if (LocationPlugin.IsInOpenWorld && info.Act != 0)
							{
								//this pool was assigned a waypoint in town, find another waypoint
								if (info.Waypoint is object && info.Waypoint.Area is object && info.Waypoint.Area.IsTown)
									info.Waypoint = GetNearestWaypoint(info);
							}
						}
						else //new pool found
						{
							PoolInfo info = new PoolInfo(marker) {
								Area = Hud.Game.Me.SnoArea, 
								SceneId = Hud.Game.Me.Scene.SceneId, 
								Act = LocationPlugin.Act, 
								WorldSno = LocationPlugin.WorldSno
							};
							
							
							// if (info.Area == null)
								// Hud.Sound.Speak("no area");
							// else
								// Hud.Sound.Speak(info.Area.NameLocalized);
							
							if (LocationPlugin.IsInOpenWorld && info.Act != 0)
								info.Waypoint = GetNearestWaypoint(info);
							
							Pools.Add(marker.Id, info);
						}
					} catch (Exception e) {
						Hud.TextLog.Log("_MenuPools", "exception occurred in area: " + Hud.Game.Me.SnoArea?.NameLocalized + " (Акт: " + LocationPlugin.Act + ")\n\n", true, true);
					}
				}
			}
			
			foreach (IActor actor in Hud.Game.Actors.Where(a => a.GizmoType == GizmoType.PoolOfReflection && a.Scene is object && a.WorldId == Hud.Game.Me.WorldId))
			{
				PoolInfo pool = Pools.Values.FirstOrDefault(p => p.Coordinate.Equals(actor.FloorCoordinate) && !p.AreaFound); 
				if (pool is object)
				{
					if (LocationPlugin.IsInOpenWorld && actor.Scene is object && pool.SceneId != actor.Scene.SceneId) //pool.Area != actor.Scene.SnoArea) //
					{
						// if (actor.Scene.SnoArea == null)
							// Hud.Sound.Speak("no area");
						// else
							// Hud.Sound.Speak(actor.Scene.SnoArea.NameLocalized);
						//Hud.TextLog.Log("_pooltest", actor.Scene.SnoArea.Sno + " vs " + Hud.Game.Me.SnoArea.Sno, true, true); //Code
						//Hud.TextLog.Log("_pooltest", actor.Scene.SnoScene.Code + " vs " + Hud.Game.Me.Scene.SnoScene.Code, true, true); //Code
						//Hud.TextLog.Log("_pooltest", actor.Scene.SceneId + " vs " + Hud.Game.Me.Scene.SceneId, true, true); //Code
						//Hud.TextLog.Log("_pooltest", actor.Scene.SnoScene.Hint.Hint + " vs " + Hud.Game.Me.Scene.SnoScene.Hint.Hint, true, true); //Code
						
						pool.Area = Hud.Game.Me.SnoArea; //actor.Scene.SnoArea;
						pool.SceneId = Hud.Game.Me.Scene.SceneId;
						pool.Act = LocationPlugin.Act; //LocationPlugin.GetAct(pool.Area); //
						pool.WorldSno = LocationPlugin.WorldSno; //LocationPlugin.GetWorldSno(pool.Area); //
						
						if (pool.Act != 0)
							pool.Waypoint = GetNearestWaypoint(pool);
					}
					else
						pool.AreaFound = true;
				}
			}
			
			bool wasRiftOpen = IsRiftOpen;
			IsRiftOpen = Hud.Game.Quests.Any(q => q.SnoQuest.Sno == 337492 && (q.QuestStepId == 1 || q.QuestStepId == 3 || q.QuestStepId == 10 || q.QuestStepId == 5));
			if (!IsRiftOpen && wasRiftOpen)
			{
				//remove any pools that were found in the rift
				foreach (var p in Pools.Where(pair => pair.Value.WorldSno == 0).ToArray())
					Pools.Remove(p.Key);
			}
		}
		
		//public void PaintTopInGame(ClipState clipState)
		public void PaintArea(MovableController mover, MovableArea area, float deltaX = 0, float deltaY = 0)
		{
			//if (clipState != ClipState.BeforeClip || !ShowOnPortraits || BonusPools == null) // | !BonusPools.ContainsKey(Hud.Game.Me.HeroId)
			if (BonusPools == null)
				return;
			
			double pools = Heroes.ContainsKey(Hud.Game.Me.HeroId) ? Heroes[Hud.Game.Me.HeroId].Pools : 0;
			if (pools > 0)
			{
				((LabelStringDecorator)PortraitUI.Labels[0]).StaticText = pools.ToString("0.##"); //(player.IsMe ? Heroes[player.HeroId].Pools : BonusPools[player.HeroId]).ToString("0.##");
				PortraitUI.Paint(area.Rectangle.X + deltaX, area.Rectangle.Y + deltaY); //player.PortraitUiElement.Rectangle.Bottom - player.PortraitUiElement.Rectangle.Height*0.21f); //player.PortraitUiElement.Rectangle.X - 10 // - ((LabelTextureDecorator)PortraitUI.Labels[1]).Width
			}

			if (Hud.Game.NumberOfPlayersInGame > 1)
			{
				var dY = area.Rectangle.Y - Hud.Game.Me.PortraitUiElement.Rectangle.Y;
				
				foreach (IPlayer player in Hud.Game.Players.Where(p => !p.IsMe))
				{
					pools = BonusPools.ContainsKey(player.HeroId) ? BonusPools[player.HeroId] : 0;
					if (pools > 0) //BonusPools.ContainsKey(player.HeroId))
					{
						
						((LabelStringDecorator)PortraitUI.Labels[0]).StaticText = pools.ToString("0.##"); //(player.IsMe ? Heroes[player.HeroId].Pools : BonusPools[player.HeroId]).ToString("0.##");
						PortraitUI.Paint(area.Rectangle.X + deltaX, player.PortraitUiElement.Rectangle.Y + deltaY + dY); //player.PortraitUiElement.Rectangle.Bottom - player.PortraitUiElement.Rectangle.Height*0.21f); //player.PortraitUiElement.Rectangle.X - 10 // - ((LabelTextureDecorator)PortraitUI.Labels[1]).Width
					}
				}
			}
		}
		
		/*private void InsertInOrder(uint heroId)
		{
			for (int i = 0; i < HeroIndex.Count; ++i)
			{
				HeroInfo alreadyInList = Heroes[HeroIndex[i]];
				int comparison = Heroes[heroId].Name.CompareTo(alreadyInList.Name);
				if (comparison == 0)
				{
					if (heroId < alreadyInList.Id)
						HeroIndex.Insert(i, heroId);
					else if (i == HeroIndex.Count-1)
						HeroIndex.Add(heroId);
					else
						HeroIndex.Insert(i+1, heroId);
					break;
				}
				else if (comparison < 0)
				{
					HeroIndex.Insert(i, heroId);
					break;
				}
			}
		}*/
		
		private WaypointInfo GetNearestWaypoint(PoolInfo pool)
		{
			if (LocationPlugin == null || LocationPlugin.Waypoints == null || pool.Area == null) //sanity check
				return null;
			
			if (LocationPlugin.Waypoints.ContainsKey(pool.Area.Sno))
			{
				WaypointInfo favoredWP = LocationPlugin.Waypoints[pool.Area.Sno];
				var distance = favoredWP.Coordinate.XYDistanceTo(pool.Coordinate);
				
				if (pool.WorldSno != 0)
				{
					var waypointsInWorld = LocationPlugin.Waypoints.Values.Where(w => w.Area is object && w != favoredWP && w.WorldSno == pool.WorldSno);
					if (waypointsInWorld.Any())
					{
						var waypoint = waypointsInWorld.Aggregate((left, right) => left.Coordinate.XYDistanceTo(pool.Coordinate) < right.Coordinate.XYDistanceTo(pool.Coordinate) ? left : right); //waypointsInWorld.OrderBy(w => w.Coordinate.XYDistanceTo(pool.Coordinate)).First();
						var altDistance = waypoint.Coordinate.XYDistanceTo(pool.Coordinate);
						
						//if (distance < altDistance)
						//	return favoredWP;
						
						if (distance > altDistance * 1.4f) //alternative route from the favored wp must be much more efficient to supersede it
							return waypoint;
						
						//return favoredWP;
						//return waypointsInWorld.Aggregate((left, right) => left.Coordinate.XYDistanceTo(coord) < right.Coordinate.XYDistanceTo(Hud.Game.Me.FloorCoordinate) ? left : right);
					}
				}
				
				return favoredWP;
			}
			else if (LocationPlugin.NearestWaypoint == null)
			{
				//just find the closest wp in the shared world
				var waypointsInWorld = LocationPlugin.Waypoints.Values.Where(w => w.Area is object && w.WorldSno == pool.WorldSno);
				if (waypointsInWorld.Any())
					return waypointsInWorld.Aggregate((left, right) => left.Coordinate.XYDistanceTo(pool.Coordinate) < right.Coordinate.XYDistanceTo(pool.Coordinate) ? left : right);
			}
			else if (LocationPlugin.NearestWaypoint.Act == LocationPlugin.Act)
				return LocationPlugin.NearestWaypoint;

			return null;
		}
		
		private string GenerateConfig()
		{
			return (ShowOnPortraits ? "1" : "0") + " " + string.Join(";", Heroes.Values);
		}
		
		private void DeleteHero(ILabelDecorator label)
		{
			if (TableMe.HoveredRow > -1 && TableMe.HoveredRow < Heroes.Count)
			{
				KeyValuePair<uint, HeroInfo> info = Heroes.ElementAt(TableMe.HoveredRow);
				Heroes.Remove(info.Key);
				Config = GenerateConfig();
				Plugin.Save();
			}
		}
	}
}