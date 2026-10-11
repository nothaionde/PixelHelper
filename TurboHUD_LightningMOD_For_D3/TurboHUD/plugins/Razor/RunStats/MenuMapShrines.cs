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

	public class MenuMapShrines : BasePlugin, IMenuAddon, INewAreaHandler//, ICustomizer, IInGameTopPainter /*, ILeftClickHandler, IRightClickHandler*/
	{
		public bool ShowPylons { get; set; } = true;
		public bool ShowShrines { get; set; } = true;
		public bool ShowPools { get; set; } = true;
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
		
		private LabelRowDecorator PylonUI;
		private LabelRowDecorator ShrineUI;
		private LabelRowDecorator PoolUI;
		private int CountPools;
		private int CountPoolsTaken;
		private int CountPylons;
		private int CountShrines;
		
		
        public MenuMapShrines()
        {
            Enabled = true;
			Priority = 10;
			DockId = "MinimapBottom";
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
        }
		
		/*public void Customize()
		{
			if (HideDefaultPlugin)
				Hud.TogglePlugin<ExperienceOverBarPlugin>(false);
		}*/
		
		public void OnNewArea(bool newGame, ISnoArea area)
		{
			
		}
		
		public void OnRegister(MenuPlugin plugin)
		{
			TextFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 225, 225, 225, false, false, 100, 0, 0, 0, true);
			//FadedFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 150, 150, 150, false, false, true);
			//EnabledFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 0, 255, 0, false, false, true);
			//DisabledFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 255, 0, 0, false, false, true); //170, 150, 120
			
			PylonUI = new LabelRowDecorator(Hud);
			ShrineUI = new LabelRowDecorator(Hud,
				new LabelStringDecorator(Hud, () => CountShrines.ToString()) {Font = TextFont},
				new LabelTextureDecorator(Hud, Hud.Texture.GetTexture(218235, 0)) {TextureWidth = 37, TextureHeight = 35, ContentHeight = plugin.MenuHeight, ContentWidth = 16}
			);
			PoolUI = new LabelRowDecorator(Hud,
				new LabelStringDecorator(Hud, () => (CountPools - CountPoolsTaken).ToString()) {Font = TextFont},
				new LabelTextureDecorator(Hud, Hud.Texture.GetTexture(376779, 0)) {TextureHeight = 38, ContentHeight = plugin.MenuHeight, ContentWidth = 18}
			);
			
			//Label
			Label = new LabelRowDecorator(Hud,
				PylonUI, //pylon count
				ShrineUI, //shrine count
				PoolUI //pool count
			) {
				OnBeforeRender = (label) => {
					if (Hud.Game.IsInTown)
					{
						if (plugin.Mover.EditMode)
						{
							PylonUI.Enabled = false;
							ShrineUI.Enabled = true;
							PoolUI.Enabled = true;
							return true;
						}
						
						return false;
					}
					
					CountPools = 0;
					CountPoolsTaken = 0;
					CountPylons = 0;
					CountShrines = 0;
					
					foreach (var marker in Hud.Game.Markers)
					{
						if (marker.IsPoolOfReflection)
						{
							++CountPools;

							if (marker.IsUsed)
								++CountPoolsTaken;
						}
						else if (marker.IsShrine)
							++CountShrines;
						else if (marker.IsPylon)
						{
							++CountPylons;
							if (PylonUI.Labels.Count < CountPylons)
								PylonUI.Labels.Add(new LabelTextureDecorator(Hud, GetPylonTexture(marker)) {TextureWidth = 37, TextureHeight = 35, ContentHeight = plugin.MenuHeight, ContentWidth = 18});
							else
							{
								LabelTextureDecorator ui = (LabelTextureDecorator)PylonUI.Labels[CountPylons - 1];
								ui.Enabled = true;
								ui.Texture = GetPylonTexture(marker);
							}
						}
					}
					
					if (CountPylons < PylonUI.Labels.Count)
					{
						for (int i = CountPylons; i < PylonUI.Labels.Count; ++i)
							PylonUI.Labels[i].Enabled = false;
					}
					
					PylonUI.Enabled = CountPylons > 0;
					
					if (CountShrines == 0)
						ShrineUI.Enabled = false;
					else
					{
						ShrineUI.Enabled = true;
						ShrineUI.SpacingLeft = (CountPylons == 0 ? 0 : 10);
					}
					
					if ((CountPools - CountPoolsTaken) <= 0)
						PoolUI.Enabled = false;
					else
					{
						PoolUI.Enabled = true;
						PoolUI.SpacingLeft = (CountPylons == 0 && CountShrines == 0 ? 0 : 10);
					}
					
					return true;
				}
			};
		}
		
		private ITexture GetPylonTexture(IMarker marker) //ShrineType type)
		{
			switch (marker.SnoActor.Sno)
			{
				case ActorSnoEnum._x1_lr_shrine_damage: //ShrineType.PowerPylon:
					return Hud.Texture.GetTexture((uint)(marker.IsUsed ? 455278 : 451494), 0); //Hud.Texture.GetTexture("Buff_Shrine_Damage");
				case ActorSnoEnum._x1_lr_shrine_electrified: //ShrineType.ConduitPylon:
					return Hud.Texture.GetTexture((uint)(marker.IsUsed ? 455277 : 451503), 0); //Hud.Texture.GetTexture("Buff_Shrine_Electrified");
				case ActorSnoEnum._x1_lr_shrine_infinite_casting: //ShrineType.ChannelingPylon:
					return Hud.Texture.GetTexture((uint)(marker.IsUsed ? 455276 : 451508), 0); //Hud.Texture.GetTexture("Buff_Shrine_Casting");
				case ActorSnoEnum._x1_lr_shrine_invulnerable: //ShrineType.ShieldPylon:
					return Hud.Texture.GetTexture((uint)(marker.IsUsed ? 455279 : 451493), 0); //Hud.Texture.GetTexture("Buff_Shrine_Invulnerable");
				case ActorSnoEnum._x1_lr_shrine_run_speed: //ShrineType.SpeedPylon:
					return Hud.Texture.GetTexture((uint)(marker.IsUsed ? 455280 : 451504), 0); //Hud.Texture.GetTexture("Buff_Shrine_Running");
			}
			
			return null;
		}
	}
}