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
	using Turbo.Plugins.Razor.Util; //Hud.Sno.GetExpToNextLevel

	public class MenuMoveSpeed : BasePlugin, IMenuAddon//, INewAreaHandler, ICustomizer, IInGameTopPainter /*, ILeftClickHandler, IRightClickHandler*/
	{
		//public bool HideDefaultPlugin { get; set; } = true;

		public IFont TextFont { get; set; }
		//public IFont FadedFont { get; set; }
		//public IFont EnabledFont { get; set; }
		//public IFont DisabledFont { get; set; }
		
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
		
		private Dictionary<string, BuffRule> GeneralSpeedRules;
		private Dictionary<HeroClass, Dictionary<string, BuffRule>> ClassSpeedRules;
		private List<string> SpeedStack = new List<string>();
		private uint CurrentHeroId;
		private float BaseSpeed;
		
		/*public class SpeedBuffRule {
			public 
		}*/
		
        public MenuMoveSpeed()
        {
            Enabled = true;
			Priority = 50;
			DockId = "BottomCenter";
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
        }
		
		/*public void Customize()
		{
			if (HideDefaultPlugin)
				Hud.TogglePlugin<ExperienceOverBarPlugin>(false);
		}
		
		public void OnNewArea(bool newGame, ISnoArea area)
		{
			
		}*/
		
		public void OnRegister(MenuPlugin plugin)
		{
			TextFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 225, 225, 225, false, false, 100, 0, 0, 0, true);
			//FadedFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 150, 150, 150, false, false, true);
			//EnabledFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 0, 255, 0, false, false, true);
			//DisabledFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 255, 0, 0, false, false, true); //170, 150, 120
			
			GeneralSpeedRules = new Dictionary<string, BuffRule>() {
				//Wreath of Lightning
				{"Венец молнии", new BuffRule(Hud.Sno.SnoPowers.WreathOfLightningSecondary.Sno) {IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.Unique_Gem_004_x1.Sno)}},
				//Boon of the Hoarder
				{"Дар стяжателя", new BuffRule(Hud.Sno.SnoPowers.BoonOfTheHoarderSecondary.Sno) {IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.Unique_Gem_014_x1.Sno)}},
				//Rechel's Ring
				{"Rechel's Ring", new BuffRule(Hud.Sno.SnoPowers.RechelsRingOfLarceny.Sno) {IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.Unique_Ring_104_x1.Sno)}},
				//Speed Pylon
				{"Пилон скорости", new BuffRule(Hud.Sno.SnoPowers.Generic_PagesBuffRunSpeed.Sno) {IconIndex = 0, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false}},
				//Fleeting Shrine 260348
				{"Fleeting Shrine", new BuffRule(Hud.Sno.SnoPowers.Generic_ShrineDesecratedHoarder.Sno) {MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false}},
				//Echoing Fury
				{"Echoing Fury", new BuffRule(Hud.Sno.SnoPowers.EchoingFury.Sno) {IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true}},
				//Krelm's Buff Belt
				{"Krelm's Buff Belt", new BuffRule(Hud.Sno.SnoPowers.KrelmsBuffBelt.Sno) {IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.Unique_Belt_Set_02_x1.Sno)}},
				//Warzechian Armguards
				{"Warzechian Armguards", new BuffRule(Hud.Sno.SnoPowers.WarzechianArmguards.Sno) {IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.Unique_Bracer_101_x1.Sno)}},
				//Barbarian - Sprint
				{"Спринт", new BuffRule(Hud.Sno.SnoPowers.Barbarian_Sprint.Sno) {MinimumIconCount = 1}},
				//Barbarian - Chilanik's Chain - War Cry
				{"Цепь Чиланика", new BuffRule(Hud.Sno.SnoPowers.ChilaniksChain.Sno) {IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false, UseLegendaryItemTexture = Hud.Inventory.GetSnoItem(Hud.Sno.SnoItems.Unique_BarbBelt_101_x1.Sno)}},
				//Crusader - Laws of Hope - Wings of Angels
				{"Законы надежды - Крылья ангелов", new BuffRule(342299) {IconIndex = 12, MinimumIconCount = 1}},
				//Monk - Breath of Heaven - Zephyr
				{"Дыхание небес - Зефир", new BuffRule(Hud.Sno.SnoPowers.Monk_BreathOfHeaven.Sno) {IconIndex = 2, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false}},
				//Monk - Mantra of Conviction - Annihilation
				{"Мантра убеждения — Уничтожение", new BuffRule(375089) {IconIndex = 8, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = false}},
			};
			
			ClassSpeedRules = new Dictionary<HeroClass, Dictionary<string, BuffRule>>() {
				{HeroClass.Barbarian, new Dictionary<string, BuffRule>() {
					//Barbarian - Pound of Flesh (Passive)
					{"Pound of Flesh", new BuffRule(Hud.Sno.SnoPowers.Barbarian_Passive_PoundOfFlesh.Sno) {MinimumIconCount = 1}},
					//Barbarian - Wrath of the Berserker
					{"Гнев Берсерка", new BuffRule(Hud.Sno.SnoPowers.Barbarian_WrathOfTheBerserker.Sno) {MinimumIconCount = 1}},
					//Barbarian - Ignore Pain - Bravado
					//{"Ignore Pain - Bravado", new BuffRule(Hud.Sno.SnoPowers.Barbarian_IgnorePain.Sno) {MinimumIconCount = 1}},
					//Barbarian - Battle Rage - Ferocity
					//Barbarian - Sprint
				}},
				{HeroClass.Crusader, new Dictionary<string, BuffRule>() {
					//Crusader - Steed Charge
					//Crusader - Justice - Sword of Justice
					//Crusader - Iron Skin - Flash
				}},
				{HeroClass.DemonHunter, new Dictionary<string, BuffRule>() {
					//Demon Hunter - Gears of Dreadlands (4pc) - Momentum stacks
					//Demon Hunter - Tactical Advantage (Passive)
					//Demon Hunter - Hot Pursuit (Passive)
					//Demon Hunter - Shadow Power - Shadow Glide
					//Demon Hunter - Smoke Screen
					//Demon Hunter - Companion - Ferret Companion
				}},
				{HeroClass.Monk, new Dictionary<string, BuffRule>() {
					//Monk - Patterns of Justice (2pc) - Sweeping Wind
					{"Patterns of Justice (2pc) - Sweeping Wind", new BuffRule(483662) {IconIndex = 1, MinimumIconCount = 1, ShowTimeLeft = true, ShowStacks = true}},
					//Monk - Dashing Strike - Way of the Falling Star
					
					//Monk - Tempest Rush - Tailwind
					//Monk - Way of the Hundred Fists - Blazing Fists
					//Monk - Fleet Footed (Passive)
					{"Fleet Footed", new BuffRule(Hud.Sno.SnoPowers.Monk_Passive_FleetFooted.Sno) {MinimumIconCount = 1}},
				}},
				{HeroClass.Necromancer, new Dictionary<string, BuffRule>() {
					//Necromancer - Steuart's Greaves - Blood Rush
					//Necromancer - Fueled by Death (Passive)
					//Necromancer - Lost Time + cold attack
				}},
				{HeroClass.WitchDoctor, new Dictionary<string, BuffRule>() {
					//Witch Doctor - Fierce Loyalty (Passive)
					//Witch Doctor - Spirit Walk
					//Witch Doctor - Horrify - Stalker
					//Witch Doctor - Big Bad Voodoo
					//Witch Doctor - Hex - Angry Chicken
					//Witch Doctor - Manajuma's Way (2pc) + Angry Chicken
				}},
				{HeroClass.Wizard, new Dictionary<string, BuffRule>() {
					//Wizard - Illusionist (Passive) + illusion skill
					{"Иллюзионист", new BuffRule(Hud.Sno.SnoPowers.Wizard_Passive_Illusionist.Sno) {IconIndex = 1, MinimumIconCount = 1}},
					//Wizard - Diamond Skin - Sleek Shell
					{"Алмазная кожа - Гладкая оболочка", new BuffRule(Hud.Sno.SnoPowers.Wizard_DiamondSkin.Sno) {IconIndex = 2, MinimumIconCount = 1}},
				}},

			};			
			
			
			
			
			//Label
			Label = new LabelRowDecorator(Hud,
				new LabelStringDecorator(Hud, () => Hud.Game.Me.Stats.MoveSpeed.ToString("F0") + "%") {Font = TextFont},
				new LabelStringDecorator(Hud, "🦶") {Font = TextFont}
			);
			
			Panel = new LabelColumnDecorator(Hud, 
				new LabelDelayedDecorator(Hud,
					new LabelAlignedDecorator(Hud, 
						new LabelStringDecorator(Hud, "БОНУС СКОРОСТИ ДВИЖЕНИЯ") {Font = plugin.TitleFont, SpacingLeft = 15, SpacingRight = 15},
						plugin.CreatePin(this)
					)
				) {BackgroundBrush = plugin.BgBrush},
				new LabelTableDecorator(Hud,
					new LabelRowDecorator(Hud,
						new LabelTextureDecorator(Hud),
						new LabelStringDecorator(Hud) {Font = TextFont, Alignment = HorizontalAlign.Left}
					) {SpacingTop = 2, SpacingBottom = 2}
				) {
					BackgroundBrush = plugin.BgBrush,
					HoveredBrush = plugin.HighlightBrush,
					OnBeforeRender = (label) => {
						//check which run speed buffs are currently active and build the speed stack
						SpeedStack.Clear();
						
						foreach (KeyValuePair<string, BuffRule> rule in GeneralSpeedRules)
						{
							if (rule.Value.IconIndex.HasValue)
							{
								if (Hud.Game.Me.Powers.BuffIsActive(rule.Value.PowerSno, rule.Value.IconIndex.Value))
								{
									SpeedStack.Add(rule.Key);
								}
							}
							else if (Hud.Game.Me.Powers.BuffIsActive(rule.Value.PowerSno))
							{
								SpeedStack.Add(rule.Key);
							}
						}
						
						if (ClassSpeedRules.ContainsKey(Hud.Game.Me.HeroClassDefinition.HeroClass))
						{
							foreach (KeyValuePair<string, BuffRule> rule in ClassSpeedRules[Hud.Game.Me.HeroClassDefinition.HeroClass])
							{
								if (rule.Value.IconIndex.HasValue)
								{
									if (Hud.Game.Me.Powers.BuffIsActive(rule.Value.PowerSno, rule.Value.IconIndex.Value))
									{
										SpeedStack.Add(rule.Key);
									}
								}
								else if (Hud.Game.Me.Powers.BuffIsActive(rule.Value.PowerSno))
								{
									SpeedStack.Add(rule.Key);
								}
							}
						}
						
						if (SpeedStack.Count == 0)
							BaseSpeed = Hud.Game.Me.Stats.MoveSpeed;
						
						return true;
					},
					OnFillRow = (row, index) => {
						if (index >= SpeedStack.Count)
							return false;
						
						var label = (LabelStringDecorator)row.Labels[1];
						label.StaticText = SpeedStack[index];
						
						return true;
					}
				}
			);
		}
	}
}