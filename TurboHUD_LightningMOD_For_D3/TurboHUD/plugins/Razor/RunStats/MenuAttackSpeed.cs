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

	public class MenuAttackSpeed : BasePlugin, IMenuAddon//, INewAreaHandler, ICustomizer, IInGameTopPainter /*, ILeftClickHandler, IRightClickHandler*/
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
		
		private LabelStringDecorator OffhandUI;
		private LabelStringDecorator MainhandUI;
		private LabelRowDecorator PainEnhancerUI;
		
        public MenuAttackSpeed()
        {
            Enabled = true;
			Priority = 40;
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
			TextFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 255, 255, 160, false, false, 100, 0, 0, 0, true);
			//FadedFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 150, 150, 150, false, false, true);
			//EnabledFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 0, 255, 0, false, false, true);
			//DisabledFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 255, 0, 0, false, false, true); //170, 150, 120
			
			MainhandUI = new LabelStringDecorator(Hud, () => Hud.Game.Me.Offense.AttackSpeedMainHand.ToString("0.##")) {/*Hint = plugin.CreateHint("Mainhand Attacks Per Second"), */Font = TextFont, Alignment = HorizontalAlign.Left};
			OffhandUI = new LabelStringDecorator(Hud, () => Hud.Game.Me.Offense.AttackSpeedOffHand.ToString("0.##")) {/*Hint = plugin.CreateHint("Offhand Attacks Per Second"), */Font = TextFont, Alignment = HorizontalAlign.Left};
			PainEnhancerUI = new LabelRowDecorator(Hud,
				new LabelStringDecorator(Hud, "Pain Enhancer") {Font = TextFont, Alignment = HorizontalAlign.Right, SpacingLeft = 10, SpacingRight = 10},
				new LabelStringDecorator(Hud, () => Hud.Game.AliveMonsters.Count(m => m.Attackable && m.NormalizedXyDistanceToMe <= 20 && m.DotDpsApplied > 0 && m.GetAttributeValueAsInt(Hud.Sno.Attributes.Bleeding, uint.MaxValue) == 1).ToString() + " stacks") {Font = TextFont, Alignment = HorizontalAlign.Left, SpacingLeft = 10, SpacingRight = 10}
			);
			
			//Label
			Label = new LabelRowDecorator(Hud,
				MainhandUI,
				new LabelStringDecorator(Hud, "«»") {Font = Hud.Render.CreateFont("tahoma", plugin.FontSize+2, 255, 255, 255, 160, false, false, 100, 0, 0, 0, true), SpacingLeft = 2, SpacingRight = 2}, //TextFont}, //🗲
				new LabelTextureDecorator(Hud, Hud.Texture.GetItemTexture(Hud.Sno.SnoItems.Unique_Gem_006_x1)) {TextureHeight = plugin.MenuHeight},
				OffhandUI,
				//hidden icon at the end for vertical
				new LabelStringDecorator(Hud, "«»") {Enabled = false, Font = Hud.Render.CreateFont("tahoma", plugin.FontSize+2, 255, 255, 255, 160, false, false, 100, 0, 0, 0, true), SpacingLeft = 2, SpacingRight = 2} //TextFont}, //🗲
				//new LabelTextureDecorator(Hud, Hud.Texture.GetItemTexture(Hud.Sno.SnoItems.Unique_Gem_006_x1)) {Enabled = false, TextureHeight = plugin.MenuHeight}
			) {
				OnBeforeRender = (label) => {
					OffhandUI.Enabled = Hud.Game.Me.Offense.AttackSpeedOffHand != 0;
					
					//if the first element is hidden, then a vertical dock probably trying to override its visibility
					if (((LabelRowDecorator)Label).Labels[0].Enabled)
					{
						if (Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.PainEnhancerPrimary.Sno)) //pain enhancer equipped 403462
						{
							((LabelRowDecorator)Label).Labels[1].Enabled = false;
							((LabelRowDecorator)Label).Labels[2].Enabled = true;
							PainEnhancerUI.Enabled = true;
						}
						else
						{
							((LabelRowDecorator)Label).Labels[1].Enabled = true;
							((LabelRowDecorator)Label).Labels[2].Enabled = false;
							PainEnhancerUI.Enabled = false;
						}
					}
					
					return true;
				}
			};
			
			Panel = new LabelColumnDecorator(Hud, 
				new LabelDelayedDecorator(Hud,
					new LabelAlignedDecorator(Hud, 
						new LabelStringDecorator(Hud, "СКОРОСТЬ АТАКИ") {Font = plugin.TitleFont, SpacingLeft = 15, SpacingRight = 15},
						plugin.CreatePin(this)
					)
				) {BackgroundBrush = plugin.BgBrush},
				new LabelRowCollection(Hud,
					new LabelRowDecorator(Hud,
						new LabelStringDecorator(Hud, "Скорость атаки правой рукой") {Font = TextFont, Alignment = HorizontalAlign.Right, SpacingLeft = 10, SpacingRight = 10},
						new LabelStringDecorator(Hud, () => Hud.Game.Me.Offense.AttackSpeedMainHand.ToString("0.####")) {Hint = plugin.CreateHint("Атак правой рукой в секунду"), Font = TextFont, Alignment = HorizontalAlign.Left, SpacingLeft = 10, SpacingRight = 10}
					),
					new LabelRowDecorator(Hud,
						new LabelStringDecorator(Hud, "Скорость атаки левой рукой") {Font = TextFont, Alignment = HorizontalAlign.Right, SpacingLeft = 10, SpacingRight = 10},
						new LabelStringDecorator(Hud, () => Hud.Game.Me.Offense.AttackSpeedOffHand.ToString("0.####")) {Hint = plugin.CreateHint("Атак левой рукой в секунду"), Font = TextFont, Alignment = HorizontalAlign.Left, SpacingLeft = 10, SpacingRight = 10}
					),
					new LabelRowDecorator(Hud,
						new LabelStringDecorator(Hud, "Скорость атаки питомца") {Font = TextFont, Alignment = HorizontalAlign.Right, SpacingLeft = 10, SpacingRight = 10},
						new LabelStringDecorator(Hud, () => /*((double)Hud.Game.Me.Offense.AttackSpeedPets).ToHumanReadable(2)*/ Hud.Game.Me.Offense.AttackSpeedPets.ToString("0.####")) {Font = TextFont, Alignment = HorizontalAlign.Left, SpacingLeft = 10, SpacingRight = 10}
					),
					PainEnhancerUI,
					/*new LabelRowDecorator(Hud,
						new LabelStringDecorator(Hud, "Attack Speed Bonus") {Font = TextFont, Alignment = HorizontalAlign.Right, SpacingLeft = 10, SpacingRight = 10},
						new LabelStringDecorator(Hud, () => Hud.Game.Me.Offense.AttackSpeedBonus.ToString("0.##")) {Font = TextFont, Alignment = HorizontalAlign.Left, SpacingLeft = 10, SpacingRight = 10}
					),*/
					new LabelRowDecorator(Hud,
						new LabelStringDecorator(Hud, "Процентные бонусы") {Font = TextFont, Alignment = HorizontalAlign.Right, SpacingLeft = 10, SpacingRight = 10},
						new LabelStringDecorator(Hud, () => (Hud.Game.Me.Offense.AttackSpeedPercent*100).ToString("0.##") + "%") {Font = TextFont, Alignment = HorizontalAlign.Left, SpacingLeft = 10, SpacingRight = 10}
					)
				) {
					SpacingBottom = 5,
					BackgroundBrush = plugin.BgBrush,
					HoveredBrush = plugin.HighlightBrush
				}
				/*new LabelTableDecorator(Hud,
					new LabelRowDecorator(Hud,
						new LabelTextureDecorator(Hud),
						new LabelStringDecorator(Hud) {Font = TextFont, Alignment = HorizontalAlign.Left}
					)
				) {
					SpacingBottom = 5,
					BackgroundBrush = plugin.BgBrush,
					HoveredBrush = plugin.HighlightBrush,
					OnBeforeRender = (label) => {
						
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
				}*/
			);
		}
	}
}