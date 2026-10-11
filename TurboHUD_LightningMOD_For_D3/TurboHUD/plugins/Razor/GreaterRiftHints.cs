/*
	GreaterRiftHints Plugin by Razorfish
	Separated the following features from RunStats_GreaterRiftHelper into its own plugin:
	- highest GR unlocked party summary to Obelisk menu
	- Gem Upgrade party status display below Urshi (gem upgrade npc) and Orek (GR close npc) in the world
	- sound notification for Greater Rift invites
*/

namespace Turbo.Plugins.Razor
{
	using SharpDX.DirectWrite;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Media;
	using System.Threading;

	using Turbo.Plugins.Default;
	using Turbo.Plugins.Razor.Label;
	using Turbo.Plugins.Razor.Util;

	public class GreaterRiftHints : BasePlugin, IAfterCollectHandler, IInGameTopPainter //, IInGameWorldPainter
	{
		public bool ShowUpgradeHint { get; set; } = true;
		public bool ShowObeliskHint { get; set; } = true;
		public bool ShowPostRiftMonstersWarning { get; set; } = true;
		public bool NotifyRiftInvite { get; set; } = false;

		public string NotifyInviteSoundFile { get; set; } //optional (play this instead of TTS)
		public string TTS_RiftInvite { get; set; } = "Rift Invite";
		private SoundPlayer SoundRiftInvite;
		private bool NotifiedRiftInvite = false;
		
		public WorldDecoratorCollection MonsterWarningDecorator { get; set; }

		public IFont CountFont { get; set; }
		public IFont GoodFont { get; set; }
		public IFont BadFont { get; set; }
		public IFont TooltipFont { get; set; }
		
		public Dictionary<uint, Tuple<int, int, int, int>> GemUpgrades { get; set; } = new Dictionary<uint, Tuple<int, int, int, int>>(); //used, max, bonus, bonus max
		public Dictionary<uint, Tuple<int, int, int>> GreaterRiftCaps { get; set; } = new Dictionary<uint, Tuple<int, int, int>>(); //highest gr openable, shard cap
		
		private ILabelDecorator GemUpgradeUI;
		private ILabelDecorator ObeliskUI;
		private uint[] GemUpgradeKeys;
		private uint[] GreaterRiftCapKeys;
		
		public GreaterRiftHints()
		{
			Enabled = true;
		}

