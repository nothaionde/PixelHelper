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

	public class MenuBounties : BasePlugin, IMenuAddon, IAfterCollectHandler /*, ILeftClickHandler, IInGameTopPainter, ILeftClickHandler, IRightClickHandler*/
	{
		public string TextNone { get; set; } = "--";
		
		public string Id { get; set; }
		public int Priority { get; set; } //the priority on the dock to show this addon (smaller to the left, higher to the right)
		public string DockId { get; set; }
		public string Config { get; set; }
		
		public ILabelDecorator Label { get; set; }
		public ILabelDecorator LabelHint { get; set; }
		public float LabelSize { get; set; }
		public ILabelDecorator Panel { get; set; }

		public IFont TextFont { get; set; }
		public IFont AncientFont { get; set; }
		public IFont TimeFont { get; set; }
		public IFont DisabledFont { get; set; }
		public IFont YellowFont { get; set; }
		public IFont RedFont { get; set; }
		public IFont GreenFont { get; set; }
		
		public class BountyEvent {
			public DateTime Timestamp { get; set; }
			public BountyAct Act { get; set; }
			public uint Quest { get; set; }
			public string Line { get; set; }
			//public ITexture IconTexture { get; set; }
			//public int IconWidth { get; set; }
			//public int IconHeight { get; set; }
			//public IFont Font { get; set; }
		}
		
		public class BountyActInfo //Meta
		{
			public int Index { get; set; } //zero-indexed
			public BountyAct BountyAct { get; set; }
			public string Name { get; set; }
			public uint[] BossQuests { get; set; } //SnoQuest.Sno
			public uint CacheQuestSno { get; set; } //quest that grants the act cache quest reward
			
			//per-game data
			public Dictionary<uint, QuestState> QuestStates { get; set; } = new Dictionary<uint, QuestState>();
			public List<BountyEvent> EventHistory { get; set; } = new List<BountyEvent>();
			public int QuestsCompleted { get; set; }
			public int QuestsTotal { get; set; }
			public long StartElapsed { get; set; }
			public long EndElapsed { get; set; }
			//public uint CurrentBossQuest { get; set; }
			public string CurrentBossName { get; set; }
			public bool IsBossDead { get; set; }
			public bool IsCacheQuestTurnedIn { get; set; }
			//public DateTime? StartTime { get; set; }
			//public IWatch Timer { get; set; }
		}
		//public BountyActMeta[] BountyActMetaData { get; set; }
		//public Dictionary<BountyAct, BountyActInfo> Acts { get; set; } = new Dictionary<BountyAct, BountyActInfo>();
		public List<BountyActInfo> Acts { get; set; } = new List<BountyActInfo>();
		
		private Dictionary<uint, uint> QuestCompletionAreas = new Dictionary<uint, uint>() { //completion area sno, quest sno
			//act 1 bosses
			{19789, 361234}, //crypt of the skeleton king, kill skeleton king
			{62726, 345528}, //chamber of queen araneae, kill araneae
			{90881, 347032}, //chamber of suffering, kill the butcher

			//act 2 bosses
			{195268, 347558}, //lair of the witch, kill maghda
			{60194, 347656}, //soulstone chamber, kill zoltan kulle
			{60757, 358353}, //imperial palace, kill belial
			{467383, 474066}, //sanctum of blood, kill vidian
			
			//act 3 bosses
			{112580, 349242}, //edge of the abyss, kill siegebreaker
			{111232, 346166}, //the larder, kill ghom
			{111516, 349244}, //heart of sin, kill azmodan
			{119656, 349224}, //heart of the cursed, kill cydaea
			
			//act 4 bosses
			{215396, 361421}, //the great span, kill izual
			{143648, 349262}, //library of fate, kill rakanoth
			{215235, 349288}, //the crystal arch, kill diablo
			
			//act 5 bosses
			{287220, 359915}, //the great hall, kill adria
			{330576, 359927}, //heart of the fortress, kill malthael
			{308487, 359919}, //tower of korelan, kill urzael
		};
		private LabelTableDecorator TableUI;
		private LabelStringDecorator HintClaimed;
		private LabelStringDecorator HintComplete;
		private LabelStringDecorator HintIncomplete;
		private int QuestsCompleted;
		private int QuestsTotal;
		
		private BountyDropTracker Tracker;
		
        public MenuBounties()
        {
            Enabled = true;
			Priority = 30;
			DockId = "BottomRight";
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
			
			Acts.Add(/*BountyAct.A1,*/ new BountyActInfo()
			{
				Index = 1,
				Name = ToRoman(1),
				BountyAct = BountyAct.A1,
				BossQuests = new uint[]
				{
					347032, //Butcher 1
					345528, //Araneae 1
					361234, //Skeleton King 1
				},
				CacheQuestSno = 356988, //horadric reliquary
			});
			Acts.Add(/*BountyAct.A2,*/ new BountyActInfo()
			{
				Index = 2,
				Name = ToRoman(2),
				BountyAct = BountyAct.A2,
				BossQuests = new uint[]
				{
					347656, //Zultan Kulle 2
					358353, //Belial 2
					347558, //Maghda 2
					474066, //Vidian 2
				},
				CacheQuestSno = 356994, // = gift of the emperor
			});
			Acts.Add(/*BountyAct.A3,*/ new BountyActInfo()
			{
				Index = 3,
				Name = ToRoman(3),
				BountyAct = BountyAct.A3,
				BossQuests = new uint[]
				{
					346166, //Ghom 3
					349242, //Siegebreaker 3
					349244, //Azmodan 3
					349224, //Cydaea 3
				},
				CacheQuestSno = 356996, // = keep armament
			});
			Acts.Add(/*BountyAct.A4,*/ new BountyActInfo()
			{
				Index = 4,
				Name = ToRoman(4),
				BountyAct = BountyAct.A4,
				BossQuests = new uint[]
				{
					361421, //Izual 4
					349262, //Rakanoth 4
					349288, //Diablo 4
					//364336, //Sledge 4
					//409761, //Erra 4
					//364333, //Hammersmash 4
				},
				CacheQuestSno = 356999, // = blessings of the high heavens
			});
			Acts.Add(/*BountyAct.A5,*/ new BountyActInfo()
			{
				Index = 5,
				Name = ToRoman(5),
				BountyAct = BountyAct.A5,
				BossQuests = new uint[]
				{
					359915, //Adria 5
					359927, //Malthael 5
					359919, //Urzael 5
				},
				CacheQuestSno = 357001, // = westmarch stores
			});
        }
		
		public void OnRegister(MenuPlugin plugin)
		{
			
			
			TextFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 255, 255, 255, false, false, true);
			AncientFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 255, 120, 0, false, false, true);
			TimeFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 190, 255, 255, 255, false, false, true);
			DisabledFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 125, 125, 125, false, false, true);
			YellowFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 190, 255, 255, 55, false, false, true);
			RedFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 255, 0, 0, false, false, true);
			GreenFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 91, 237, 59, false, false, true);
			
			//Label
			Label = new LabelRowDecorator(Hud,
				/*new LabelStringDecorator(Hud, () => QuestsCompleted.ToString() ) {
					Font = GreenFont, 
					//SpacingLeft = 5, 
					OnBeforeRender = (label) => {
						if (QuestsCompleted == QuestsTotal)
							((LabelStringDecorator)label).Font = GreenFont;
						else if (QuestsCompleted >= ((float)QuestsTotal * 0.5f))
							((LabelStringDecorator)label).Font = YellowFont;
						else
							((LabelStringDecorator)label).Font = RedFont;
						
						return true;
					}
				},*/
				//new LabelStringDecorator(Hud, "/") {Font = GreenFont},
				new LabelStringDecorator(Hud, () => QuestsCompleted.ToString() + "/" + QuestsTotal) {Font = GreenFont},
				new LabelTextureDecorator(Hud, Hud.Texture.GetTexture(557320215)) {TextureHeight = 32, ContentHeight = plugin.MenuHeight, ContentWidth = 10}
			); //{ SpacingLeft = 15 }; //, SpacingRight = 15
			LabelHint = plugin.CreateHint("Bounties");
			HintClaimed = plugin.CreateHint("Complete - Cache Claimed");
			HintComplete = plugin.CreateHint("Complete - Cache Available");
			HintIncomplete = plugin.CreateHint("Не завершено");
			
			TableUI = new LabelTableDecorator(Hud, 
				new LabelRowDecorator(Hud,
					//completion status icon
					new LabelTextureDecorator(Hud) {TextureHeight = plugin.MenuHeight},
					//act name
					new LabelStringDecorator(Hud) {Font = TextFont, SpacingLeft = 5, SpacingRight = 5},
					//progress
					new LabelStringDecorator(Hud) {Font = TextFont, Alignment = HorizontalAlign.Left, SpacingLeft = 5, SpacingRight = 5},
					//boss status
					new LabelRowDecorator(Hud,
						new LabelTextureDecorator(Hud, Hud.Texture.GetTexture(3692681898)) {Hint = plugin.CreateHint("Босс Мертв"), TextureHeight = 34, ContentHeight = plugin.MenuHeight, SpacingLeft = 5}, 
						new LabelTextureDecorator(Hud, Hud.Texture.GetTexture(3153276977)) {Hint = plugin.CreateHint("Босс Жив"), TextureHeight = 34, ContentHeight = plugin.MenuHeight, SpacingLeft = 5}, //boss status
						new LabelStringDecorator(Hud) {Font = AncientFont, Alignment = HorizontalAlign.Left, SpacingLeft = 5, SpacingRight = 5} //boss name
					) {Alignment = HorizontalAlign.Left},
					//timer
					new LabelStringDecorator(Hud) {Font = TimeFont, Alignment = HorizontalAlign.Left, SpacingLeft = 5, SpacingRight = 5},
					//materials count
					new LabelRowDecorator(Hud,
						new LabelTextureDecorator(Hud) {TextureHeight = plugin.MenuHeight, SpacingLeft = 5, SpacingRight = 5},
						new LabelStringDecorator(Hud) {Font = TextFont, SpacingRight = 5}
					)
				) {SpacingTop = 2, SpacingBottom = 2}
			) { 
				BackgroundBrush = plugin.BgBrush,
				HoveredBrush = plugin.HighlightBrush,
				//SpacingLeft = 10,
				//SpacingRight = 10,
				SpacingBottom = 5,
				FillWidth = false, //true,
				OnFillRow = (row, index) => {
					if (index >= Acts.Count)
						return false;
							
					BountyActInfo info = Acts[index];
					
					//completion status icon
					LabelTextureDecorator iconUI = (LabelTextureDecorator)row.Labels[0];
					if (info.IsCacheQuestTurnedIn)
					{
						iconUI.Texture = Hud.Texture.GetTexture(1464158164); //557320215);
						iconUI.TextureHeight = 40;
						iconUI.TextureWidth = 0;
						iconUI.ContentHeight = plugin.MenuHeight;
						iconUI.ContentWidth = plugin.MenuHeight;
						iconUI.Hint = HintClaimed; //"Complete - Cache Claimed";
					}
					else if (info.QuestsTotal > 0 && info.QuestsCompleted == info.QuestsTotal)
					{
						iconUI.Texture = Hud.Texture.GetTexture(2854193535); //1303306988);
						iconUI.TextureHeight = 34;
						iconUI.TextureWidth = 0;
						iconUI.ContentHeight = plugin.MenuHeight;
						iconUI.ContentWidth = plugin.MenuHeight;
						iconUI.Hint = HintComplete; //"Complete - Cache Available";
					}
					else
					{
						iconUI.Texture = Hud.Texture.GetTexture(557320215); //1303306988);
						iconUI.TextureHeight = 34;
						iconUI.TextureWidth = 0;
						iconUI.ContentHeight = plugin.MenuHeight;
						iconUI.ContentWidth = plugin.MenuHeight;
						iconUI.Hint = HintIncomplete; //"Incomplete";
					}
					//iconUI.Height = plugin.MenuHeight;
					
					string events = string.Join("\n", info.EventHistory.Select(e => e.Line));
					if (string.IsNullOrEmpty(events))
						events = "placeholder";
					
					//act name
					LabelStringDecorator nameUI = (LabelStringDecorator)row.Labels[1];
					nameUI.StaticText = info.Name;
					//nameUI.Hint = events;
					//nameUI.TextFont = rule.Font;

					//progress
					LabelStringDecorator countUI = (LabelStringDecorator)row.Labels[2];
					countUI.StaticText = info.QuestsCompleted.ToString() + " / " + info.QuestsTotal;
					//countUI.Hint = events;
					if (info.QuestsCompleted == info.QuestsTotal)
						countUI.Font = GreenFont;
					else if (info.QuestsCompleted > 2)
						countUI.Font = YellowFont;
					else
						countUI.Font = RedFont;
					
					//boss status
					var bossDeadIcon = (LabelTextureDecorator)((LabelRowDecorator)row.Labels[3]).Labels[0];
					var bossAliveIcon = (LabelTextureDecorator)((LabelRowDecorator)row.Labels[3]).Labels[1];
					//iconUI.IconHeight = 38;
					LabelStringDecorator bossUI = (LabelStringDecorator)((LabelRowDecorator)row.Labels[3]).Labels[2]; //(MenuStringDecorator)view.Decorators[4];
					if (info.IsBossDead)
					{
						//iconUI.IconTexture = Hud.Texture.GetTexture(3692681898); //557320215);
						//iconUI.Hint = "Boss Dead";
						bossDeadIcon.Enabled = true;
						bossAliveIcon.Enabled = false;
						bossUI.Font = DisabledFont;
					}
					else
					{
						//iconUI.IconTexture = Hud.Texture.GetTexture(3153276977); //557320215);
						//iconUI.Hint = "Boss Alive";
						bossDeadIcon.Enabled = false;
						bossAliveIcon.Enabled = true;
						bossUI.Font = AncientFont;
					}
					//iconUI.IconHeight = plugin.MenuHeight - 2;
					//iconUI.Height = plugin.MenuHeight;
					
					//boss name
					//MenuStringDecorator bossUI = (MenuStringDecorator)view.Decorators[4];
					bossUI.StaticText = (string.IsNullOrEmpty(info.CurrentBossName) ? TextNone : System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(info.CurrentBossName));
					//bossUI.Resize();
					//totaltimeUI.TextFont = rule.Font;
					
					LabelStringDecorator timerUI = (LabelStringDecorator)row.Labels[4];
					//long elapsed = ((double)(info.StartElapsed - info.EndElapsed) / 1000d);
					timerUI.StaticText = ValueToString((info.StartElapsed - info.EndElapsed) * TimeSpan.TicksPerMillisecond, ValueFormat.LongTime);
					//percentUI.TextFont = rule.Font;
					
					LabelRowDecorator matUI = (LabelRowDecorator)row.Labels[5];
					switch (info.BountyAct)
					{
						case BountyAct.A1:
							((LabelStringDecorator)matUI.Labels[1]).StaticText = ValueToString(Hud.Game.Me.Materials.KhanduranRune, ValueFormat.NormalNumberNoDecimal);
							((LabelTextureDecorator)matUI.Labels[0]).Texture = Hud.Texture.GetItemTexture(Hud.Sno.SnoItems.p2_ActBountyReagent_01);
							break;
						case BountyAct.A2:
							((LabelStringDecorator)matUI.Labels[1]).StaticText = ValueToString(Hud.Game.Me.Materials.CaldeumNightShade, ValueFormat.NormalNumberNoDecimal);
							((LabelTextureDecorator)matUI.Labels[0]).Texture = Hud.Texture.GetItemTexture(Hud.Sno.SnoItems.p2_ActBountyReagent_02);
							break;
						case BountyAct.A3:
							((LabelStringDecorator)matUI.Labels[1]).StaticText = ValueToString(Hud.Game.Me.Materials.ArreatWarTapestry, ValueFormat.NormalNumberNoDecimal);
							((LabelTextureDecorator)matUI.Labels[0]).Texture = Hud.Texture.GetItemTexture(Hud.Sno.SnoItems.p2_ActBountyReagent_03);
							break;
						case BountyAct.A4:
							((LabelStringDecorator)matUI.Labels[1]).StaticText = ValueToString(Hud.Game.Me.Materials.CorruptedAngelFlesh, ValueFormat.NormalNumberNoDecimal);
							((LabelTextureDecorator)matUI.Labels[0]).Texture = Hud.Texture.GetItemTexture(Hud.Sno.SnoItems.p2_ActBountyReagent_04);
							break;
						case BountyAct.A5:
							((LabelStringDecorator)matUI.Labels[1]).StaticText = ValueToString(Hud.Game.Me.Materials.WestmarchHolyWater, ValueFormat.NormalNumberNoDecimal);
							((LabelTextureDecorator)matUI.Labels[0]).Texture = Hud.Texture.GetItemTexture(Hud.Sno.SnoItems.p2_ActBountyReagent_05);
							break;
					}
			
					return true;
				}
			};
			
			Panel = new LabelColumnDecorator(Hud, 
				new LabelDelayedDecorator(Hud,
					new LabelAlignedDecorator(Hud, 
						new LabelStringDecorator(Hud, "ПОРУЧЕНИЯ") {Font = plugin.TitleFont, SpacingLeft = 10, SpacingRight = 10},
						//plugin.CreateReset(this.Reset),
						plugin.CreatePin(this)
					)
				) {BackgroundBrush = plugin.BgBrush},
				//(ILabelDecorator)SummaryUI,
				new LabelAlignedDecorator(Hud,
					new LabelRowDecorator(Hud,
						new LabelStringDecorator(Hud, "⚠🡇") {Font = RedFont},
						new LabelTextureDecorator(Hud, Hud.Texture.GetItemTexture(Hud.Sno.SnoItems.HoradricCacheA1)) {TextureHeight = plugin.MenuHeight}
					) {Hint = plugin.CreateHint("Оповещение о выпадении награды"), Alignment = HorizontalAlign.Left, SpacingRight = 20},
					new LabelRowDecorator(Hud,
						new LabelStringDecorator(Hud, () => {
							var startElapsed = Acts.Max(a => a.StartElapsed);
							var endElapsed = Acts.Min(a => a.EndElapsed);
							//var time = TimeSpan.FromMilliseconds(startElapsed - endElapsed);
							return plugin.MillisecondsToString(startElapsed - endElapsed);
							//return TextNone;
						}) {Font = TextFont, SpacingRight = 2},
						new LabelStringDecorator(Hud, "🕓") {Font = TimeFont}
					) {Hint = plugin.CreateHint("Время завершения"), Alignment = HorizontalAlign.Right, SpacingLeft = 5}
				) {
					BackgroundBrush = plugin.BgBrush, 
					SpacingLeft = 5,//10, 
					SpacingRight = 5, //10,
					SpacingTop = 2,
					SpacingBottom = 2,
					OnBeforeRender = (label) => {
						((ILabelDecoratorCollection)label).Labels[0].Enabled = Tracker.CachesOnFloor.Count > 0;
						return true;
					}
				},
				TableUI
			);
			
			//Plugin = plugin;
			Tracker = Hud.GetPlugin<BountyDropTracker>();
		}
		
		public void Reset(ILabelDecorator label = null)
		{
			foreach (BountyActInfo act in Acts)
			{
				act.QuestStates.Clear();
				act.EventHistory.Clear();
				act.CurrentBossName = string.Empty;
				act.StartElapsed = 0;
				act.EndElapsed = 0;
			}
			
			//UI.Resize();
		}
		
		public void AfterCollect()
		{
			if (!Hud.Game.IsInGame)
				return;
			
			if (Label is object)
			{
				Label.Enabled = Hud.Game.Bounties is object && Hud.Game.Bounties.Any();
				
				if (!Label.Enabled)
					return;
			}
			
			bool init = false;
			QuestsCompleted = 0;
			QuestsTotal = 0;
			
			foreach (BountyActInfo act in Acts)
			{
				var quests = Hud.Game.Bounties.Where(b => b.SnoQuest.BountyAct == act.BountyAct);
				if (!quests.Any()) //sanity check
					continue;
				
				//init
				if (!act.QuestStates.Any())
				{
					init = true;
					
					foreach (var quest in quests)
					{
						act.QuestStates[quest.SnoQuest.Sno] = quest.State;
						
						if (act.BossQuests.Contains(quest.SnoQuest.Sno))
						{
							act.CurrentBossName = quest.SnoQuest.NameLocalized;
							//act.IsBossDead = quest.State == QuestState.completed;
						}
					}
				}
				else
				{
					act.QuestsCompleted = 0;
					act.QuestsTotal = 0;
					
					foreach (var quest in quests)
					{
						if (!act.QuestStates.ContainsKey(quest.SnoQuest.Sno))
						{
							//new set of bounties detected, clear the old data
							Reset();
							return;
						}

						//find the earliest start time
						if (quest.StartedOn.ElapsedMilliseconds > act.StartElapsed)
							act.StartElapsed = quest.StartedOn.ElapsedMilliseconds;
						
						if (quest.State == QuestState.completed)
						{
							//find the latest end time
							if (act.EndElapsed < act.StartElapsed || quest.CompletedOn.ElapsedMilliseconds < act.EndElapsed)
								act.EndElapsed = quest.CompletedOn.ElapsedMilliseconds;
								
							++act.QuestsCompleted;
							++QuestsCompleted;
						}
						
						if (act.BossQuests.Contains(quest.SnoQuest.Sno))
							act.IsBossDead = quest.State == QuestState.completed;

						if (act.QuestStates[quest.SnoQuest.Sno] != quest.State)
						{
							//find the player(s) responsible for changing the event state (find the players in the quest area)
							var players = string.Join(", ", Hud.Game.Players.Where(p => p.SnoArea == quest.SnoQuest.BountySnoArea || (QuestCompletionAreas.ContainsKey(p.SnoArea.Sno) && QuestCompletionAreas[p.SnoArea.Sno] == quest.SnoQuest.Sno)).Select(p => p.BattleTagAbovePortrait));
							
							//generate an event
							act.EventHistory.Add(new BountyEvent() {
								Timestamp = Hud.Time.Now,
								Quest = quest.SnoQuest.Sno,
								Line = string.IsNullOrEmpty(players) ? string.Format("{0} {1}", quest.SnoQuest.NameLocalized, quest.State) : string.Format("{0} {1} - {2}", quest.SnoQuest.NameLocalized, quest.State, players),
								//IconTexture = Hud.Texture.GetTexture((uint)(quest.State == QuestState.started ? 557320215 : 1464158164)),
							});
						}

              			
						act.QuestStates[quest.SnoQuest.Sno] = quest.State;
						
						++act.QuestsTotal;
						++QuestsTotal;
					}
					
					//var cacheQuest = Hud.Game.Quests.FirstOrDefault(q => q.SnoQuest.Sno == act.CacheQuestSno);
					act.IsCacheQuestTurnedIn = Hud.Game.Quests.First(q => q.SnoQuest.Sno == act.CacheQuestSno).State == QuestState.completed;
				}
			}
			
			if (init)
			{
				//parse out the quest text to extract the boss namespace
				Dictionary<BountyActInfo, string[]> tokens = new Dictionary<BountyActInfo, string[]>();
				foreach (BountyActInfo act in Acts)
				{
					if (!string.IsNullOrEmpty(act.CurrentBossName))
					{
						tokens.Add(act, act.CurrentBossName.Split(' '));			
					}
				}
				
				//find the quest name text that doesn't appear in other quest names
				foreach (KeyValuePair<BountyActInfo, string[]> pair in tokens)
				{
					string[] questName = pair.Value;
					string BossName = null;
					foreach (string s in questName)
					{
						//other tokens have this string fragment, skip
						if (tokens.Any(kvp => kvp.Key != pair.Key && !kvp.Value.Contains(s)))
						{
							if (string.IsNullOrWhiteSpace(BossName))
								BossName = s;
							else
								BossName += " " + s;
						}
					}
					
					pair.Key.CurrentBossName = BossName;
				}
			}
		}
		
		//recursive function adapted from https://stackoverflow.com/questions/7040289/converting-integers-to-roman-numerals
		public string ToRoman(int n)
		{
			if ((n < 1) || (n > 10)) return string.Empty;
			if (n == 10) return "X" + ToRoman(n - 10);
			if (n >= 9) return "IX" + ToRoman(n - 9);
			if (n >= 5) return "V" + ToRoman(n - 5);
			if (n >= 4) return "IV" + ToRoman(n - 4);
			if (n >= 1) return "I" + ToRoman(n - 1);
			return string.Empty;
		}
	}
}