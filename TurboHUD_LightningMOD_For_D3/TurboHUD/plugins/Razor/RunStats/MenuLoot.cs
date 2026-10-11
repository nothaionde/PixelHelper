/*

Keeps count of loot obtained via drops, crafting, gambling

*/

namespace Turbo.Plugins.Razor.RunStats
{
	using SharpDX.DirectWrite;
	using System;
	using System.Drawing;
	using System.Linq;
	using System.Collections.Generic;

	using Turbo.Plugins.Default;
	using Turbo.Plugins.Razor.Label;
	using Turbo.Plugins.Razor.Menu;
	using Turbo.Plugins.Razor.Seasonal;

	public class MenuLoot : BasePlugin, IMenuAddon, IAfterCollectHandler, ILootGeneratedHandler, IItemPickedHandler //IInGameTopPainter /*, ILeftClickHandler, IRightClickHandler*/
	{
		public int HistoryShown { get; set; } = 15;
		public int HistoryShownTimeout { get; set; } = 30;
		
		public int HighlightDuration { get; set; } = 20; //seconds
		public IBrush HighlightBrush { get; set; }
		
		public bool ShowCountLegendary { get; set; } = true;
		public bool ShowCountAncient { get; set; } = true;
		public bool ShowCountPrimal { get; set; } = true;
		//public bool ShowCountGambled { get; set; } = true;
		//public bool ShowCountCrafted { get; set; } = true;
		
		public bool ShowLegendaryHistory { get; set; } = true;
		public bool ShowAncientHistory { get; set; } = true;
		public bool ShowPrimalHistory { get; set; } = true;
		
		public bool SpeakAncientCrafted { get; set; } = true;
		public bool SpeakPrimalCrafted { get; set; } = true;
		
		public string TextDropped { get; set; } = "Упало";
		public string TextGambled { get; set; } = "Кадала";
		public string TextCrafted { get; set; } = "Создано";
		public string TextPickedUp { get; set; } = "Подобрал";
		
		public long CountLegendaryDropped { get; set; } = 0;
		public long CountLegendaryGambled { get; set; } = 0;
		public long CountLegendaryCrafted { get; set; } = 0;
		public long CountAncientDropped { get; set; } = 0;
		public long CountAncientGambled { get; set; } = 0;
		public long CountAncientCrafted { get; set; } = 0;
		public long CountPrimalDropped { get; set; } = 0;
		public long CountPrimalGambled { get; set; } = 0;
		public long CountPrimalCrafted { get; set; } = 0;
		public long CountNormalDropped { get; set; } = 0;
		public long CountMagicDropped { get; set; } = 0;
		public long CountRareDropped { get; set; } = 0;
		public long CountEtherealDropped { get; set; } = 0;
		
		public IFont TimeFont { get; set; }
		public IFont NormalFont { get; set; }
		public IFont MagicFont { get; set; }
		public IFont RareFont { get; set; }
		public IFont LegendaryFont { get; set; }
		public IFont LegendarySetFont { get; set; }
		public IFont LegendaryCountFont { get; set; }
		public IFont AncientFont { get; set; }
		public IFont AncientSetFont { get; set; }
		public IFont AncientCountFont { get; set; }
		public IFont PrimalFont { get; set; }
		public IFont PrimalSetFont { get; set; }
		public IFont PrimalCountFont { get; set; }
		public IFont EtherealFont { get; set; }
		public IFont EtherealCountFont { get; set; }
		public IFont GambledFont { get; set; }
		public IFont CraftedFont { get; set; }
		public IFont DroppedFont { get; set; }
		public IFont PickedUpFont { get; set; }
		public IFont DisabledFont { get; set; }
		public IBrush PrimalBg { get; set; }
		
		public string Id { get; set; }
		public int Priority { get; set; } //the priority on the dock to show this addon (smaller to the left, higher to the right)
		public string DockId { get; set; }
		public string Config { get; set; }

		public ILabelDecorator Label { get; set; }
		public ILabelDecorator LabelHint { get; set; }
		public float LabelSize { get; set; }
		public ILabelDecorator Panel { get; set; }
		
		private int LastUpdateTick = 0;
		private long LastMaterialsChangeTick = 0;
		private long CountReusableParts = 0;
		private long CountArcaneDust = 0;
		private long CountVeiledCrystal = 0;
		private long CountDeathsBreath = 0;
		private long CountForgottenSoul = 0;
		private long CountAct1Mat = 0;
		private long CountAct2Mat = 0;
		private long CountAct3Mat = 0;
		private long CountAct4Mat = 0;
		private long CountAct5Mat = 0;
		private long CountUber1Mat = 0;
		private long CountUber2Mat = 0;
		private long CountUber3Mat = 0;
		private long CountUber4Mat = 0;
		private List<int> InventorySeen;
		private List<ItemInfo> History = new List<ItemInfo>();
		private List<ItemInfo> HistoryOutsideRift = new List<ItemInfo>();
		private List<ItemInfo> HistoryLegendary = new List<ItemInfo>();
		private List<ItemInfo> HistoryAncient = new List<ItemInfo>();
		private List<ItemInfo> HistoryPrimal = new List<ItemInfo>();
		private LabelTableDecorator TableUI;
		private LabelRowDecorator SummaryUI;
		private LabelStringDecorator HintDropped;
		private LabelStringDecorator HintPickedUp;
		private LabelStringDecorator HintCrafted;
		private LabelStringDecorator HintGambled;
		private LabelStringDecorator HintType;
		private LabelStringDecorator HintAreaName;
		private List<ItemInfo> View;
		private DateTime StartTime;
		