		public override void Load(IController hud)
		{
			base.Load(hud);

			CountFont = Hud.Render.CreateFont("tahoma", 7f, 255, 210, 210, 210, false, false, 100, 50, 15, 40, true);
			GoodFont = Hud.Render.CreateFont("tahoma", 7f, 255, 0, 255, 0, false, false, 100, 50, 15, 40, true);
			BadFont = Hud.Render.CreateFont("tahoma", 7f, 255, 255, 77, 0, false, false, 100, 50, 15, 40, true);
			TooltipFont = Hud.Render.CreateFont("tahoma", 7f, 255, 255, 255, 255, false, false, 100, 0, 0, 0, true);
			
			if (NotifyRiftInvite)
			{
				//load sound bite if specified
				if (!string.IsNullOrEmpty(NotifyInviteSoundFile))
					SoundRiftInvite = Hud.Sound.LoadSoundPlayer(NotifyInviteSoundFile);
				
				//register greater rift invite prompt/popup because it is not normally tracked by TH
				Hud.Render.RegisterUiElement("Root.NormalLayer.rift_join_party_main.LayoutRoot", Hud.Render.InGameBottomHudUiElement, null);
			}

			if (ShowUpgradeHint)
			{
				GemUpgradeUI = new LabelTableDecorator(Hud, 
					new LabelRowDecorator(Hud,
						new LabelStringDecorator(Hud) {Hint = CreateHint("Battle Tag"), Font = CountFont, SpacingRight = 10},
						new LabelStringDecorator(Hud) {Hint = CreateHint("Upgrades Done / Max"), Font = GoodFont, SpacingLeft = 10}
					)
				) {
					SpacingLeft = 10,
					SpacingRight = 10,
					SpacingTop = 5,
					SpacingBottom = 5,
					BackgroundBrush = Hud.Render.CreateBrush(150, 0, 0, 0, 0),
					OnBeforeRender = (label) => {GemUpgradeKeys = GemUpgrades.Keys.ToArray(); return true;},
					OnFillRow = (row, index) => {
						if (index >= GemUpgrades.Count)
							return false;
						
						Tuple<int, int, int, int> info = GemUpgrades[GemUpgradeKeys[index]]; //.ElementAt(index);
						//KeyValuePair<uint, Tuple<int, int, int, int>> pair = GemUpgrades.ElementAt(index);
						//foreach (KeyValuePair<uint, Tuple<uint, uint, uint, uint>> pair in GemUpgrades)
						//{
							IPlayer player = Hud.Game.Players.FirstOrDefault(p => p.HeroId == GemUpgradeKeys[index]); //pair.Key);
							if (player is object)
							{
								row.Enabled = true;
								
								((LabelStringDecorator)row.Labels[0]).StaticText = player.BattleTagAbovePortrait;
								((LabelStringDecorator)row.Labels[0]).Font = Hud.Render.GetHeroFont(player.HeroClassDefinition.HeroClass, 8f, true, false, true);
								
								//debug
								//((LabelStringDecorator)row.Labels[0].Hint).StaticText = player.HeroId.ToString();
								
								int spent = info.Item1 + (info.Item4 - info.Item3); //current used
								int max = info.Item2 + info.Item4; //total
								
								((LabelStringDecorator)row.Labels[1]).StaticText = spent.ToString() + " / " + max;
								((LabelStringDecorator)row.Labels[1]).Font = spent == max ? GoodFont : BadFont;
							}
							else
								row.Enabled = false;
						//}
						return true;
					}
				};
			}
			
			if (ShowObeliskHint)
			{
				ObeliskUI = new LabelTableDecorator(Hud, 
					new LabelRowDecorator(Hud,
						new LabelStringDecorator(Hud) {Hint = CreateHint("Battle Tag"), Font = CountFont, SpacingLeft = 10, SpacingRight = 10},
						new LabelStringDecorator(Hud) {Hint = CreateHint("Разблокированный уровень Великого Портала"), Font = CountFont, SpacingLeft = 10, SpacingRight = 10},
						new LabelRowDecorator(Hud,
							new LabelStringDecorator(Hud) {Font = BadFont, SpacingRight = 2},
							new LabelTextureDecorator(Hud, Hud.Texture.GetItemTexture(Hud.Inventory.GetSnoItem(2603730171))) {TextureHeight = 30, ContentHeight = 28, ContentWidth = 20}
						) {Hint = CreateHint("Кровавые осколки"), SpacingLeft = 10, SpacingRight = 10},
						new LabelStringDecorator(Hud) {Hint = CreateHint("Соло ВП (Этим героем)"), Font = GoodFont, SpacingLeft = 10, SpacingRight = 10}
					)
				) {
					BackgroundBrush = Hud.Render.CreateBrush(150, 0, 0, 0, 0),
					OnBeforeRender = (label) => {GreaterRiftCapKeys = GreaterRiftCaps.Keys.ToArray(); return true;},
					OnFillRow = (row, index) => {
						if (index >= GreaterRiftCaps.Count)
							return false;
						
						Tuple<int, int, int> info = GreaterRiftCaps[GreaterRiftCapKeys[index]]; //.ElementAt(index);
						//KeyValuePair<uint, Tuple<int, int, int>> pair = GreaterRiftCaps.ElementAt(index);
						//foreach (KeyValuePair<uint, Tuple<int, int, int>> pair in GreaterRiftCaps)
						//{
							IPlayer player = Hud.Game.Players.FirstOrDefault(p => p.HeroId == GreaterRiftCapKeys[index]); //pair.Key);
							if (player is object)
							{
								row.Enabled = true;
								
								((LabelStringDecorator)row.Labels[0]).StaticText = player.BattleTagAbovePortrait;
								((LabelStringDecorator)row.Labels[0]).Font = Hud.Render.GetHeroFont(player.HeroClassDefinition.HeroClass, 8f, true, false, true);
								((LabelStringDecorator)row.Labels[0].Hint).StaticText = player.HeroId.ToString();
								
								((LabelStringDecorator)row.Labels[1]).StaticText = info.Item1.ToString();
								((LabelStringDecorator)((LabelRowDecorator)row.Labels[2]).Labels[0]).StaticText = (500 + (info.Item2 * 10)).ToString();
								((LabelStringDecorator)row.Labels[2].Hint).StaticText = "Кровавых осколков\nПри прохождение Великого порртала: " + info.Item2;
								((LabelStringDecorator)row.Labels[3]).StaticText = info.Item3.ToString();
							}
							else
								row.Enabled = false;
							
						//}
						return true;
					}
				};
			}
			
			if (ShowPostRiftMonstersWarning)
			{
				MonsterWarningDecorator = new WorldDecoratorCollection(
					new GroundCircleDecorator(Hud)
					{
						Brush = Hud.Render.CreateBrush(200, 255, 0, 0, 7f),
						Radius = -1f,
						RadiusTransformator = new StandardPingRadiusTransformator(hud, 1000, 0.95f, 1.05f), //1000 - animation interval, higher is slower
					}
				);
			}
		}
		
