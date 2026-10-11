namespace Turbo.Plugins.Razor.RunStats
{
	using SharpDX.DirectWrite;
	using System;
	using System.Drawing;
	using System.Linq;
	using System.Collections.Generic;

	using Turbo.Plugins.Default;
	using Turbo.Plugins.Razor.Log;
	using Turbo.Plugins.Razor.Label;
	using Turbo.Plugins.Razor.Menu;
	using Turbo.Plugins.Razor.Util;

	public class MenuSpiritBarrage : BasePlugin, IMenuAddon, IBeforeRenderHandler //IAfterCollectHandler /*, IInGameTopPainter, ILeftClickHandler, IRightClickHandler*/
	{
		public int HistoryShown { get; set; } = 15;
		public float HistoryShownTimeout { get; set; } = 15f; //outside of town, only show entries for this many seconds
		
		public string TextNotApplicable { get; set; } = "--";
		
		public string Id { get; set; }
		public int Priority { get; set; } //the priority on the dock to show this addon (smaller to the left, higher to the right)
		public string DockId { get; set; }
		public string Config { get; set; }

		public ILabelDecorator Label { get; set; }
		public ILabelDecorator LabelHint { get; set; }
		public float LabelSize { get; set; }
		public ILabelDecorator Panel { get; set; }

		public IFont TextFont { get; set; }
		public IFont TimeFont { get; set; }
		public IFont LevelFont { get; set; }
		public IFont ParagonFont { get; set; }
		public IFont GreenFont { get; set; }
		
		public IBrush ActiveBrush { get; set; }
		
		private LabelTableDecorator Table;
		private SpiritBarrageHelper Helper;
		private uint LastLevelSeen;
		private int LastUpdateTick;
		
        public MenuSpiritBarrage()
        {
            Enabled = true;
			Priority = 20;
			DockId = "BottomLeft";
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
        }
		
		public void OnRegister(MenuPlugin plugin)
		{
			//LabelSize = Hud.Window.Size.Height * 0.05f;
			//Pin = plugin.CreatePin(); //enable pin handling
			//Pin.Alignment = HorizontalAlign.Right;
			Helper = Hud.GetPlugin<SpiritBarrageHelper>();
			
			TextFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 175, 175, 175, false, false, 135, 0, 0, 0, true);
			TimeFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 255, 255, 255, false, false, 135, 0, 0, 0, true);
			LevelFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 87, 137, 255, false, false, 135, 0, 0, 0, true); //57, 137, 205
			ParagonFont = Hud.Render.CreateFont("tahoma", plugin.FontSize - 1f, 255, 255, 255, 255, true, false, 135, 0, 0, 0, true);
			GreenFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 0, 255, 0, false, false, 135, 0, 0, 0, true);
			
			ActiveBrush = plugin.HighlightBrush;
			
			//Label
			Label = new LabelTextureDecorator(Hud, Hud.Texture.GetTexture(Hud.Sno.SnoPowers.WitchDoctor_SpiritBarrage.NormalIconTextureId)) {
				Enabled = Helper.SaveBarrageDetails, 
				TextureHeight = plugin.MenuHeight - 1, 
				ContentHeight = plugin.MenuHeight,
				//OnBeforeRender = (label) => Helper.Phantasms.Count > 0 || Helper.DisplayedPlayer is object, //hide if there is no data saved or there is no witch doctor around
			};
			LabelHint = plugin.CreateHint("Spirit Barrage Inspector");
			
			//Menu
			Table = new LabelTableDecorator(Hud, 
				new LabelRowDecorator(Hud,
					//timestamp
					new LabelStringDecorator(Hud) {Hint = plugin.CreateHint("Time"), Font = TextFont, Alignment = HorizontalAlign.Right, SpacingLeft = 5, SpacingRight = 5, SpacingTop = 2, SpacingBottom = 2},
					//ticks
					new LabelStringDecorator(Hud) {Hint = plugin.CreateHint("Ticks"), Font = TimeFont, SpacingLeft = 5, SpacingRight = 5, SpacingTop = 2, SpacingBottom = 2},
					//attack speed snapshot
					new LabelStringDecorator(Hud) {Hint = plugin.CreateHint("Attack Speed Snapshot"), Font = TextFont, SpacingLeft = 5, SpacingRight = 5, SpacingTop = 2, SpacingBottom = 2},
					//gruesome feast stacks
					new LabelStringDecorator(Hud) {Hint = plugin.CreateHint("Gruesome Feast Stacks"), Font = TextFont, SpacingLeft = 5, SpacingRight = 5, SpacingTop = 2, SpacingBottom = 2},
					//sacrifice stacks
					new LabelStringDecorator(Hud) {Hint = plugin.CreateHint("Sacrifice Stacks"), Font = TextFont, SpacingLeft = 5, SpacingRight = 5, SpacingTop = 2, SpacingBottom = 2},
					//density sum
					new LabelStringDecorator(Hud) {Hint = plugin.CreateHint("Monster Density * Ticks"), Font = TextFont, SpacingLeft = 5, SpacingRight = 5, SpacingTop = 2, SpacingBottom = 2},
					//confidence coverage
					new LabelStringDecorator(Hud) {Hint = plugin.CreateHint("Confidence Ritual (Passive)"), Font = TextFont, SpacingLeft = 5, SpacingRight = 5, SpacingTop = 2, SpacingBottom = 2},
					//convention coverage
					new LabelStringDecorator(Hud) {Hint = plugin.CreateHint("Convention of Elements (Cold)"), Font = TextFont, SpacingLeft = 5, SpacingRight = 5, SpacingTop = 2, SpacingBottom = 2},
					//oculus coverage
					new LabelStringDecorator(Hud) {Hint = plugin.CreateHint("Oculus"), Font = TextFont, SpacingLeft = 5, SpacingRight = 5, SpacingTop = 2, SpacingBottom = 2},
					//endless walk
					new LabelStringDecorator(Hud) {Hint = plugin.CreateHint("Endless Walk (stack average)"), Font = TextFont, SpacingLeft = 5, SpacingRight = 5, SpacingTop = 2, SpacingBottom = 2},
					//squirts
					new LabelStringDecorator(Hud) {Hint = plugin.CreateHint("Squirts Necklace (stack average)"), Font = TextFont, SpacingLeft = 5, SpacingRight = 5, SpacingTop = 2, SpacingBottom = 2},
					//power pylon coverage
					new LabelStringDecorator(Hud) {Hint = plugin.CreateHint("Power Pylon"), Font = TextFont, SpacingLeft = 5, SpacingRight = 5, SpacingTop = 2, SpacingBottom = 2},
					//channeling pylon coverage
					new LabelStringDecorator(Hud) {Hint = plugin.CreateHint("Channeling Pylon"), Font = TextFont, SpacingLeft = 5, SpacingRight = 5, SpacingTop = 2, SpacingBottom = 2}
				)
			) {
				BackgroundBrush = plugin.BgBrush,
				HoveredBrush = plugin.HighlightBrush,
				SpacingLeft = 10,
				SpacingRight = 10,
				Header = new LabelRowDecorator(Hud,
					//time spent
					new LabelStringDecorator(Hud, "🕓") {Hint = plugin.CreateHint("Time"), Font = TimeFont, SpacingLeft = 5, SpacingRight = 5},
					//tick count
					new LabelStringDecorator(Hud, "Ticks") {Hint = plugin.CreateHint("Ticks"), Font = TimeFont, SpacingLeft = 5, SpacingRight = 5},
					//attack speed snapshot
					new LabelStringDecorator(Hud, "⇌") {Hint = plugin.CreateHint("Attack Speed Snapshot"), Font = TimeFont, SpacingLeft = 5, SpacingRight = 5},
					//fpa
					//new LabelStringDecorator(Hud, "FPA") {Font = TimeFont},
					//gruesome feast stacks
					new LabelTextureDecorator(Hud, Hud.Texture.GetTexture(1591242582)) {Hint = plugin.CreateHint("Gruesome Feast Stacks"), TextureHeight = plugin.MenuHeight - 2, ContentHeight = plugin.MenuHeight, SpacingLeft = 5, SpacingRight = 5}, //gruesome feast stacks //texture id borrowed from RNN's spiritbarrageandcoe
					//provoke the pack
					new LabelTextureDecorator(Hud, Hud.Texture.GetTexture(Hud.Sno.SnoPowers.WitchDoctor_Sacrifice.NormalIconTextureId)) {Hint = plugin.CreateHint("Sacrifice Stacks"), TextureHeight = plugin.MenuHeight, ContentHeight = plugin.MenuHeight, SpacingLeft = 5, SpacingRight = 5},
					//density tick accumulator - number of mobs inside
					new LabelTextureDecorator(Hud, Hud.Texture.GetTexture(2462638855)) {Hint = plugin.CreateHint("Monster Density * Ticks"), /*TextureHeight = plugin.MenuHeight - 2, */ContentHeight = plugin.MenuHeight, SpacingLeft = 5, SpacingRight = 5},
					//confidence ritual coverage = confidence ticks / density ticks
					new LabelTextureDecorator(Hud, Hud.Texture.GetTexture(Hud.Sno.SnoPowers.WitchDoctor_Passive_ConfidenceRitual.NormalIconTextureId)) {Hint = plugin.CreateHint("Confidence Ritual (Passive)"), TextureHeight = plugin.MenuHeight, ContentHeight = plugin.MenuHeight, SpacingLeft = 5, SpacingRight = 5},
					//convention cold
					new LabelTextureDecorator(Hud, Hud.Texture.GetTexture(Hud.Sno.SnoPowers.ConventionOfElements.Icons[2].TextureId)) {Hint = plugin.CreateHint("Convention of Elements (Cold)"), TextureHeight = plugin.MenuHeight, ContentHeight = plugin.MenuHeight, SpacingLeft = 5, SpacingRight = 5},
					//oculus
					new LabelTextureDecorator(Hud, Hud.Texture.GetItemTexture(Hud.Sno.SnoItems.Unique_Ring_017_p4)) {Hint = plugin.CreateHint("Oculus"), TextureHeight = 29, ContentHeight = plugin.MenuHeight, SpacingLeft = 5, SpacingRight = 5},
					//endless walk
					new LabelTextureDecorator(Hud, Hud.Texture.GetItemTexture(Hud.Sno.SnoItems.Unique_Amulet_008_x1)) {Hint = plugin.CreateHint("Endless Walk"), TextureHeight = plugin.MenuHeight, ContentHeight = plugin.MenuHeight, SpacingLeft = 5, SpacingRight = 5},
					//squirts
					new LabelTextureDecorator(Hud, Hud.Texture.GetItemTexture(Hud.Sno.SnoItems.P66_Unique_Amulet_010)) {Hint = plugin.CreateHint("Squirts Necklace"), TextureHeight = plugin.MenuHeight, ContentHeight = plugin.MenuHeight, SpacingLeft = 5, SpacingRight = 5},
					//power pylon
					new LabelTextureDecorator(Hud, Hud.Texture.GetTexture("Buff_Shrine_Damage")) {Hint = plugin.CreateHint("Power Pylon"), TextureHeight = plugin.MenuHeight - 2, ContentHeight = plugin.MenuHeight, SpacingLeft = 5, SpacingRight = 5},
					//channeling pylon
					new LabelTextureDecorator(Hud, Hud.Texture.GetTexture("Buff_Shrine_Casting")) {Hint = plugin.CreateHint("Channeling Pylon"), TextureHeight = plugin.MenuHeight - 2, ContentHeight = plugin.MenuHeight, SpacingLeft = 5, SpacingRight = 5}
				) {SpacingTop = 3, SpacingBottom = 4},
				FillWidth = false, //true,
				OnFillRow = (row, index) => {
					if (index >= Helper.Phantasms.Count) //Helper.DisplayedPlayer == null || 
						return false;
					
					var phant = Helper.Phantasms[Helper.Phantasms.Count - 1 - index];
					TimeSpan elapsed = Hud.Time.Now - phant.Timestamp;
					if (Hud.Game.IsInTown)
					{
						if (index >= HistoryShown)
							return false;
					}
					else if (elapsed.TotalSeconds > HistoryShownTimeout)
					{
						//row.Enabled = false;
						return false; // all the subsequent entries would be even older
					}
					
					//row.Enabled = true;
					
					//highlight any phantasms that are active
					row.BackgroundBrush = phant.DeathTick > Hud.Game.CurrentGameTick && Hud.Game.CurrentGameTick - phant.LastSeen < 5 ? ActiveBrush : null;
					
					//timestamp
					LabelStringDecorator label = (LabelStringDecorator)row.Labels[0];
					//TimeSpan elapsed = Hud.Time.Now - phant.Timestamp;
					if (elapsed.TotalSeconds < 60)
						label.StaticText = elapsed.TotalSeconds.ToString("F0") + "s ago";
					//else if (elapsed.TotalMinutes < 10)
					//	label.StaticText = elapsed.TotalMinutes.ToString("F0") + "m ago";
					else
						label.StaticText = phant.Timestamp.ToString("T");
					
					//tick count
					label = (LabelStringDecorator)row.Labels[1];
					label.StaticText = phant.Ticks.ToString();
					
					//attack speed snapshot
					label = (LabelStringDecorator)row.Labels[2];
					label.StaticText = phant.AttackSpeed.ToString("0.##");
					
					//gruesome feast stacks
					label = (LabelStringDecorator)row.Labels[3];
					label.StaticText = phant.GruesomeStacks.ToString();
					
					//sacrifice stacks
					label = (LabelStringDecorator)row.Labels[4];
					label.StaticText = phant.SacrificeStacks.ToString();
					
					int a = 5;
					if (phant.Ticks == 0)
					{
						//density count (average?)
						label = (LabelStringDecorator)row.Labels[a++];
						label.StaticText = TextNotApplicable;
						
						//confidence ritual coverage
						label = (LabelStringDecorator)row.Labels[a++];
						label.StaticText = TextNotApplicable;
						
						//convention cold tick coverage
						label = (LabelStringDecorator)row.Labels[a++];
						label.StaticText = TextNotApplicable;
						
						//oculus tick coverage
						label = (LabelStringDecorator)row.Labels[a++];
						label.StaticText = TextNotApplicable;
						
						//endless stack average
						label = (LabelStringDecorator)row.Labels[a++];
						label.StaticText = TextNotApplicable;
						
						//squirts stack average
						label = (LabelStringDecorator)row.Labels[a++];
						label.StaticText = TextNotApplicable;
						
						//power pylon tick coverage
						label = (LabelStringDecorator)row.Labels[a++];
						label.StaticText = TextNotApplicable;
						
						//channeling pylon tick coverage
						label = (LabelStringDecorator)row.Labels[a++];
						label.StaticText = TextNotApplicable;
						
						//damage dealt
					}
					else
					{
						//density count (average?)
						label = (LabelStringDecorator)row.Labels[a++];
						label.StaticText = phant.DensitySum.ToString();
						
						//confidence ritual coverage
						label = (LabelStringDecorator)row.Labels[a++];
						label.StaticText = (phant.DensitySum > 0 ? ((float)phant.ConfidenceTicks / (float)phant.DensitySum)*100f : 0).ToString("F0") + "%";
						
						//convention cold tick coverage
						label = (LabelStringDecorator)row.Labels[a++];
						label.StaticText = ((float)phant.ConventionTicks / 2f).ToString("0.#") + "s"; //(((float)phant.ConventionTicks / 8f)*100f).ToString("F0") + "%";
						
						//oculus tick coverage
						label = (LabelStringDecorator)row.Labels[a++];
						label.StaticText = ((float)phant.OculusTicks / 2f).ToString("0.#") + "s"; //(((float)phant.OculusTicks / 20f)*100f).ToString("F0") + "%";
						
						//endless stack average
						label = (LabelStringDecorator)row.Labels[a++];
						label.StaticText = ((float)phant.EndlessSum / (float)phant.Ticks).ToString("F0");
						
						//squirts stack average
						label = (LabelStringDecorator)row.Labels[a++];
						label.StaticText = ((float)phant.SquirtsSum / (float)phant.Ticks).ToString("F0");
						
						//power pylon tick coverage
						label = (LabelStringDecorator)row.Labels[a++];
						label.StaticText = ((float)phant.PowerPylonTicks / 2f).ToString("0.#") + "s"; //(((float)phant.PowerPylonTicks / 20f)*100f).ToString("F0") + "%";
						
						//channeling pylon tick coverage
						label = (LabelStringDecorator)row.Labels[a++];
						label.StaticText = ((float)phant.ChannelingPylonTicks / 2f).ToString("0.#") + "s"; //(((float)phant.ChannelingPylonTicks / 20f)*100f).ToString("F0") + "%";
						
						//damage dealt
						//row[a++].TextFunc = Hud.Game.CurrentGameTick < phant.DeathTick ? TextNotApplicable : (() => ValueToString(phant.DamageDealt, ValueFormat.ShortNumber));
					}
					
													//timestamp
								/*row[0].TextFunc = () => {
									TimeSpan elapsed = Hud.Time.Now - rift.Timestamp;
									string time;
									if (elapsed.TotalSeconds < 60)
										time = elapsed.TotalSeconds.ToString("F0") + "s ago";
									else if (elapsed.TotalMinutes < 10)
										time = elapsed.TotalMinutes.ToString("F0") + "m ago";
									else
										time = rift.Timestamp.ToString("hh:mm tt");
									
									return time;
								};*/
								
					return true;
				}
			};
			
			Panel = new LabelColumnDecorator(Hud, 
				new LabelDelayedDecorator(Hud,
					new LabelAlignedDecorator(Hud, 
						new LabelStringDecorator(Hud, "SPIRIT BARRAGE INSPECTOR") {Font = plugin.TitleFont, SpacingLeft = 10, SpacingRight = 10},
						//plugin.CreateReset(this.Reset),
						plugin.CreatePin(this)
					)
				) {BackgroundBrush = plugin.BgBrush},
				Table
			) {};
			
			//Plugin = plugin;
		}
		
		public void BeforeRender()
		{
			if (Helper == null) //!Hud.Game.IsInGame || Helper == null)
				return;
			
			Label.Enabled = Helper.Enabled && (Helper.Phantasms.Count > 0 || Helper.DisplayedPlayer is object);
		}
	}
}