		private long ComputedNormalCount;
		private long ComputedMagicCount;
		private long ComputedRareCount;
		private long ComputedLegendaryCount;
		private long ComputedAncientCount;
		private long ComputedPrimalCount;
		private double ElapsedHours;
		
		public class ItemInfo {
			public int Seed { get; set; }
			public DateTime Timestamp { get; set; }
			public string Name { get; set; }
			public string Type { get; set; }
			
			public ITexture Texture { get; set; }
			public List<IItemStat> Stats { get; set; }
			
			public bool IsGambled { get; set; }
			public bool IsCrafted { get; set; }
			public bool IsInRift { get; set; }
			
			public bool IsAncient { get; set; }
			public bool IsPrimal { get; set; }
			public bool IsSet { get; set; }
			public bool IsEthereal { get; set; }
			
			public IWorldCoordinate FloorCoordinate { get; set; } //null = picked up
			public string AreaName { get; set; }
			public uint AreaSno { get; set; } = 0;
			
			public ItemInfo(IItem item, DateTime time, bool isGambled = false, bool isCrafted = false, bool isInRift = false, string areaName = null)
			{
				//Item = item;
				Seed = item.Seed;
				Name = item.SnoItem.NameLocalized; //item.FullNameLocalized ?? 
				Type = item.SnoItem.SnoItemType.NameLocalized;
				
				//Stats = item.StatList.ToList(); //make a copy
				
				if (!isGambled && !isCrafted && item.Location == ItemLocation.Floor)
				{
					FloorCoordinate = item.FloorCoordinate.Offset(0, 0, 0); //make a copy
					if (item.Scene.SnoArea is object)
						AreaSno = item.Scene.SnoArea.Sno;
				}

				AreaName = areaName;
				
				Timestamp = time;
				IsGambled = isGambled;
				IsCrafted = isCrafted;
				IsInRift = isInRift;
				
				IsAncient = item.AncientRank == 1;
				IsPrimal = item.AncientRank > 1;
				IsSet = item.SetSno != uint.MaxValue;
				IsEthereal = item.IsEthereal();
			}
		}
		
        public MenuLoot()
        {
            Enabled = true;
			Priority = 20;
			DockId = "BottomRight";
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
			
			//Delay = Hud.Time.CreateWatch();
        }
		