		public void OnNewArea(bool newGame, ISnoArea area)
		{
			if (newGame)
			{
				if (ShowUpgradeHint)
					GemUpgrades.Clear();
				if (ShowObeliskHint)
					GreaterRiftCaps.Clear();
				NotifiedRiftInvite = false;
			}
		}
		
		public void AfterCollect()
		{
			if (!Hud.Game.IsInGame)
				return;
			
			IQuest riftQuest = Hud.Game.Quests.FirstOrDefault(q => q.SnoQuest.Sno == 337492 && q.QuestStepId == 34);
			
			foreach (IPlayer player in Hud.Game.Players.Where(p => p.HasValidActor && p.CoordinateKnown))
			{
				//save obelisk data
				if (ShowObeliskHint)
				{
					//todo: this data isn't going to change that often, can optimize this to only execute rarely
					
					//save gr cap data
					int grCap = player.GetAttributeValueAsInt(Hud.Sno.Attributes.Highest_Unlocked_Rift_Level, uint.MaxValue, 1);
					//int grShardCap = 500 + (player.HighestSoloRiftLevel * 10);
					//int grSolo = p.HighestSoloRiftLevel;
						
					if (!GreaterRiftCaps.ContainsKey(player.HeroId))
						GreaterRiftCaps[player.HeroId] = new Tuple<int, int, int>(grCap, player.HighestSoloRiftLevel, player.HighestHeroSoloRiftLevel); //grCap, grShardCap
					else
					{
						Tuple<int, int, int> caps = GreaterRiftCaps[player.HeroId];
						if (caps.Item1 != grCap || caps.Item2 != player.HighestSoloRiftLevel || caps.Item3 != player.HighestHeroSoloRiftLevel)
							GreaterRiftCaps[player.HeroId] = new Tuple<int, int, int>(grCap, player.HighestSoloRiftLevel, player.HighestHeroSoloRiftLevel);
					}
				}
				
				//save gem upgrade data
				if (ShowUpgradeHint)
				{
					if (riftQuest is object)
					{
						int used = player.GetAttributeValueAsInt(Hud.Sno.Attributes.Jewel_Upgrades_Used, uint.MaxValue, 0);
						int max = player.GetAttributeValueAsInt(Hud.Sno.Attributes.Jewel_Upgrades_Max, uint.MaxValue, 0);
						int bonus = player.GetAttributeValueAsInt(Hud.Sno.Attributes.Jewel_Upgrades_Bonus, uint.MaxValue, 0);

						if (GemUpgrades.ContainsKey(player.HeroId))
						{
							Tuple<int, int, int, int> status = GemUpgrades[player.HeroId]; //used, max, bonus, bonus max

							if (used != status.Item1 || max > status.Item2 || bonus != status.Item3 || bonus > status.Item4)
								GemUpgrades[player.HeroId] = new Tuple<int, int, int, int>(used, (max > status.Item2 ? max : status.Item2), bonus, (bonus > status.Item4 ? bonus : status.Item4));
						}
						else
							GemUpgrades[player.HeroId] = new Tuple<int, int, int, int>(used, max, bonus, bonus);
					}
					else
					{
						if (GemUpgrades.Count > 0)
							GemUpgrades.Clear();
					}
				}
			}
			
			
			if (NotifyRiftInvite && riftQuest == null)
			{
				IUiElement uiTest = Hud.Render.GetUiElement("Root.NormalLayer.rift_join_party_main.LayoutRoot"); //Hud.Render.RegisterUiElement("Root.NormalLayer.rift_join_party_main.LayoutRoot", Hud.Render.InGameBottomHudUiElement, null);
				if (uiTest.Visible)
				{
					if (!NotifiedRiftInvite)
					{
						NotifiedRiftInvite = true;				
						ThreadPool.QueueUserWorkItem(state => {
							try {
								if (SoundRiftInvite is object) SoundRiftInvite.Play();
								else if (!string.IsNullOrEmpty(TTS_RiftInvite)) Hud.Sound.Speak(TTS_RiftInvite);
							} catch (Exception) {}
						});
					}
				}
				else if (NotifiedRiftInvite)
					NotifiedRiftInvite = false;
			}
		}
		
