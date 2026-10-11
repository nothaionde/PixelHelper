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
	using Turbo.Plugins.Razor.Util;

	public class MenuCrowdControl : BasePlugin, IMenuAddon, IAfterCollectHandler, IInGameTopPainter /*, ILeftClickHandler, IRightClickHandler*/
	{
		public int HistoryLimit { get; set; } = 200; //how many history entries to remember for computations
		public int HistoryView { get; set; } = 15; //while in town
		public float HistoryViewTimeout { get; set; } = 10; //in seconds, applies only while pinned, while outside of town
		
		public enum CCType { Stun, Freeze, Root, Fear, Knockback, Slow, Jail, Vortex, Stone, Blind, Immobilize, Charm }
		
		public bool ShowLabels { get; set; } = true;
		public bool ShowPlayerControlled { get; set; } = true;
		public bool ShowMonsterControlled { get; set; } = true;
		
		public Func<IPlayer, bool> PlayerFilter { get; set; }
		public Func<IMonster, bool> MonsterFilter { get; set; }
		public Dictionary<CCType, CCRule> PlayerRules { get; set; }
		public Dictionary<CCType, CCRule> MonsterRules { get; set; }

		public List<CCEvent> History = new List<CCEvent>();
		
		public float IconSizeMultiplier { get; set; } = 0.7f;
		public float MonsterIconSizeMultiplier { get; set; } = 1f; //IconSizeMultiplier * MonsterIconSizeMultiplier
		public float BossIconSizeMultiplier { get; set; } = 1.3f;
		public float IconSize => 55f / 1200.0f * Hud.Window.Size.Height * IconSizeMultiplier;
        public float IconSpacing => 3.0f / 1200.0f * Hud.Window.Size.Height * IconSizeMultiplier;
		
		public string Id { get; set; }
		public int Priority { get; set; } //the priority on the dock to show this addon (smaller to the left, higher to the right)
		public string DockId { get; set; }
		public string Config { get; set; }

		public ILabelDecorator Label { get; set; }
		public ILabelDecorator LabelHint { get; set; }
		public float LabelSize { get; set; }
		public ILabelDecorator Panel { get; set; }

		public IFont TextFont { get; set; }
		public IFont NegativeFont { get; set; }
		public IFont PositiveFont { get; set; }
		//public IFont ChampionFont { get; set; }
		//public IFont RareFont { get; set; }
		//public IFont BossFont { get; set; }
		public Dictionary<ActorRarity, IFont> RarityFonts { get; set; }
		public IBrush PositiveBrush { get; set; }
		
		private LabelTableDecorator TableUI;
		private bool IsInGame;
		private int LastGameTickSeen;
		//private bool HistoryChanged = false;
		
		public class CCEvent
		{
			public uint Id { get; set; } //HeroId for players, AnnId for monsters
			public string Name { get; set; }
			public bool IsPlayer { get; set; }
			public IFont Font { get; set; }

			public CCType Type { get; set; }

			public DateTime Timestamp { get; set; }
			public int StartTick { get; set; } = -1;
			public int FinishTick { get; set; } = -1;
			public int LastSeen { get; set; }

			public CCEvent() {}
		}
		
		public class CCRule
		{
			public CCType Type { get; set; }
			public string Name { get; set; }
			public ITexture Texture { get; set; }
			public IFont Font { get; set; }
			public Func<IActor, bool> Rule { get; set; }
		}
		
        public MenuCrowdControl()
        {
            Enabled = true;
			Priority = 10;
			DockId = "BottomLeft";
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
			
			PlayerFilter = (player) => player.IsMe;
			MonsterFilter = (monster) => !monster.Illusion && (monster.Rarity == ActorRarity.Champion || monster.Rarity == ActorRarity.Rare || monster.Rarity == ActorRarity.Boss || monster.Rarity == ActorRarity.Unique);
			
			PlayerRules = new Dictionary<CCType, CCRule>() {
				{ CCType.Freeze, new CCRule() { Type = CCType.Freeze, Name = "Заморозка", Texture = Hud.Texture.GetTexture("DebuffFrozen"), Rule = (actor) => ((IPlayer)actor).Powers.Frozen } },
				{ CCType.Stun, new CCRule() { Type = CCType.Stun, Name = "Оглушение", Texture = Hud.Texture.GetTexture("Debuff_General_Stun"), Rule = (actor) => ((IPlayer)actor).Powers.Stunned } },
				{ CCType.Fear, new CCRule() { Type = CCType.Fear, Name = "Страх", Texture = Hud.Texture.GetTexture("Debuff_General_Fear"), Rule = (actor) => actor.GetAttributeValueAsInt(Hud.Sno.Attributes.Feared, uint.MaxValue) == 1 } },
				{ CCType.Root, new CCRule() { Type = CCType.Root, Name = "Root", Texture = Hud.Texture.GetTexture("DebuffRooted"), Rule = (actor) => ((IPlayer)actor).Powers.Rooted } },
				{ CCType.Jail, new CCRule() { Type = CCType.Jail, Name = "Тюрьма", Texture = Hud.Texture.GetTexture("Debuff_General_Immobilize"), Rule = (actor) => actor.GetAttributeValueAsInt(Hud.Sno.Attributes.Buff_Icon_Count0, 222744) == 1 } },
				{ CCType.Slow, new CCRule() { Type = CCType.Slow, Name = "Замедление", Texture = Hud.Texture.GetTexture("Debuff_General_Slow"), Rule = (actor) => ((IPlayer)actor).Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_DebuffSlowed.Sno) } },
				{ CCType.Vortex, new CCRule() { Type = CCType.Vortex, Name = "Вихрь", Texture = Hud.Texture.GetTexture("DebuffStun"), Rule = (actor) => actor.GetAttributeValueAsInt(Hud.Sno.Attributes.Buff_Icon_Count0, 70432) == 1 && actor.GetAttributeValueAsInt(Hud.Sno.Attributes.Buff_Icon_Count0, 120305) == 1 } },
				{ CCType.Knockback, new CCRule() { Type = CCType.Knockback, Name = "Отбрасывание", Texture = Hud.Texture.GetTexture("DebuffStun"), Rule = (actor) => actor.GetAttributeValueAsInt(Hud.Sno.Attributes.Buff_Icon_Count0, 70432) == 1 && actor.GetAttributeValueAsInt(Hud.Sno.Attributes.Buff_Icon_Count0, 120305) != 1 } },
				{ CCType.Stone, new CCRule() { Type = CCType.Stone, Name = "Камень", Texture = Hud.Texture.GetTexture(Hud.Sno.SnoPowers.StoneGauntlets.Icons[1].TextureId), Rule = (actor) => ((IPlayer)actor).Powers.BuffIsActive(Hud.Sno.SnoPowers.StoneGauntlets.Sno, 2) } },
			};

			MonsterRules = new Dictionary<CCType, CCRule>() {
				{ CCType.Freeze, new CCRule() { Type = CCType.Freeze, Name = "Заморозка", Texture = Hud.Texture.GetTexture("DebuffFrozen"), Rule = (actor) => ((IMonster)actor).Frozen } },
				{ CCType.Stun, new CCRule() { Type = CCType.Stun, Name = "Оглушение", Texture = Hud.Texture.GetTexture("Debuff_General_Stun"), Rule = (actor) => ((IMonster)actor).Stunned } },
				{ CCType.Blind, new CCRule() { Type = CCType.Blind, Name = "Ослепление", Texture = Hud.Texture.GetTexture("Debuff_General_Blind"), Rule = (actor) => ((IMonster)actor).Blind } },
				{ CCType.Fear, new CCRule() { Type = CCType.Fear, Name = "Страх", Texture = Hud.Texture.GetTexture("Debuff_General_Fear"), Rule = (actor) => actor.GetAttributeValueAsInt(Hud.Sno.Attributes.Feared, uint.MaxValue) == 1 } },
				{ CCType.Jail, new CCRule() { Type = CCType.Jail, Name = "Тюрьма", Texture = Hud.Texture.GetTexture("Debuff_General_Immobilize"), Rule = (actor) => actor.GetAttributeValueAsInt(Hud.Sno.Attributes.Buff_Icon_Count0, 222744) == 1 } },
				{ CCType.Immobilize, new CCRule() { Type = CCType.Immobilize, Name = "Суждение", Texture = Hud.Texture.GetTexture("Debuff_General_Immobilize"), Rule = (actor) => actor.GetAttributeValue(Hud.Sno.Attributes.Power_Buff_0_Visual_Effect_E,267600) == 1 || actor.GetAttributeValue(Hud.Sno.Attributes.Power_Buff_0_Visual_Effect_D,267600) == 1 || actor.GetAttributeValue(Hud.Sno.Attributes.Power_Buff_0_Visual_Effect_C,267600) == 1 || actor.GetAttributeValue(Hud.Sno.Attributes.Power_Buff_0_Visual_Effect_B,267600) == 1 || actor.GetAttributeValue(Hud.Sno.Attributes.Power_Buff_0_Visual_Effect_A,267600) == 1 || actor.GetAttributeValue(Hud.Sno.Attributes.Power_Buff_0_Visual_Effect_None,267600) == 1 } }, //borrowed from RNN's Shield of Fury Stacks plugin
				//new CCRule() { IsPlayer = true, Type = CCType.Jail, Rule = (actor) => actor.GetAttributeValueAsInt(Hud.Sno.Attributes.Buff_Icon_Count0, 222744) == 1 },
				//new CCRule() { IsPlayer = true, Type = CCType.Slow, Rule = (actor) => ((IPlayer)actor).Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_DebuffSlowed.Sno) },
				//new CCRule() { IsPlayer = true, Type = CCType.Stone, Rule = (actor) => ((IPlayer)actor).Powers.BuffIsActive(Hud.Sno.SnoPowers.StoneGauntlets.Sno, 2) }
			};
			
			/*
			charm?
			*/
			
			NegativeFont = Hud.Render.CreateFont("tahoma", 6.5f, 255, 255, 0, 0, true, false, 160, 0, 0, 0, true);
			PositiveFont = Hud.Render.CreateFont("tahoma", 6.5f, 255, 0, 255, 0, true, false, 160, 0, 0, 0, true);
			//ChampionFont = Hud.Render.CreateFont("tahoma", 8f, 255, 64, 128, 255, true, false, 160, 0, 0, 0, true);
			//RareFont = Hud.Render.CreateFont("tahoma", 8f, 255, 255, 148, 20, true, false, 160, 0, 0, 0, true);
			//BossFont = Hud.Render.CreateFont("tahoma", 8f, 255, 184, 255, 20, true, false, 160, 0, 0, 0, true);
			PositiveBrush = Hud.Render.CreateBrush(255, 0, 255, 0, 2);
			
			RarityFonts = new Dictionary<ActorRarity, IFont>() {
				{ActorRarity.Champion, Hud.Render.CreateFont("tahoma", 8f, 255, 64, 128, 255, false, false, 160, 0, 0, 0, true)},
				{ActorRarity.Rare, Hud.Render.CreateFont("tahoma", 8f, 255, 255, 148, 20, false, false, 160, 0, 0, 0, true)},
				{ActorRarity.Boss, Hud.Render.CreateFont("tahoma", 8f, 255, 184, 255, 20, false, false, 160, 0, 0, 0, true)},
			};
        }
		
		public void AfterCollect()
		{
			if (!Hud.Game.IsInGame || Hud.Game.IsInTown)
				return;
			
			//check for player ccs
			foreach (IPlayer player in Hud.Game.Players.Where(p => PlayerFilter(p)))
			{
				foreach (CCRule rule in PlayerRules.Values)
				{
					if (rule.Rule(player))
					{
						CCEvent cce = History.FirstOrDefault(e => e.IsPlayer && e.Id == player.HeroId && e.Type == rule.Type && e.FinishTick == -1);
						if (cce is object)
							cce.LastSeen = Hud.Game.CurrentGameTick;
						else
							AddEvent(new CCEvent() { IsPlayer = true, Id = player.HeroId, Name = player.BattleTagAbovePortrait, Font = Hud.Render.GetHeroFont(player.HeroClassDefinition.HeroClass, 8f, true, false, true), Type = rule.Type, Timestamp = Hud.Time.Now, StartTick = Hud.Game.CurrentGameTick, LastSeen = Hud.Game.CurrentGameTick });
					}
					else
					{
						CCEvent cce = History.FirstOrDefault(e => e.IsPlayer && e.Id == player.HeroId && e.Type == rule.Type && e.FinishTick == -1);
						if (cce is object)
							cce.FinishTick = cce.LastSeen;
					}
				}
			}
			
			//check for monster ccs
			foreach (IMonster monster in Hud.Game.AliveMonsters.Where(m => MonsterFilter(m)))
			{
				foreach (CCRule rule in MonsterRules.Values)
				{
					if (rule.Rule(monster))
					{
						CCEvent cce = History.FirstOrDefault(e => !e.IsPlayer && e.Id == monster.AnnId && e.Type == rule.Type && e.FinishTick == -1);
						if (cce is object)
							cce.LastSeen = Hud.Game.CurrentGameTick;
						else
							AddEvent(new CCEvent() { IsPlayer = false, Id = monster.AnnId, Name = monster.SnoMonster.NameLocalized, Font = RarityFonts.ContainsKey(monster.Rarity) ? RarityFonts[monster.Rarity] : TextFont, Type = rule.Type, Timestamp = Hud.Time.Now, StartTick = Hud.Game.CurrentGameTick, LastSeen = Hud.Game.CurrentGameTick });
					}
					else
					{
						CCEvent cce = History.FirstOrDefault(e => !e.IsPlayer && e.Id == monster.AnnId && e.Type == rule.Type && e.FinishTick == -1);
						if (cce is object)
							cce.FinishTick = cce.LastSeen;
					}
				}
			}
			
			//check in on history entries that didn't get updated
			foreach (CCEvent cce in History.Where(e => e.FinishTick == -1 && e.LastSeen < Hud.Game.CurrentGameTick))
			{
				if (cce.IsPlayer)
				{
					if (!Hud.Game.Players.Any(p => p.HeroId == cce.Id && p.IsDead))
						cce.FinishTick = cce.LastSeen;
				}
				else
				{
					if (Hud.Game.Monsters.Any(m => m.AnnId == cce.Id)) //monster probably died
						cce.FinishTick = cce.LastSeen;
				}
			}
		}
		
		public void PaintTopInGame(ClipState clipState)
		{
			if (clipState != ClipState.AfterClip)
				return;
			
			if (Hud.Game.IsInTown)
				return;
			
			if (ShowPlayerControlled)
			{
				foreach (IPlayer player in Hud.Game.Players.Where(p => p.IsOnScreen && PlayerFilter(p)))
				{
					IEnumerable<CCEvent> current = History.Where(e => e.IsPlayer && e.Id == player.HeroId && e.FinishTick == -1); //.OrderBy(e => e.StartTick);
					
					if (!current.Any())
						continue;
					
					int count = current.Count();
					float width = count*IconSize + (count - 1)*IconSpacing;
					IScreenCoordinate pos = player.FloorCoordinate.ToScreenCoordinate();
					IScreenCoordinate pos2 = player.CollisionCoordinate.ToScreenCoordinate();
					float x = pos.X - width*0.5f;
					float y = pos2.Y - IconSize*0.5f;

					var tmp = IconSizeMultiplier;
					IconSizeMultiplier = IconSizeMultiplier; //*BossSizeMultiplier;
					foreach (CCEvent e in current)
					{
						if (ShowLabels)
						{
							TextLayout layout = NegativeFont.GetTextLayout(PlayerRules[e.Type].Name);
							//Plugin.BgBrushAlt.DrawRectangle(x + IconSize*0.5f - layout.Metrics.Width*0.5f - IconSpacing - 1, y + IconSize - 1, layout.Metrics.Width + IconSpacing*2 + 2, layout.Metrics.Height + IconSpacing*2 + 2);
							//TextFont.DrawText(layout, x + IconSize*0.5f - layout.Metrics.Width*0.5f, y + IconSize + IconSpacing);
							//BackgroundBrush?.DrawRectangle(x + IconSize*0.5f - layout.Metrics.Width*0.5f - IconSpacing - 1, y - IconSpacing*2 - layout.Metrics.Height - 1, layout.Metrics.Width + IconSpacing*2 + 2, layout.Metrics.Height + IconSpacing*2 + 2);
							NegativeFont.DrawText(layout, x + IconSize*0.5f - layout.Metrics.Width*0.5f, y - IconSpacing - layout.Metrics.Height);
								
							//time elapsed
							layout = NegativeFont.GetTextLayout(((float)(Hud.Game.CurrentGameTick - e.StartTick)/60f).ToString("F1") + "s");
							NegativeFont.DrawText(layout, x + IconSize*0.5f - layout.Metrics.Width*0.5f, y + IconSize + IconSpacing);
						}

						var texture = PlayerRules[e.Type].Texture;
						if (texture is object)
						{
							texture.Draw(x, y, IconSize, IconSize, 1f);
							Hud.Texture.DebuffFrameTexture.Draw(x, y, IconSize, IconSize, 1f);
							//CCBrush.DrawRectangle(x, y, IconSize, IconSize);
							//Hud.Texture.DebuffFrameTexture.Draw(x, y, IconSize, IconSize, 1f);
							//Hud.Texture.DebuffFrameTexture.Draw(x, y, IconSize, IconSize + layout.Metrics.Height + IconSpacing*2, 1f);
						}

						x += IconSize + IconSpacing;
					}
					IconSizeMultiplier = tmp;
				}
			}
			
			if (ShowMonsterControlled)
			{
				foreach (IMonster monster in Hud.Game.AliveMonsters.Where(m => m.IsOnScreen && MonsterFilter(m)))
				{
					IEnumerable<CCEvent> current = History.Where(e => !e.IsPlayer && e.Id == monster.AnnId && e.FinishTick == -1); //.OrderBy(e => e.StartTick);
					
					if (!current.Any())
						continue;
					
					int count = current.Count();
					float width = count*IconSize + (count - 1)*IconSpacing;
					IScreenCoordinate pos = monster.FloorCoordinate.ToScreenCoordinate();
					IScreenCoordinate pos2 = monster.CollisionCoordinate.ToScreenCoordinate();
					float x = pos.X - width*0.5f;
					float y = pos2.Y - IconSize*0.5f;

					var tmp = IconSizeMultiplier;
					IconSizeMultiplier = IconSizeMultiplier * (monster.Rarity == ActorRarity.Boss ? BossIconSizeMultiplier : MonsterIconSizeMultiplier);
					foreach (CCEvent e in current)
					{
					
						if (ShowLabels)
						{
							TextLayout layout = PositiveFont.GetTextLayout(MonsterRules[e.Type].Name);

							//Plugin.BgBrushAlt.DrawRectangle(x + IconSize*0.5f - layout.Metrics.Width*0.5f - IconSpacing - 1, y + IconSize - 1, layout.Metrics.Width + IconSpacing*2 + 2, layout.Metrics.Height + IconSpacing*2 + 2);
							//TextFont.DrawText(layout, x + IconSize*0.5f - layout.Metrics.Width*0.5f, y + IconSize + IconSpacing);
							//BackgroundBrush?.DrawRectangle(x + IconSize*0.5f - layout.Metrics.Width*0.5f - IconSpacing - 1, y - IconSpacing*2 - layout.Metrics.Height - 1, layout.Metrics.Width + IconSpacing*2 + 2, layout.Metrics.Height + IconSpacing*2 + 2);
							PositiveFont.DrawText(layout, x + IconSize*0.5f - layout.Metrics.Width*0.5f, y - IconSpacing - layout.Metrics.Height);
								
							//time elapsed
							layout = PositiveFont.GetTextLayout(((float)(Hud.Game.CurrentGameTick - e.StartTick)/60f).ToString("F1") + "s");
							PositiveFont.DrawText(layout, x + IconSize*0.5f - layout.Metrics.Width*0.5f, y + IconSize + IconSpacing);
						}

						var texture = MonsterRules[e.Type].Texture;
						if (texture is object)
						{
							texture.Draw(x, y, IconSize, IconSize, 1f);
							PositiveBrush.DrawRectangle(x, y, IconSize, IconSize);
							//Hud.Texture.DebuffFrameTexture.Draw(x, y, IconSize, IconSize, 1f);
							//Hud.Texture.DebuffFrameTexture.Draw(x, y, IconSize, IconSize + layout.Metrics.Height + IconSpacing*2, 1f);
						}

						x += IconSize + IconSpacing;
					}
					IconSizeMultiplier = tmp;
				}
				
				
			}
		}
		
		private void AddEvent(CCEvent e)
		{
			History.Add(e);
			
			if (History.Count > HistoryLimit)
				History.RemoveAt(0);
				
			//HistoryChanged = true;
		}
		
		/*private IEnumerable<CCEvent> GetEvent(uint id, CCType type)
		{
			return History.Where(e => {
				CCEvent cce = (CCEvent)e[0];
				return cce.IsPlayer && cce.Id == id && cce.Type == type && cce.FinishTick == -1;
			}).Select(e => (CCEvent)e[0]); //FirstOrDefault
			//return (CCEvent)activeEvent[0];
		}*/
		
		public void OnRegister(MenuPlugin plugin)
		{
			
			TextFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 255, 255, 255, false, false, true);
			
			//Label
			Label = /*new LabelRowDecorator(Hud,
				new LabelStringDecorator(Hud, "CC") {Font = TextFont},
				new LabelTextureDecorator(Hud, Hud.Texture.GetTexture("Debuff_General_Stun")) {ContentHeight = plugin.MenuHeight - 2} //DebugBrush1 = Hud.Render.CreateBrush(192, 255, 0, 0, 2, SharpDX.Direct2D1.DashStyle.Dash), DebugBrush2 = Hud.Render.CreateBrush(192, 0, 0, 255, 2, SharpDX.Direct2D1.DashStyle.Dash),
			) {SpacingTop = 2, SpacingBottom = 2};*/
			new LabelTextureDecorator(Hud, Hud.Texture.GetTexture("Debuff_General_Stun")) {TextureHeight = plugin.MenuHeight - 2};
				
			
			TableUI = new LabelTableDecorator(Hud, 
				new LabelRowDecorator(Hud,
					new LabelStringDecorator(Hud) {Font = TextFont, Alignment = HorizontalAlign.Right, SpacingLeft = 5, SpacingRight = 5},
					new LabelStringDecorator(Hud) {Font = TextFont, Alignment = HorizontalAlign.Left, SpacingLeft = 5, SpacingRight = 5},
					new LabelStringDecorator(Hud) {Font = TextFont, Alignment = HorizontalAlign.Left, SpacingLeft = 5, SpacingRight = 5},
					new LabelStringDecorator(Hud) {Font = TextFont, Alignment = HorizontalAlign.Left, SpacingLeft = 5, SpacingRight = 5}
				) {SpacingTop = 2, SpacingBottom = 2}
			) {
				BackgroundBrush = plugin.BgBrush,
				HoveredBrush = plugin.HighlightBrush,
				SpacingLeft = 10,
				SpacingRight = 10,
				//FillWidth = false, //true,
				OnFillRow = (row, index) => {
					if (index >= History.Count) //TimeToLevel.Length)
						return false;
					
					CCEvent cce = History[History.Count - 1 - index];
					if (Hud.Game.IsInTown)
					{
						if (index >= HistoryView)
							return false;
					}
					else if (cce.FinishTick > 0 && Hud.Game.CurrentGameTick - cce.FinishTick > HistoryViewTimeout*60)
					{
						row.Enabled = false;
						return true;
					}
						
					row.Enabled = true;
					row.BackgroundBrush = cce.FinishTick == -1 ? plugin.HighlightBrush : null;
					
					LabelStringDecorator timeUI = (LabelStringDecorator)row.Labels[0];
					LabelStringDecorator nameUI = (LabelStringDecorator)row.Labels[1];
					LabelStringDecorator statusUI = (LabelStringDecorator)row.Labels[2];
					LabelStringDecorator durationUI = (LabelStringDecorator)row.Labels[3];
					
					TimeSpan elapsed = Hud.Time.Now - cce.Timestamp;
					string time;
					if (elapsed.TotalSeconds < 60)
						time = elapsed.TotalSeconds.ToString("F0") + "с назад";
					else if (elapsed.TotalMinutes < 10)
						time = elapsed.TotalMinutes.ToString("F0") + "м назад";
					else
						time = cce.Timestamp.ToString("hh:mm tt");
					timeUI.StaticText = time;
					//timeUI.StaticText = cce.Timestamp.ToString("T");
					
					nameUI.StaticText = cce.Name;
					nameUI.Font = cce.Font;
					
					var rule = (cce.IsPlayer ? PlayerRules[cce.Type] : MonsterRules[cce.Type]);
					statusUI.StaticText = rule.Name;
					
					var duration = (cce.FinishTick > -1 ? (float)(cce.FinishTick - cce.StartTick) : (float)(cce.LastSeen - cce.StartTick)) / 60f;
					durationUI.StaticText = duration.ToString("F1") + "s";
					
					return true;
				}
			};
			
			//Menu
			Panel = new LabelColumnDecorator(Hud, 
				new LabelDelayedDecorator(Hud,
					new LabelAlignedDecorator(Hud, 
						new LabelStringDecorator(Hud, "СДЕРЖИВАНИЕ ТОЛПЫ") {Font = plugin.TitleFont, SpacingLeft = 10, SpacingRight = 10},
						plugin.CreateReset(this.Reset),
						plugin.CreatePin(this)
					)
				) {BackgroundBrush = plugin.BgBrush},
				TableUI
			);
			
			
			/*
			Menu.Add(new MenuTableDecorator() { 
				BackgroundBrush = plugin.BgBrush,
				HighlightBrush = plugin.HighlightBrush,
				Data = History,
				UpdateWhileVisible = (decorator) => {
					if (HistoryChanged)
					{
						List<object> view = History.Where(e => ((CCEvent)e).FinishTick == -1).OrderByDescending(e => ((CCEvent)e).StartTick).ToList();
						view.AddRange(History.Where(e => ((CCEvent)e).FinishTick > -1).OrderByDescending(e => ((CCEvent)e).StartTick).Take(HistoryViewLimit - view.Count));

						//((MenuTableDecorator)Menu.MenuList[1]).Data = view;
						((MenuTableDecorator)decorator).Data = view;
						
						HistoryChanged = false;
					}
					
					return true;
				},
				IsRowHighlighted = (row) => ((CCEvent)row).FinishTick == -1,
				AssignRow = (row, view) => {
					MenuStringDecorator nameUI = (MenuStringDecorator)view.Decorators[0];
					MenuStringDecorator statusUI = ((MenuStringDecorator)view.Decorators[1]);
					MenuStringDecorator durationUI = ((MenuStringDecorator)view.Decorators[2]);
					
					CCEvent cce = (CCEvent)row;
					nameUI.StaticText = cce.Name;
					
					var rule = (cce.IsPlayer ? PlayerRules[cce.Type] : MonsterRules[cce.Type]);
					statusUI.StaticText = rule.Name;
					
					var duration = (cce.FinishTick > -1 ? (float)(cce.FinishTick - cce.StartTick) : (float)(cce.LastSeen - cce.StartTick)) / 60f;
					durationUI.StaticText = duration.ToString("F1") + "s";
					
					return true;
				},
				ViewRows = new MenuRowDecorator(
					new MenuStringDecorator() {TextFont = TextFont, Alignment = HorizontalAlign.Right, SpacingLeft = 5, SpacingRight = 5, SpacingTop = 3, SpacingBottom = 3},
					new MenuStringDecorator() {TextFont = TextFont, Alignment = HorizontalAlign.Left, SpacingLeft = 5, SpacingRight = 5, SpacingTop = 3, SpacingBottom = 3},
					new MenuStringDecorator() {TextFont = TextFont, Alignment = HorizontalAlign.Left, SpacingLeft = 5, SpacingRight = 5, SpacingTop = 3, SpacingBottom = 3}
				)
			});*/
			
			//Plugin = plugin;
		}
		
		/*public void OnClick(IMenuAddon addon)
		{
			//Console.Beep(250, 200);
			int index = ((MenuTableDecorator)Menu.MenuList[0]).HoveredCellRow;
			if (index > -1)
			{
				Hud.Sound.Speak(index.ToString());
				IPlugin plugin = (IPlugin)Data[index][0];
				plugin.Enabled = !plugin.Enabled;
			}
		}*/
		
		public void Reset(ILabelDecorator label)
		{
			History.Clear();
		}
	}
}