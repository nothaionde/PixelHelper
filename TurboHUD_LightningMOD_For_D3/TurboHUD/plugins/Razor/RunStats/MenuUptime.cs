/*

Tracks the uptime of definable statuses

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

	public class MenuUptime : BasePlugin, IMenuAddon, IAfterCollectHandler /*, ILeftClickHandler, IInGameTopPainter, ILeftClickHandler, IRightClickHandler*/
	{
		public List<UptimeRule> Watching { get; set; } = new List<UptimeRule>(); //UptimeRule
		public bool IsInRift { get; private set; }
		public bool IsInCombat { get; private set; }
		public string TextUptime { get; set; } = " uptime";
		public int OculusValidDistanceFromTarget { get; set; } = 40; //skeletal mage reach is about 40yds, oculus radius is about 10yds
		
		public class UptimeRule
		{
			//rules
			public Func<UptimeRule, bool> IsRelevant { get; set; } //should this tracked stat be shown for the current hero?
			public Func<UptimeRule, bool> IsWatching { get; set; } //what is the context for the uptime? (e.g. in a rift + in combat)
			public Func<UptimeRule, bool> IsUp { get; set; } //is this stat being tracked active?
			public string Description { get; set; }

			//track
			public IWatch Uptime { get; set; } //duration for which IsUp returns true
			public IWatch TotalTime { get; set; } //duration for which IsWatching returns true

			//display
			public IBrush BgBrush { get; set; } //optional
			public IFont Font { get; set; } //optional
			public ITexture IconTexture { get; set; } = null; //optional
			public int IconWidth { get; set; } = -1; //optional override
			public int IconHeight { get; set; } = -1; //optional override

			public bool IsShown { get; set; } //cache of IsRelevant, let the plugin set this
			
			public UptimeRule(IController hud)
			{
				Uptime = hud.Time.CreateWatch();
				TotalTime = hud.Time.CreateWatch();
			}
			
			public double Percent()
			{
				return (TotalTime.ElapsedMilliseconds > 0 ?
					((double)Uptime.ElapsedMilliseconds/(double)TotalTime.ElapsedMilliseconds) : 0);
			}
		}
		
		public string Id { get; set; }
		public int Priority { get; set; } //the priority on the dock to show this addon (smaller to the left, higher to the right)
		public string DockId { get; set; }
		public string Config { get; set; }

		public ILabelDecorator Label { get; set; }
		public ILabelDecorator LabelHint { get; set; }
		public float LabelSize { get; set; }
		public ILabelDecorator Panel { get; set; }
		
		public IFont TextFont { get; set; }
		public IFont FadedFont { get; set; }
		public IFont EnabledFont { get; set; }
		public IFont DisabledFont { get; set; }
		
		private List<object[]> Data;
		private LabelTableDecorator TableUI;
		
        public MenuUptime()
        {
            Enabled = true;
			Priority = 40;
			DockId = "BottomLeft";
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
        }
		
		public void OnRegister(MenuPlugin plugin)
		{
			
			TextFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 255, 255, 255, false, false, true);
			FadedFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 150, 150, 150, false, false, true);
			EnabledFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 0, 255, 0, false, false, true);
			DisabledFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 255, 0, 0, false, false, true); //170, 150, 120
			
			//track Hexing Pants uptime
			Watching.Add(new UptimeRule(Hud)
			{
				IconTexture = Hud.Texture.GetItemTexture(Hud.Sno.SnoItems.Unique_Pants_101_x1),
				IconWidth = 24,
				IconHeight = 32,
				IsRelevant = (rule) => Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.HexingPantsOfMrYan.Sno), //HasCubedItem(Hud.Sno.SnoItems.Unique_Pants_101_x1.Sno) || Hud.Game.Items.Any(x => x.Location == ItemLocation.Legs && x.SnoItem.Sno == Hud.Sno.SnoItems.Unique_Pants_101_x1.Sno), //(Hud.Game.Me.CubeSnoItem2?.Sno == Hud.Sno.SnoItems.Unique_Pants_101_x1.Sno) || Hud.Game.Items.Any(x => x.Location == ItemLocation.Legs && x.SnoItem.Sno == Hud.Sno.SnoItems.Unique_Pants_101_x1.Sno), // Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.HexingPantsOfMrYan.Sno), //hexing pants is equipped/cubed
				IsUp = (rule) => IsInRift && IsInCombat && Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.HexingPantsOfMrYan.Sno, 2),
				IsWatching = (rule) => IsInRift && IsInCombat,
				Description = Hud.Sno.SnoItems.Unique_Pants_101_x1.NameLocalized, //"Hexing Pants" + TextUptime,
				Font = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 135, 135, 135, false, false, true),
			});
			
			//track Dayntee's buff uptime
			Watching.Add(new UptimeRule(Hud)
			{
				IconTexture = Hud.Texture.GetItemTexture(Hud.Sno.SnoItems.P61_Unique_Belt_01),
				IsRelevant = (rule) => Hud.Game.Me.HeroClassDefinition.HeroClass == HeroClass.Necromancer && Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.DaynteesBinding.Sno), //(Hud.Game.Me.CubeSnoItem2?.Sno == Hud.Sno.SnoItems.P61_Unique_Belt_01.Sno) || Hud.Game.Items.Any(x => x.Location == ItemLocation.Waist && x.SnoItem.Sno == Hud.Sno.SnoItems.P61_Unique_Belt_01.Sno), //dayntee's is cubed or equipped
				IsUp = (rule) => IsInRift && IsInCombat && Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.DaynteesBinding.Sno, 1),
				IsWatching = (rule) => IsInRift && IsInCombat,
				Description = Hud.Sno.SnoItems.P61_Unique_Belt_01.NameLocalized, //"Dayntee's Binding " + TextUptime,
				//BgBrush = Hud.Render.CreateBrush(200, 81, 78, 72, 0),
				Font = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 255, 251, 209, false, false, true),
			});
			
			//track Simulacrum uptime
			Watching.Add(new UptimeRule(Hud)
			{
				IconTexture = Hud.Texture.GetTexture(Hud.Sno.SnoPowers.Necromancer_Simulacrum.NormalIconTextureId),
				IsRelevant = (rule) => Hud.Game.Me.Powers.UsedSkills.Any(s => s.SnoPower.Sno == Hud.Sno.SnoPowers.Necromancer_Simulacrum.Sno), //sim skill is on the action bar
				IsUp = (rule) => IsInRift && IsInCombat && Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Necromancer_Simulacrum.Sno, 1),
				IsWatching = (rule) => IsInRift && IsInCombat,
				Description = Hud.Sno.SnoPowers.Necromancer_Simulacrum.NameLocalized, // + " " + TextUptime,
				//BgBrush = Hud.Render.CreateBrush(200, 107, 3, 3, 0),
				Font = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 255, 60, 60, false, false, true),
			});
			
			//track IP uptime
			Watching.Add(new UptimeRule(Hud)
			{
				IconTexture = Hud.Texture.GetTexture(Hud.Sno.SnoPowers.Barbarian_IgnorePain.NormalIconTextureId),
				IsRelevant = (rule) => rule.TotalTime.ElapsedMilliseconds > 0 || Hud.Game.Players.Any(p => p.HeroClassDefinition.HeroClass == HeroClass.Barbarian && p.Powers.UsedSkills.Any(x => x.SnoPower.Sno == Hud.Sno.SnoPowers.Barbarian_IgnorePain.Sno)), //someone in the party has IP equipped
				IsUp = (rule) => 
					IsInRift && 
					IsInCombat && 
					Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Barbarian_IgnorePain.Sno) && 
					!Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_ActorInvulBuff.Sno) && //not in stasis
					!Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_PagesBuffInvulnerable.Sno), //no Shield Pylon
				IsWatching = (rule) => 
					IsInRift && 
					IsInCombat && 
					!Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_ActorInvulBuff.Sno) &&  //not in stasis
					!Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_PagesBuffInvulnerable.Sno), //no Shield Pylon
				Description = Hud.Sno.SnoPowers.Barbarian_IgnorePain.NameLocalized, // + " " + TextUptime,
				//BgBrush = Hud.Render.CreateBrush(200, 9, 68, 34, 0),
				Font = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 18, 234, 110, false, false, true),
			});

			//track Oculus buff uptime
			Watching.Add(new UptimeRule(Hud)
			{
				IconTexture = Hud.Texture.GetItemTexture(Hud.Sno.SnoItems.Unique_Ring_017_p4),
				IconWidth = 29,
				IconHeight = 29,
				IsRelevant = (rule) => rule.TotalTime.ElapsedMilliseconds > 0 || Hud.Game.Players.Any(p => p.Powers.BuffIsActive(Hud.Sno.SnoPowers.OculusRing.Sno)), //someone in the party (or active follower) has oculus ring equipped/cubed
				IsUp = (rule) => 
					IsInRift && 
					IsInCombat && 
					Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.OculusRing.Sno, 2) &&
					Hud.Game.AliveMonsters.Any(m => m.IsElite && m.Rarity != ActorRarity.RareMinion && m.CentralXyDistanceToMe < OculusValidDistanceFromTarget), //player.FloorCoordinate.XYZDistanceTo(m.FloorCoordinate)
				IsWatching = (rule) => {
					if (!IsInRift) return false;
					if (!IsInCombat) return false;					
					var circles = Hud.Game.Actors.Where(a => a.SnoActor.Sno == ActorSnoEnum._generic_proxy && a.GetAttributeValueAsInt(Hud.Sno.Attributes.Power_Buff_1_Visual_Effect_None, Hud.Sno.SnoPowers.OculusRing.Sno) == 1);
					return circles.Any(c => Hud.Game.AliveMonsters.Any(m => m.IsElite && m.Rarity != ActorRarity.RareMinion && c.FloorCoordinate.XYZDistanceTo(m.FloorCoordinate) < OculusValidDistanceFromTarget+10));
				},
				Description = Hud.Sno.SnoItems.Unique_Ring_017_p4.NameLocalized, // + TextUptime,
				//BgBrush = Hud.Render.CreateBrush(200, 76, 79, 7, 0),
				Font = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 158, 255, 100, false, false, true), //255, 253, 229
			});
			
			//track Flying Dragon buff uptime
			Watching.Add(new UptimeRule(Hud)
			{
				IconTexture = Hud.Texture.GetItemTexture(Hud.Sno.SnoItems.Unique_CombatStaff_2H_009_x1),
				IsRelevant = (rule) => (Hud.Game.Me.CubeSnoItem1?.Sno == Hud.Sno.SnoItems.Unique_CombatStaff_2H_009_x1.Sno) || Hud.Game.Items.Any(x => x.Location == ItemLocation.RightHand && x.SnoItem.Sno == Hud.Sno.SnoItems.Unique_CombatStaff_2H_009_x1.Sno), //flying dragon is cubed or equipped
				IsUp = (rule) => IsInRift && IsInCombat && Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.FlyingDragon.Sno, 1),
				IsWatching = (rule) => IsInRift && IsInCombat,
				Description = Hud.Sno.SnoItems.Unique_CombatStaff_2H_009_x1.NameLocalized, // + TextUptime,
				//BgBrush = Hud.Render.CreateBrush(200, 81, 78, 72, 0),
				Font = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 255, 251, 209, false, false, true),
			});
			
			//track Epiphany buff uptime if not full uptime
			Watching.Add(new UptimeRule(Hud)
			{
				IconTexture = Hud.Texture.GetTexture(Hud.Sno.SnoPowers.Monk_Epiphany.NormalIconTextureId),
				IsRelevant = (rule) => Hud.Game.Me.HeroClassDefinition.HeroClass == HeroClass.Monk && Hud.Game.Me.Powers.UsedSkills.Any(s => s.SnoPower.Sno == Hud.Sno.SnoPowers.Monk_Epiphany.Sno), //skill is equipped
				IsUp = (rule) => IsInRift && IsInCombat && Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Monk_Epiphany.Sno),
				IsWatching = (rule) => IsInRift && IsInCombat,
				Description = Hud.Sno.SnoPowers.Monk_Epiphany.NameLocalized, // + TextUptime,
				//BgBrush = Hud.Render.CreateBrush(200, 81, 78, 72, 0),
				Font = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 255, 200, 30, false, false, true),
			});
			
			//track Akarat's Champion buff uptime if not full uptime
			Watching.Add(new UptimeRule(Hud)
			{
				IconTexture = Hud.Texture.GetTexture(Hud.Sno.SnoPowers.Crusader_AkaratsChampion.NormalIconTextureId),
				IsRelevant = (rule) => Hud.Game.Me.HeroClassDefinition.HeroClass == HeroClass.Crusader && Hud.Game.Me.Powers.UsedSkills.Any(s => s.SnoPower.Sno == Hud.Sno.SnoPowers.Crusader_AkaratsChampion.Sno), //skill is equipped
				IsUp = (rule) => IsInRift && IsInCombat && Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Crusader_AkaratsChampion.Sno, 1),
				IsWatching = (rule) => IsInRift && IsInCombat,
				Description = Hud.Sno.SnoPowers.Crusader_AkaratsChampion.NameLocalized, // + TextUptime,
				//BgBrush = Hud.Render.CreateBrush(200, 81, 78, 72, 0),
				Font = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 255, 200, 30, false, false, true),
			});
			
			//track Wrath of the Berserker buff uptime if not full uptime
			Watching.Add(new UptimeRule(Hud)
			{
				IconTexture = Hud.Texture.GetTexture(Hud.Sno.SnoPowers.Barbarian_WrathOfTheBerserker.NormalIconTextureId),
				IsRelevant = (rule) => Hud.Game.Me.HeroClassDefinition.HeroClass == HeroClass.Barbarian && Hud.Game.Me.Powers.UsedSkills.Any(s => s.SnoPower.Sno == Hud.Sno.SnoPowers.Barbarian_WrathOfTheBerserker.Sno), //skill is equipped
				IsUp = (rule) => IsInRift && IsInCombat && Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.Barbarian_WrathOfTheBerserker.Sno),
				IsWatching = (rule) => IsInRift && IsInCombat,
				Description = Hud.Sno.SnoPowers.Barbarian_WrathOfTheBerserker.NameLocalized, // + TextUptime,
				//BgBrush = Hud.Render.CreateBrush(200, 81, 78, 72, 0),
				Font = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 255, 200, 30, false, false, true),
			});
			
			//track Big Bad Voodoo buff uptime
			Watching.Add(new UptimeRule(Hud)
			{
				IconTexture = Hud.Texture.GetTexture(Hud.Sno.SnoPowers.WitchDoctor_BigBadVoodoo.NormalIconTextureId),
				IsRelevant = (rule) => Hud.Game.Me.HeroClassDefinition.HeroClass == HeroClass.WitchDoctor && Hud.Game.Me.Powers.UsedSkills.Any(s => s.SnoPower.Sno == Hud.Sno.SnoPowers.WitchDoctor_BigBadVoodoo.Sno), //skill is equipped
				IsUp = (rule) => IsInRift && IsInCombat && Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.WitchDoctor_BigBadVoodoo.Sno, 4),
				IsWatching = (rule) => IsInRift && IsInCombat,
				Description = Hud.Sno.SnoPowers.WitchDoctor_BigBadVoodoo.NameLocalized, // + TextUptime,
				//BgBrush = Hud.Render.CreateBrush(200, 81, 78, 72, 0),
				Font = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 255, 200, 30, false, false, true),
			});
			
            //track Focus + Restraint uptime
            Watching.Add(new UptimeRule(Hud)
            {
				IconTexture = Hud.Texture.GetItemTexture(Hud.Sno.SnoItems.Unique_Ring_Set_001_x1),
				IconWidth = 29,
				IconHeight = 29,
                IsRelevant = (rule) => Hud.Game.Items.Any(x => (x.Location == ItemLocation.LeftRing || x.Location == ItemLocation.RightRing) && x.SnoItem.Sno == Hud.Sno.SnoItems.Unique_Ring_Set_001_x1.Sno) && Hud.Game.Items.Any(x => (x.Location == ItemLocation.LeftRing || x.Location == ItemLocation.RightRing) && x.SnoItem.Sno == Hud.Sno.SnoItems.Unique_Ring_Set_002_x1.Sno), // Unique_Ring_Set_001_x1 (Focus) - Unique_Ring_Set_002_x1 (Restraint)
                IsUp = (rule) => IsInRift && IsInCombat && IsInCombat && Hud.Game.Me.Powers.BuffIsActive(359583, 2) && Hud.Game.Me.Powers.BuffIsActive(359583, 1),
                IsWatching = (rule) => IsInRift && IsInCombat,
                Description = Hud.Sno.SnoItems.Unique_Ring_Set_001_x1.NameLocalized + " + " + Hud.Sno.SnoItems.Unique_Ring_Set_002_x1.NameLocalized, // + TextUptime,
                Font = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 255, 120, 20, false, false, true),
            });

            //track Squirt = 10 stacks uptime
            Watching.Add(new UptimeRule(Hud)
            {
				IconTexture = Hud.Texture.GetItemTexture(Hud.Sno.SnoItems.P66_Unique_Amulet_010),
				IconWidth = 22,
				IconHeight = 22,
                IsRelevant = (rule) => Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.SquirtsNecklace.Sno), //(Hud.Game.Me.CubeSnoItem3?.Sno == Hud.Sno.SnoItems.P66_Unique_Amulet_010.Sno || Hud.Game.Items.Any(x => x.Location == ItemLocation.Neck && x.SnoItem.Sno == Hud.Sno.SnoItems.P66_Unique_Amulet_010.Sno)), // Hud.Sno.SnoItems.Unique_Amulet_010_x1.Sno - 1187653737
                IsUp = (rule) => IsInRift && IsInCombat && Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.SquirtsNecklace.Sno, 5) && Hud.Game.Me.Powers.GetBuff(Hud.Sno.SnoPowers.SquirtsNecklace.Sno)?.IconCounts[5] == 10,
                IsWatching = (rule) => IsInRift && IsInCombat,
                Description = Hud.Sno.SnoItems.P66_Unique_Amulet_010.NameLocalized, // + TextUptime,
                Font = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 200, 200, 200, false, false, true),
            });
			
			//track Vengeance uptime if not full uptime
			
			//Label
			Label = new LabelStringDecorator(Hud, "🕓") {Font = TextFont};
			LabelHint = plugin.CreateHint("Uptime Tracker");
			
			//Menu
			TableUI = new LabelTableDecorator(Hud, 
				new LabelRowDecorator(Hud,
					//buff icon
					new LabelTextureDecorator(Hud) {Alignment = HorizontalAlign.Center, SpacingLeft = 5, SpacingRight = 5},
					//name
					new LabelStringDecorator(Hud) {Hint = plugin.CreateHint("Название Статуса"), Font = TextFont, Alignment = HorizontalAlign.Left, SpacingLeft = 5, SpacingRight = 5,},
					//time active
					new LabelStringDecorator(Hud) {Hint = plugin.CreateHint("Активное Время"), Font = TextFont, Alignment = HorizontalAlign.Left, SpacingLeft = 5, SpacingRight = 5,},
					//time watched
					new LabelStringDecorator(Hud) {Hint = plugin.CreateHint("Общее Время Работы"), Font = TextFont, Alignment = HorizontalAlign.Left, SpacingLeft = 5, SpacingRight = 5,},
					//percentage uptime
					new LabelStringDecorator(Hud) {Hint = plugin.CreateHint("% Время безотказной работы"), Font = TextFont, Alignment = HorizontalAlign.Left, SpacingLeft = 5, SpacingRight = 5,},
					//reset specific rule
					plugin.CreateReset(this.ResetRule)
				) {SpacingTop = 2, SpacingBottom = 2}
			) {
				BackgroundBrush = plugin.BgBrush,
				HoveredBrush = plugin.HighlightBrush,
				SpacingLeft = 10, 
				SpacingRight = 10,
				//Hint = new LabelStringDecorator(Hud, "2tooltip!") {Font = TextFont},
				//OnClick = (lbl) => Hud.Sound.Speak("2"),
				FillWidth = false, //true,
				OnFillRow = (row, index) => {
					if (index >= Watching.Count)
						return false;
							
					UptimeRule rule = Watching[index];
					if (!rule.IsShown)
						row.Enabled = false;
					else
					{
						row.Enabled = true;
					
						//icon
						LabelTextureDecorator iconUI = (LabelTextureDecorator)row.Labels[0];
						iconUI.Texture = rule.IconTexture;
						iconUI.TextureWidth = (rule.IconWidth > 0 ? rule.IconWidth : plugin.MenuHeight - 2);
						iconUI.TextureHeight = (rule.IconHeight > 0 ? rule.IconHeight : plugin.MenuHeight - 2);
						iconUI.ContentHeight = plugin.MenuHeight;
						
						//name
						LabelStringDecorator nameUI = (LabelStringDecorator)row.Labels[1];
						nameUI.StaticText = rule.Description;
						nameUI.Font = rule.Font;
					
						//time up
						LabelStringDecorator uptimeUI = (LabelStringDecorator)row.Labels[2];
						uptimeUI.StaticText = ValueToString(rule.Uptime.ElapsedMilliseconds * TimeSpan.TicksPerMillisecond, ValueFormat.LongTime);
						uptimeUI.Font = rule.Font;
						
						//time watched
						LabelStringDecorator totaltimeUI = (LabelStringDecorator)row.Labels[3];
						totaltimeUI.StaticText = ValueToString(rule.TotalTime.ElapsedMilliseconds * TimeSpan.TicksPerMillisecond, ValueFormat.LongTime);
						totaltimeUI.Font = rule.Font;

						//percentage uptime
						LabelStringDecorator percentUI = (LabelStringDecorator)row.Labels[4];
						var pct = rule.Percent() * 100;
						percentUI.StaticText = pct.ToString("0.##") + "%";
						percentUI.Font = rule.Font;
					}
					
					return true;
				}
			};
			
			Panel = new LabelColumnDecorator(Hud, 
				new LabelDelayedDecorator(Hud,
					new LabelAlignedDecorator(Hud, 
						new LabelStringDecorator(Hud, "ВРЕМЯ БЕЗОТКАЗНОЙ РАБОТЫ") {Font = plugin.TitleFont, SpacingLeft = 10, SpacingRight = 10},
						plugin.CreateReset(this.Reset),
						plugin.CreatePin(this)
					)
				) {BackgroundBrush = plugin.BgBrush},
				TableUI
			);
		}
		
		public void ResetRule(ILabelDecorator label)
		{
			if (TableUI.HoveredRow > -1 && TableUI.HoveredRow < Watching.Count) //&& Table.HoveredRow < Addons.Count)
			{
				UptimeRule rule = Watching[TableUI.HoveredRow];
				rule.TotalTime.Reset();
				rule.Uptime.Reset();
				//Console.Beep(150, 50);
			}
		}
		
		public void Reset(ILabelDecorator label)
		{
			foreach (UptimeRule rule in Watching)
			{
				rule.TotalTime.Reset();
				rule.Uptime.Reset();
			}
			//Console.Beep(150, 50);
		}
		
		public void AfterCollect()
		{
			if (!Hud.Game.IsInGame)
				return;
				
			if (Label is object)
			{
				IsInRift = (Hud.Game.SpecialArea == SpecialArea.Rift) || (Hud.Game.SpecialArea == SpecialArea.GreaterRift);
				IsInCombat = Hud.Game.Me.InCombat;
				
				//calculate uptimes
				bool isAnyRelevant = false;
				foreach (UptimeRule rule in Watching)
				{
					if (rule.IsRelevant(rule))
					{
						isAnyRelevant = true;
						rule.IsShown = true;
						
						if (rule.IsWatching(rule))
						{
							if (!rule.TotalTime.IsRunning)
								rule.TotalTime.Start();
							
							if (rule.IsUp(rule))
							{
								if (!rule.Uptime.IsRunning)
									rule.Uptime.Start();
							}
							else if (rule.Uptime.IsRunning)
								rule.Uptime.Stop();
						}
						else
						{
							if (rule.Uptime.IsRunning)
								rule.Uptime.Stop();
							if (rule.TotalTime.IsRunning)
								rule.TotalTime.Stop();
						}
					}
					else
					{
						rule.IsShown = false;
						
						if (rule.Uptime.IsRunning)
							rule.Uptime.Stop();
						if (rule.TotalTime.IsRunning)
							rule.TotalTime.Stop();
					}
				}
				
				Label.Enabled = isAnyRelevant;
			}
		}
		
		public bool HasCubedItem(IPlayer player, uint sno) //uint = item sno
		{	
			return (player.CubeSnoItem1?.Sno == sno) || (player.CubeSnoItem2?.Sno == sno) || (player.CubeSnoItem3?.Sno == sno) || (player.CubeSnoItem4?.Sno == sno);
		}
	}
}