		public void PaintTopInGame(ClipState clipState)
		{
			if (clipState != ClipState.AfterClip)
				return;
			
			//draw obelisk hint - gr related info on the rift opening window if it is open
			if (ShowObeliskHint && Hud.Game.IsInTown && GreaterRiftCaps.Count > 0)
			{
				IUiElement ui = Hud.Render.GetUiElement("Root.NormalLayer.rift_dialog_mainPage");
				IUiElement blocking = Hud.Render.GetUiElement("Root.NormalLayer.chatentry_dialog_backgroundScreen.chatentry_content.chat_editline");
				if (ui is object && ui.Visible && !blocking.Visible) {
					//don't show if the gr difficulty drop down is visible
					blocking = Hud.Render.GetUiElement("Root.TopLayer.DropDown._content");
					if (blocking is object && blocking.Visible)
						return;
					
					float x = ui.Rectangle.Width * 0.103f; //Hud.Window.Size.Width * 0.0285f;
					float y = ui.Rectangle.Height * 0.6086f; //Hud.Window.Size.Height;
					ObeliskUI.Paint(x, y);
				}
			}
			
			//draw gem upgrades
			if (ShowUpgradeHint && GemUpgrades.Count > 0)
			{
				IActor npc = null;
				if (Hud.Game.IsInTown)
					npc = Hud.Game.Actors.FirstOrDefault(a => a.SnoActor.Sno == ActorSnoEnum._x1_lr_nephalem && a.IsOnScreen); //Orek
				else if (Hud.Game.Me.InGreaterRift)
					npc = Hud.Game.Actors.FirstOrDefault(a => a.SnoActor.Sno == ActorSnoEnum._p1_lr_tieredrift_nephalem && a.IsOnScreen); //Urshi
				
				if (npc is object)
					GemUpgradeUI.Paint(npc.ScreenCoordinate.X - GemUpgradeUI.Width * 0.5f, npc.ScreenCoordinate.Y);
			}
			
			if (ShowPostRiftMonstersWarning && Hud.Game.Me.InGreaterRift && Hud.Game.SpecialArea != SpecialArea.GreaterRift && Hud.Game.Me.InGreaterRiftRank > 0 && Hud.Game.Quests.Any(q => q.SnoQuest.Sno == 337492 && (q.QuestStepId == 34 || q.QuestStepId == 46))) // && Hud.Game.SpecialArea != SpecialArea.ChallengeRift && Hud.Game.SpecialArea != SpecialArea.ChallengeRiftHub
			{
				//IQuest riftQuest = Hud.Game.Quests.FirstOrDefault(q => q.SnoQuest.Sno == 337492);
				//if (riftQuest == null) return;
				//if (riftQuest.QuestStepId == 34 || riftQuest.QuestStepId == 46)
				//{
					//int count = 0;
					foreach (IMonster m in Hud.Game.AliveMonsters)
					//{
						MonsterWarningDecorator.Paint(WorldLayer.Ground, m, m.FloorCoordinate, m.SnoActor.NameLocalized);
						//++count;
					//}
				//}
				
				//show warning sign if you're in combat
				//if (count > 0 && Hud.Game.Me.InCombat)					
			}

			/*Tuple<int, int, int, int> status = GemUpgrades[player.HeroId]; //used, max, bonus, bonus max
			int spent = status.Item1 + (status.Item4 - status.Item3); //current used
			int max = status.Item2 + status.Item4; //total
			string txt = string.Format("{0} / {1}", spent, max);

			row[0].TextFont = (spent < max ? PrimalFont : SetFont);
			row[0].TextFunc = () => txt;*/
		}
		
		private LabelStringDecorator CreateHint(string text)
		{
			return new LabelStringDecorator(Hud, text) {Font = TooltipFont, SpacingLeft = 3, SpacingRight = 3, SpacingTop = 3, SpacingBottom = 3};
		}
	}
}