		public void AfterCollect()
		{
			if (!Hud.Game.IsInGame)
				return;

			if (LastUpdateTick == 0)
			{
				CountEtherealDropped = 0;
				CountLegendaryDropped = Hud.Tracker.Session.DropLegendary;
				CountLegendaryGambled = 0;
				CountLegendaryCrafted = 0;
				CountAncientDropped = Hud.Tracker.Session.DropAncient;
				CountAncientGambled = 0;
				CountAncientCrafted = 0;
				CountPrimalDropped = Hud.Tracker.Session.DropPrimalAncient;
				CountPrimalGambled = 0;
				CountPrimalCrafted = 0;
				CountNormalDropped = Hud.Tracker.Session.DropWhite;
				CountMagicDropped = Hud.Tracker.Session.DropMagic;
				CountRareDropped = Hud.Tracker.Session.DropRare;
				SaveMaterialsCount();
				SaveInventorySeen();
				LastUpdateTick = Hud.Game.CurrentGameTick;
				StartTime = Hud.Time.Now;
			}
			else if (Hud.Game.IsInTown)
			{
				int diff = Hud.Game.CurrentGameTick - LastUpdateTick;
				if (diff < 0 || diff > 20)
				{
					LastUpdateTick = Hud.Game.CurrentGameTick;
				
					//check if vendor windows are open
					//bool AtVendor = Hud.Render.GetUiElement("Root.NormalLayer.vendor_dialog_mainPage.panel").Visible; //artisan, gem upgrade, and kanai cube panels
					//bool AtKadala = Hud.Render.GetUiElement("Root.NormalLayer.shop_dialog_mainPage.panel")?.Visible; //merchant panels
					//bool AtCube = Hud.Render.GetUiElement("Root.NormalLayer.Kanais_Recipes_main.LayoutRoot")?.Visible;

					//check if crafting material count has changed while a crafting window is open
					if (Hud.Render.GetUiElement("Root.NormalLayer.vendor_dialog_mainPage.panel").Visible && //|| Hud.Render.GetUiElement("Root.NormalLayer.Kanais_Recipes_main.LayoutRoot").Visible) &&
						(CountReusableParts > Hud.Game.Me.Materials.ReusableParts ||
						CountArcaneDust > Hud.Game.Me.Materials.ArcaneDust ||
						CountVeiledCrystal > Hud.Game.Me.Materials.VeiledCrystal ||
						CountDeathsBreath > Hud.Game.Me.Materials.DeathsBreath ||
						CountForgottenSoul > Hud.Game.Me.Materials.ForgottenSoul ||
						CountAct1Mat > Hud.Game.Me.Materials.KhanduranRune ||
						CountAct2Mat > Hud.Game.Me.Materials.CaldeumNightShade ||
						CountAct3Mat > Hud.Game.Me.Materials.ArreatWarTapestry ||
						CountAct4Mat > Hud.Game.Me.Materials.CorruptedAngelFlesh ||
						CountAct5Mat > Hud.Game.Me.Materials.WestmarchHolyWater ||
						CountUber1Mat > Hud.Game.Me.Materials.LeoricsRegret ||
						CountUber2Mat > Hud.Game.Me.Materials.VialOfPutridness ||
						CountUber3Mat > Hud.Game.Me.Materials.IdolOfTerror ||
						CountUber4Mat > Hud.Game.Me.Materials.HeartOfFright))
					{
						//Hud.Sound.Speak("crafted");
						
						//inventory item scan
						if (InventorySeen is object)
						{
							foreach (var item in Hud.Inventory.ItemsInInventory.Where(i => !InventorySeen.Contains(i.Seed)))
							{
								if (item.Quality == ItemQuality.Legendary)
								{
									if (item.AncientRank > 0)
									{
										if (item.AncientRank == 1)
										{
											++CountAncientCrafted;
											
											if (SpeakAncientCrafted)
												Hud.Sound.Speak(item.SnoItem.NameLocalized);
										}
										else
										{
											++CountPrimalCrafted;
											
											if (SpeakPrimalCrafted)
												Hud.Sound.Speak(item.SnoItem.NameLocalized);
										}
									}
									else
										++CountLegendaryCrafted;
										
									AddToHistory(item, false, true); //gambled, crafted
								}
							}
						}
						
						//UpdateHints();
					}

					//bookkeeping
					SaveMaterialsCount();
					SaveInventorySeen();
				}
			}
			
			/*int diff = Hud.Game.CurrentGameTick - LastUpdateTick;
			if (diff < 0 || diff > 15)
			{
				
				LastUpdateTick = Hud.Game.CurrentGameTick;
				
			}*/
		}
		
		public void OnLootGenerated(IItem item, bool gambled)
        {
			if (item.Quality == ItemQuality.Legendary)
			{
				if (gambled)
				{
					if (item.AncientRank > 0)
					{
						if (item.AncientRank == 1)
							++CountAncientGambled;
						else
							++CountPrimalGambled;
					}
					else
						++CountLegendaryGambled;
				}
				else if (item.IsEthereal())
				{
					++CountEtherealDropped;
				}
					
				AddToHistory(item, gambled, false); //gambled, crafted
			}
		}
		
		public void OnItemPicked(IItem item)
		{
			List<ItemInfo> rankHistory = null;
			if (item.AncientRank > 0)
			{
				if (item.AncientRank == 1)
				{
					if (!ShowAncientHistory)
						return;
						
					rankHistory = HistoryAncient;
				}
				else 
				{
					if (!ShowPrimalHistory)
						return;
				
					rankHistory = HistoryPrimal;
				}
			}
			else
			{
				if (!ShowLegendaryHistory)
					return;
					
				rankHistory = HistoryLegendary;
			}
			
			if (rankHistory is object)
			{
				var info = rankHistory.FirstOrDefault(i => i.Seed == item.Seed);
				if (info is object)
				{
					if (info.FloorCoordinate is object)
						info.FloorCoordinate = null;
				}
			}
		}

