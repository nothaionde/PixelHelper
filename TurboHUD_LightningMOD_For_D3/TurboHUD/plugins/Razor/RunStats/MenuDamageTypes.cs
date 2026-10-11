namespace Turbo.Plugins.Razor.RunStats
{
	using System; //Convert
	using System.Globalization; //CultureInfo

	using Turbo.Plugins.Default;
	using Turbo.Plugins.Razor.Label;
	using Turbo.Plugins.Razor.Menu;

	public class MenuDamageTypes : BasePlugin, IMenuAddon//, ICustomizer//, IInGameTopPainter /*, ILeftClickHandler, IRightClickHandler*/
	{
		public bool HideDefaultPlugin { get; set; } = true;
		
		public IFont TextFont { get; set; }

		public ILabelDecorator Label { get; set; }
		public ILabelDecorator LabelHint { get; set; }
		public float LabelSize { get; set; }
		public ILabelDecorator Panel { get; set; }

		public string Id { get; set; }
		public int Priority { get; set; } //the priority on the dock to show this addon (smaller to the left, higher to the right)
		public string DockId { get; set; }
		public string Config { get; set; }
		
        public MenuDamageTypes()
        {
            Enabled = true;
			Priority = 60;
			DockId = "BottomLeft";
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
        }
		
		public void OnRegister(MenuPlugin plugin)
		{
			if (HideDefaultPlugin)
				Hud.TogglePlugin<DamageBonusPlugin>(false);
			
			TextFont = Hud.Render.CreateFont("tahoma", plugin.FontSize - 1f, 255, 225, 225, 225, false, false, 100, 0, 0, 0, true);
			
			//Label
			Label = new LabelRowDecorator(Hud,
				//stone of jordan
				new LabelRowDecorator(Hud,
					new LabelStringDecorator(Hud, () => Convert.ToInt32(Hud.Game.Me.Offense.HighestElementalDamageBonus * 100).ToString("D", CultureInfo.InvariantCulture)) {Font = TextFont},
					new LabelTextureDecorator(Hud, Hud.Texture.GetItemTexture(Hud.Sno.SnoItems.Unique_Ring_019_x1)) {TextureHeight = 30, ContentHeight = plugin.MenuHeight, ContentWidth = 20}
				) {Hint = plugin.CreateHint("Иорданов Камень")},
				//types
				new LabelStringDecorator(Hud, () => Convert.ToInt32(Hud.Game.Me.Offense.BonusToElites * 100).ToString("D", CultureInfo.InvariantCulture)) {Hint = plugin.CreateHint("Урон по Элите"), BackgroundBrush = Hud.Render.CreateBrush(225, 218, 165, 32, 0), Font = TextFont},
				new LabelStringDecorator(Hud, () => Convert.ToInt32(Hud.Game.Me.Offense.BonusToArcane * 100).ToString("D", CultureInfo.InvariantCulture)) {Hint = plugin.CreateHint("Урону от Тайной Магии"), BackgroundBrush = Hud.Render.CreateBrush(225, 249, 0, 161, 0), Font = TextFont},
				new LabelStringDecorator(Hud, () => Convert.ToInt32(Hud.Game.Me.Offense.BonusToCold * 100).ToString("D", CultureInfo.InvariantCulture)) {Hint = plugin.CreateHint("Урона от Холода"), BackgroundBrush = Hud.Render.CreateBrush(225, 150, 199, 246, 0), Font = TextFont},
				new LabelStringDecorator(Hud, () => Convert.ToInt32(Hud.Game.Me.Offense.BonusToFire * 100).ToString("D", CultureInfo.InvariantCulture)) {Hint = plugin.CreateHint("Урона от Огня"), BackgroundBrush = Hud.Render.CreateBrush(225, 255, 90, 8, 0), Font = TextFont},
				new LabelStringDecorator(Hud, () => Convert.ToInt32(Hud.Game.Me.Offense.BonusToHoly * 100).ToString("D", CultureInfo.InvariantCulture)) {Hint = plugin.CreateHint("Урон от Сил Света"), BackgroundBrush = Hud.Render.CreateBrush(225, 252, 239, 0, 0), Font = TextFont},
				new LabelStringDecorator(Hud, () => Convert.ToInt32(Hud.Game.Me.Offense.BonusToLightning * 100).ToString("D", CultureInfo.InvariantCulture)) {Hint = plugin.CreateHint("Урону от Молнии"), BackgroundBrush = Hud.Render.CreateBrush(225, 50, 50, 255, 0), Font = TextFont},
				new LabelStringDecorator(Hud, () => Convert.ToInt32(Hud.Game.Me.Offense.BonusToPhysical * 100).ToString("D", CultureInfo.InvariantCulture)) {Hint = plugin.CreateHint("Физический Урон"), BackgroundBrush = Hud.Render.CreateBrush(225, 185, 185, 185, 0), Font = TextFont},
				new LabelStringDecorator(Hud, () => Convert.ToInt32(Hud.Game.Me.Offense.BonusToPoison * 100).ToString("D", CultureInfo.InvariantCulture)) {Hint = plugin.CreateHint("Физический Урон"), BackgroundBrush = Hud.Render.CreateBrush(225, 0, 255, 0, 0), Font = TextFont}
			) {
				Gap = 1f,
				OnBeforeRender = (label) => {
					LabelRowDecorator row = (LabelRowDecorator)label;
					
					if (Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.StoneOfJordan.Sno))
					{
						row.Labels[0].Enabled = true;
						for (int i = 1; i < row.Labels.Count; ++i)
							row.Labels[i].Enabled = false;
					}
					else
					{
						for (int i = 1; i < row.Labels.Count; ++i)
						{
							row.Labels[i].Width = plugin.MenuHeight;
							
						}
						
						row.Labels[0].Enabled = false;
						row.Labels[1].Enabled = Hud.Game.Me.Offense.BonusToElites > 0;
						row.Labels[2].Enabled = Hud.Game.Me.Offense.BonusToArcane > 0;
						row.Labels[3].Enabled = Hud.Game.Me.Offense.BonusToCold > 0;
						row.Labels[4].Enabled = Hud.Game.Me.Offense.BonusToFire > 0;
						row.Labels[5].Enabled = Hud.Game.Me.Offense.BonusToHoly > 0;
						row.Labels[6].Enabled = Hud.Game.Me.Offense.BonusToLightning > 0;
						row.Labels[7].Enabled = Hud.Game.Me.Offense.BonusToPhysical > 0;
						row.Labels[8].Enabled = Hud.Game.Me.Offense.BonusToPoison > 0;
					}
					
					return true;
				}
			};
		}
	}
}