		public void OnRegister(MenuPlugin plugin)
		{
			View = History;
			
			TimeFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 190, 255, 255, 255, false, false, 180, 0, 0, 0, true);
			NormalFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 255, 255, 255, false, false, 180, 0, 0, 0, true);;
			MagicFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 105, 105, 255, false, false, 180, 0, 0, 0, true);
			RareFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 232, 232, 91, false, false, 180, 0, 0, 0, true);
			LegendaryFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 235, 120, 0, true, false, 180, 0, 0, 0, true); //191, 100, 47
			LegendarySetFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 0, 255, 0, false, false, 180, 0, 0, 0, true);
			LegendaryCountFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 191, 100, 47, false, false, 180, 0, 0, 0, true);
			AncientFont = Hud.Render.CreateFont("tahoma", plugin.FontSize - 1f, 255, 0, 0, 0, true, false, 220, 227, 153, 25, true);
			AncientSetFont = Hud.Render.CreateFont("tahoma", plugin.FontSize - 1f, 255, 0, 0, 0, true, false, 180, 0, 255, 0, true); // - 1f //, 220, 227, 153, 25
			AncientCountFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 227, 153, 25, false, false, 180, 0, 0, 0, true);
            PrimalFont = Hud.Render.CreateFont("tahoma", plugin.FontSize - 1f, 255, 0, 0, 0, true, false, 180, 255, 64, 64, true);
            PrimalSetFont = Hud.Render.CreateFont("tahoma", plugin.FontSize - 1f, 255, 0, 255, 0, true, false, 180, 255, 64, 64, true);
            PrimalCountFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 255, 50, 50, false, false, 180, 0, 0, 0, true);
			EtherealFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 66, 245, 230, true, false, 180, 0, 0, 0, true);
			EtherealCountFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 66, 245, 230, false, false, 180, 0, 0, 0, true);
            //GambledFont = Hud.Render.CreateFont("tahoma", plugin.FontSize - 1f, 255, 0, 0, 0, true, false, 180, 255, 64, 64, true);
            CraftedFont = Hud.Render.CreateFont("tahoma", plugin.FontSize - 1f, 255, 255, 255, 255, true, false, 180, 0, 0, 0, true);
            DroppedFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 255, 0, 0, true, false, 180, 0, 0, 0, true);
            PickedUpFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 0, 255, 0, true, false, 180, 0, 0, 0, true);
            DisabledFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 150, 150, 150, false, false, true); //255, 0, 0
			
			PrimalBg = Hud.Render.CreateBrush(125, 94, 5, 5, 0);
			
			//Label
			Label = new LabelRowDecorator(Hud,
				new LabelStringDecorator(Hud, () => ((Hud.Tracker.Session.DropLegendary - CountLegendaryDropped) + CountLegendaryGambled + CountLegendaryCrafted).ToString()) {Font = LegendaryCountFont},
				new LabelTextureDecorator(Hud, Hud.Texture.GetTexture(63509052)) {TextureHeight = 35, ContentHeight = plugin.MenuHeight - 2, SpacingLeft = 2}
				//new MenuIconDecorator(Hud.Texture.GetTexture(1272453464)) { IconHeight = plugin.MenuHeight + 8, Height = plugin.MenuHeight }
				//new MenuIconDecorator(Hud.Texture.GetTexture(701882332)) {Hint = TextGambled, IconWidth = plugin.MenuHeight - 2, IconHeight = plugin.MenuHeight, Height = plugin.MenuHeight - 2, Alignment = HorizontalAlign.Center}
				
				/*new MenuStringDecorator("L") {Hint = "Legendaries Generated", TextFont = LegendaryFont, SpacingLeft = 2, SpacingRight = 5},
				new MenuStringDecorator(() => ((Hud.Tracker.Session.DropAncient - CountAncientDropped) + CountAncientGambled + CountAncientCrafted).ToString()) {Enabled = false, TextFont = AncientCountFont, SpacingLeft = 5, SpacingRight = 0},
				new MenuStringDecorator("A") {Hint = "Ancient Legendaries Generated", TextFont = AncientFont, SpacingLeft = 2, SpacingRight = 5},
				new MenuStringDecorator(() => ((Hud.Tracker.Session.DropPrimalAncient - CountPrimalDropped) + CountPrimalGambled + CountPrimalCrafted).ToString()) {Enabled = false, TextFont = PrimalCountFont, SpacingLeft = 5, SpacingRight = 0},
				new MenuStringDecorator("P") {Hint = "Primal Ancient Legendaries Generated", TextFont = PrimalFont, SpacingLeft = 2, SpacingRight = 5}*/
			);
			
			TableUI = new LabelTableDecorator(Hud, 
				new LabelRowDecorator(Hud,
					new LabelStringDecorator(Hud) {Font = TimeFont, Alignment = HorizontalAlign.Right, SpacingLeft = 5, SpacingRight = 5}, //time
					new LabelStringDecorator(Hud) {Font = LegendaryFont, Alignment = HorizontalAlign.Center, SpacingLeft = 5, SpacingRight = 5}, //rank
					//new LabelTextureDecorator(Hud) {Height = plugin.MenuHeight - 2}, //name
					new LabelStringDecorator(Hud) {Hint = plugin.CreateHint(string.Empty), Font = LegendaryFont, Alignment = HorizontalAlign.Left, SpacingLeft = 5, SpacingRight = 5}, //name
					new LabelRowDecorator(Hud, //type
						new LabelStringDecorator(Hud) {Font = DroppedFont, SpacingLeft = 5, SpacingRight = 5},
						new LabelTextureDecorator(Hud, Hud.Texture.GetTexture(701882332)) {Hint = plugin.CreateHint(TextGambled), TextureWidth = plugin.MenuHeight - 2, TextureHeight = plugin.MenuHeight - 2, ContentHeight = plugin.MenuHeight - 2},
						new LabelTextureDecorator(Hud, Hud.Texture.GetTexture(1272453464)) {Hint = plugin.CreateHint(TextCrafted), TextureHeight = plugin.MenuHeight + 8, ContentHeight = plugin.MenuHeight - 2 },
						new LabelStringDecorator(Hud) {Hint = plugin.CreateHint("Локация"), Font = DroppedFont, SpacingLeft = 5, SpacingRight = 5}
					) {Alignment = HorizontalAlign.Left}
				) {SpacingTop = 2, SpacingBottom = 2}
			)
			{ 
				BackgroundBrush = plugin.BgBrush,
				//HighlightBrush = plugin.HighlightBrush,
				//SpacingLeft = 10, 
				//SpacingRight = 10,
				SpacingBottom = 5,
				FillWidth = false, //true,
				OnFillRow = (row, index) => {
					if (index >= View.Count)
						return false;
							
					ItemInfo info = View[View.Count - 1 - index];
					TimeSpan elapsed = Hud.Time.Now - info.Timestamp;
					if (Hud.Game.IsInTown)
					{
						if (index >= HistoryShown)
							return false;
					}
					else if (elapsed.TotalSeconds > HistoryShownTimeout)
					{
						row.Enabled = false;
						return true;
					}
					
					row.Enabled = true;
					row.BackgroundBrush = (HighlightDuration > 0 && elapsed.TotalSeconds < HighlightDuration ? HighlightBrush : null);
					
					//timestamp
					LabelStringDecorator timeUI = (LabelStringDecorator)row.Labels[0];
					string time;
					if (elapsed.TotalSeconds < 60)
						time = elapsed.TotalSeconds.ToString("F0") + "с назад";
					else if (elapsed.TotalMinutes < 10)
						time = elapsed.TotalMinutes.ToString("F0") + "м назад";
					else
						time = info.Timestamp.ToString("hh:mm tt");
					timeUI.StaticText = time;
					
					//rank
					LabelStringDecorator rankUI = (LabelStringDecorator)row.Labels[1];
					IFont font = null; //rankUI.TextFont;
					string rank = null;
					if (info.IsEthereal)
					{
						rank = "Э";
						font = EtherealFont;
					}
					else if (info.IsPrimal)
					{
						rank = "П";
						font = PrimalFont;
					}
					else if (info.IsAncient)
					{
						rank = "Д";
						font = AncientFont;
					}
					else
					{
						rank = " ";
						font = LegendaryCountFont;
					}
					rankUI.Font = font;
					rankUI.StaticText = rank;
					
					//item icon
					//MenuIconDecorator iconUI = (MenuIconDecorator)view.Decorators[2];
					//iconUI.IconTexture = info.Texture;
					
					//item name
					LabelStringDecorator nameUI = (LabelStringDecorator)row.Labels[2]; //3
					nameUI.BackgroundBrush = null;
					if (info.IsSet)
					{
						if (info.IsPrimal)
						{
							font = PrimalSetFont;
							nameUI.BackgroundBrush = PrimalBg;
						}
						else if (info.IsAncient)
							font = AncientSetFont;
						else
							font = LegendarySetFont;
					}
					if (info.IsInRift && info.FloorCoordinate is object && !Hud.Game.Quests.Any(q => q.SnoQuest.Sno == 337492 && q.State != QuestState.none)) //rift drop, not picked up, rift is closed
					{
						font = DisabledFont;
						nameUI.StrikeOut = true;
					}
					else
						nameUI.StrikeOut = false;
					nameUI.Font = font;
					nameUI.StaticText = info.Name;
					((LabelStringDecorator)nameUI.Hint).StaticText = info.Type;//+ "\n" + string.Join("\n", info.Stats.Select(s => s.Id + " " + s.Modifier)); // + " " + s.Processor.Code
					
					//type
					LabelRowDecorator typeUI = (LabelRowDecorator)row.Labels[3]; //4
					if (info.IsGambled)
					{
						//rank = "G";
						typeUI.Labels[0].Enabled = false;
						typeUI.Labels[1].Enabled = true;
						typeUI.Labels[2].Enabled = false;
						typeUI.Labels[3].Enabled = false;
						//nameUI.Hint = TextGambled;
						//font = GambledFont;
					}
					else if (info.IsCrafted)
					{
						typeUI.Labels[0].Enabled = false;
						typeUI.Labels[1].Enabled = false;
						typeUI.Labels[2].Enabled = true;
						typeUI.Labels[3].Enabled = false;
					}
					else {
						ILabelDecorator hint = null;
						/*if (info.IsCrafted)
						{
							rank = "🔨";
							font = CraftedFont;
							hint = TextCrafted;
						}
						else
						{*/
						typeUI.Labels[0].Enabled = true;
						typeUI.Labels[1].Enabled = false;
						typeUI.Labels[2].Enabled = false;
						typeUI.Labels[3].Enabled = true;

						if (info.FloorCoordinate is object)
						{
							rank = "🠟";
							font = DroppedFont;
							hint = HintDropped; //TextDropped; //(string.IsNullOrEmpty(info.AreaName) ? TextDropped : info.AreaName);
							//typeUI.Labels[0].Hint = HintDropped;
						}
						else
						{
							rank = "🠝";
							font = PickedUpFont;
							hint = HintPickedUp; //TextPickedUp;
							//typeUI.Labels[0].Hint = HintPickedUp;
						}

						((LabelStringDecorator)typeUI.Labels[3]).StaticText = info.AreaName;
						
						//typeUI.Decorators[0].Enabled = true;
						typeUI.Labels[0].Hint = hint;
						//nameUI.Hint = hint;
						//typeUI.Decorators[1].Enabled = false;

						((LabelStringDecorator)typeUI.Labels[0]).Font = font;
						((LabelStringDecorator)typeUI.Labels[0]).StaticText = rank;
					}
					
					return true;
				}
			};
			
			SummaryUI = new LabelRowDecorator(Hud,
				new LabelRowDecorator(Hud,
					new LabelStringDecorator(Hud, () => ComputedNormalCount.ToString()) {Font = NormalFont},
					new LabelStringDecorator(Hud, "О") {Font = NormalFont, SpacingLeft = 2}
				) {Hint = plugin.CreateHint(() => (ComputedMagicCount / ElapsedHours).ToString("0.##") + " Обычные/ч")/*, OnClick = ToggleView(), */},
				new LabelRowDecorator(Hud,
					new LabelStringDecorator(Hud, () => ComputedMagicCount.ToString()) {Font = MagicFont},
					new LabelStringDecorator(Hud, "М") {Font = MagicFont, SpacingLeft = 2}
				) {Hint = plugin.CreateHint(() => (ComputedMagicCount / ElapsedHours).ToString("0.##") + " Магические/ч"), /*OnClick = ToggleView(), */SpacingLeft = 15},
				new LabelRowDecorator(Hud,
					new LabelStringDecorator(Hud, () => ComputedRareCount.ToString()) {Font = RareFont},
					new LabelStringDecorator(Hud, "Р") {Font = RareFont, SpacingLeft = 2}
				) {Hint = plugin.CreateHint(() => (ComputedRareCount / ElapsedHours).ToString("0.##") + " Редкие/ч"), /*OnClick = ToggleView(), */SpacingLeft = 15},


				new LabelRowDecorator(Hud,
					new LabelStringDecorator(Hud, () => ComputedLegendaryCount.ToString()) {Font = LegendaryCountFont},
					new LabelStringDecorator(Hud, "Л") {Font = LegendaryFont, SpacingLeft = 2}
				) {Hint = plugin.CreateHint(() => (ComputedLegendaryCount / ElapsedHours).ToString("0.##") + " Легендарные/ч"), /*OnClick = ToggleView(), */SpacingLeft = 15},
				new LabelRowDecorator(Hud,
					new LabelStringDecorator(Hud, () => ComputedAncientCount.ToString()) {Font = AncientCountFont},
					new LabelStringDecorator(Hud, "Д") {Font = AncientFont, SpacingLeft = 2}
				) {Hint = plugin.CreateHint(() => (ComputedAncientCount / ElapsedHours).ToString("0.##") + " Древние/ч"), /*OnClick = ToggleView(), */SpacingLeft = 15},
				new LabelRowDecorator(Hud,
					new LabelStringDecorator(Hud, () => ComputedPrimalCount.ToString()) {Font = PrimalCountFont},
					new LabelStringDecorator(Hud, "П") {Font = PrimalFont, SpacingLeft = 2}
				) {Hint = plugin.CreateHint(() => (ComputedPrimalCount / ElapsedHours).ToString("0.##") + " Первозданные/ч"), /*OnClick = ToggleView(), */SpacingLeft = 15},
				new LabelRowDecorator(Hud,
					new LabelStringDecorator(Hud, () => CountEtherealDropped.ToString()) {Font = EtherealCountFont},
					new LabelStringDecorator(Hud, "Э") {Font = EtherealFont, SpacingLeft = 2}
				) {Hint = plugin.CreateHint(() => (CountEtherealDropped / ElapsedHours).ToString("0.##") + " Древние Эфирные Легендарные/ч"), /*OnClick = ToggleView(), */SpacingLeft = 15}//,
				
				/*new LabelRowDecorator(Hud,
					new LabelStringDecorator(Hud, () => CountAbandoned.ToString()) {Font = RedFont},
					new LabelStringDecorator(Hud, "❌") {Font = RedFont}
				) {Hint = plugin.CreateHint("Abandoned"), OnClick = ToggleAbandoned, Alignment = HorizontalAlign.Left, SpacingLeft = 5},
				new LabelRowDecorator(Hud,
					new LabelStringDecorator(Hud, () => AvgCompletionTicks == 0 ? TextIncomplete : ValueToString((long)AvgCompletionTicks * 1000 * TimeSpan.TicksPerMillisecond / 60, ValueFormat.LongTime)) {Hint = plugin.CreateHint(() => Completed.Count == 0 ? "Average Completion Time" : "Average Completion Time ("+Completed.Count+" Rifts)"), Font = GreenFont},
					new LabelStringDecorator(Hud, "🕓") {Font = TimeFont}
				) {Hint = plugin.CreateHint(() => Completed.Count == 0 ? "Average Completion Time" : "Average Completion Time ("+Completed.Count+" Rifts)"), Alignment = HorizontalAlign.Right, SpacingLeft = 5}*/
			) {
				BackgroundBrush = plugin.BgBrush, 
				SpacingLeft = 10, 
				SpacingRight = 10,
				SpacingTop = 2, 
				SpacingBottom = 2,
				OnBeforeRender = (label) => {
					ComputedNormalCount = Hud.Tracker.Session.DropWhite - CountNormalDropped;
					ComputedMagicCount = Hud.Tracker.Session.DropMagic - CountMagicDropped;
					ComputedRareCount = Hud.Tracker.Session.DropRare - CountRareDropped;
					ComputedLegendaryCount = (Hud.Tracker.Session.DropLegendary - CountLegendaryDropped) + CountLegendaryGambled + CountLegendaryCrafted;
					ComputedAncientCount = (Hud.Tracker.Session.DropAncient - CountAncientDropped) + CountAncientGambled + CountAncientCrafted;
					ComputedPrimalCount = (Hud.Tracker.Session.DropPrimalAncient - CountPrimalDropped) + CountPrimalGambled + CountPrimalCrafted;
					ElapsedHours = (Hud.Time.Now - StartTime).TotalHours;
					
					return true;
				},
			};
			
			Panel = new LabelColumnDecorator(Hud, 
				new LabelDelayedDecorator(Hud,
					new LabelAlignedDecorator(Hud, 
						new LabelStringDecorator(Hud, "ИСТОРИЯ ДОБЫЧИ") {Font = plugin.TitleFont, SpacingLeft = 15, SpacingRight = 15},
						plugin.CreateReset(this.Reset),
						plugin.CreatePin(this)
					)
				) {BackgroundBrush = plugin.BgBrush},
				SummaryUI,
				TableUI
			);
			
			//Plugin = plugin;
			HintDropped = plugin.CreateHint(TextDropped);
			HintPickedUp = plugin.CreateHint(TextPickedUp);
			HintCrafted = plugin.CreateHint(TextCrafted);
			HintGambled = plugin.CreateHint(TextGambled);
		}

		private void AddToHistory(IItem item, bool isGambled = false, bool isCrafted = false)
		{
			//if (item.Quality == ItemQuality.Legendary)
			//{
				List<ItemInfo> rankHistory = null;
				if (item.AncientRank > 0)
				{
					if (item.AncientRank == 1)
					{
						if (!ShowAncientHistory)
							return;
							
						rankHistory = HistoryAncient;
					}
					else 
					{
						if (!ShowPrimalHistory)
							return;
					
						rankHistory = HistoryPrimal;
					}
				}
				else
				{
					if (!ShowLegendaryHistory)
						return;
						
					rankHistory = HistoryLegendary;
				}
			//}
			if (rankHistory is object && !rankHistory.Any(i => i.Seed == item.Seed))
			{
				//bool resize = false;
				bool isInRift = (Hud.Game.Me.InGreaterRift && Hud.Game.Me.InGreaterRiftRank > 0) || Hud.Game.SpecialArea == SpecialArea.Rift;
				ItemInfo info = new ItemInfo(item, Hud.Time.Now, isGambled, isCrafted, isInRift, Hud.Game.Me.SnoArea?.NameLocalized) { Texture = Hud.Texture.GetItemTexture(item.SnoItem) }; //ItemInfo(IItem item, DateTime time, bool isGambled = false, bool isCrafted = false, bool isInRift = false, string areaName = null)
				
				//Hud.Sound.Speak(info.Type);
				
				History.Add(info);
				if (History.Count > HistoryShown)
				{
					History.RemoveAt(0);
					//resize = true;
				}
					
				if (!isInRift)
				{
					HistoryOutsideRift.Add(info);
					if (HistoryOutsideRift.Count > HistoryShown)
					{
						HistoryOutsideRift.RemoveAt(0);
						//resize = true;
					}
				}
				
				rankHistory.Add(info);
				if (rankHistory.Count > HistoryShown)
				{
					rankHistory.RemoveAt(0);
					//resize = true;
				}
				
				//if (resize)
				//	TableUI.Resize();
				
				//UpdateUI();
			}
		}
		
		private void SaveMaterialsCount()
		{
			CountReusableParts = Hud.Game.Me.Materials.ReusableParts;
			CountArcaneDust = Hud.Game.Me.Materials.ArcaneDust;
			CountVeiledCrystal = Hud.Game.Me.Materials.VeiledCrystal;
			CountDeathsBreath = Hud.Game.Me.Materials.DeathsBreath;
			CountForgottenSoul = Hud.Game.Me.Materials.ForgottenSoul;
			CountAct1Mat = Hud.Game.Me.Materials.KhanduranRune;
			CountAct2Mat = Hud.Game.Me.Materials.CaldeumNightShade;
			CountAct3Mat = Hud.Game.Me.Materials.ArreatWarTapestry;
			CountAct4Mat = Hud.Game.Me.Materials.CorruptedAngelFlesh;
			CountAct5Mat = Hud.Game.Me.Materials.WestmarchHolyWater;
			CountUber1Mat = Hud.Game.Me.Materials.LeoricsRegret;
			CountUber2Mat = Hud.Game.Me.Materials.VialOfPutridness;
			CountUber3Mat = Hud.Game.Me.Materials.IdolOfTerror;
			CountUber4Mat = Hud.Game.Me.Materials.HeartOfFright;
		}

		private void SaveInventorySeen()
		{
			InventorySeen = Hud.Inventory.ItemsInInventory.Where(i => i.Quality == ItemQuality.Legendary).Select(i => i.Seed).ToList();
		}
		
		/*private void UpdateUI()
		{
			if (Label is object)
			{
				MenuAlignedDecorator decorator = (MenuAlignedDecorator)Label;
				var dropped = Hud.Tracker.Session.DropLegendary - CountLegendaryDropped;
				if (dropped + CountLegendaryGambled + CountLegendaryCrafted > 0)
				{
					List<string> hint = new List<string>();
					
					if (dropped > 0)
						hint.Add(dropped.ToString() + " " + TextDropped);
					if (CountLegendaryGambled > 0)
						hint.Add(CountLegendaryGambled.ToString() + " " + TextGambled);
					if (CountLegendaryCrafted > 0)
						hint.Add(CountLegendaryCrafted.ToString() + " " + TextCrafted);
					
					decorator.Decorators[0].Enabled = true;
					decorator.Decorators[0].Hint = string.Join(" / ", hint);
					decorator.Decorators[1].Enabled = true;
				}
				else
				{
					decorator.Decorators[0].Enabled = false;
					decorator.Decorators[1].Enabled = false;
				}

				dropped = Hud.Tracker.Session.DropAncient - CountAncientDropped;
				if (dropped + CountAncientGambled + CountAncientCrafted > 0)
				{
					List<string> hint = new List<string>();
					if (dropped > 0)
						hint.Add(dropped.ToString() + TextDropped);
					if (CountAncientGambled > 0)
						hint.Add(CountAncientGambled.ToString() + TextGambled);
					if (CountAncientCrafted > 0)
						hint.Add(CountAncientCrafted.ToString() + TextCrafted);
					
					decorator.Decorators[2].Enabled = true;
					decorator.Decorators[2].Hint = string.Join(" / ", hint);
					decorator.Decorators[3].Enabled = true;
				}
				else
				{
					decorator.Decorators[2].Enabled = false;
					decorator.Decorators[3].Enabled = false;
				}

				
				dropped = Hud.Tracker.Session.DropPrimalAncient - CountPrimalDropped;
				if (dropped + CountPrimalGambled + CountPrimalCrafted > 0)
				{
					List<string> hint = new List<string>();
					if (dropped > 0)
						hint.Add(dropped.ToString() + TextDropped);
					if (CountPrimalGambled > 0)
						hint.Add(CountPrimalGambled.ToString() + TextGambled);
					if (CountPrimalCrafted > 0)
						hint.Add(CountPrimalCrafted.ToString() + TextCrafted);
					
					decorator.Decorators[4].Enabled = true;
					decorator.Decorators[4].Hint = string.Join(" / ", hint);
					decorator.Decorators[5].Enabled = true;
				}
				else
				{
					decorator.Decorators[4].Enabled = false;
					decorator.Decorators[5].Enabled = false;
				}
			}
		}*/
		
		private void Reset(ILabelDecorator label)
		{
			//forces the counter initialization to happen again
			LastUpdateTick = 0;
			
			//clear out history data
			History.Clear();
			HistoryOutsideRift.Clear();
			HistoryLegendary.Clear();
			HistoryAncient.Clear();
			HistoryPrimal.Clear();
			
			/*CountLegendaryDropped = 0;
			CountLegendaryGambled = 0;
			CountLegendaryCrafted = 0;
			CountAncientDropped = 0;
			CountAncientGambled = 0;
			CountAncientCrafted = 0;
			CountPrimalDropped = 0;
			CountPrimalGambled = 0;
			CountPrimalCrafted = 0;*/
			//redraw the table
			//TableUI.Resize();
		}
	}
}