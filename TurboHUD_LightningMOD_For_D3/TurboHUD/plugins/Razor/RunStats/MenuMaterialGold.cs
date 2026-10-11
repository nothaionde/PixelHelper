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

	public class MenuMaterialGold : BasePlugin, IMenuAddon, IAfterCollectHandler//, ICustomizer /*, IInGameTopPainter, ILeftClickHandler, IRightClickHandler*/
	{
		//public bool HideDefaultPlugin { get; set; } = true;
		
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
		public IFont MatFont { get; set; }
		public IFont GreenFont { get; set; }
		public IFont RedFont { get; set; }
		
		private LabelRowDecorator DeltaUI;
		private bool GracePeriod;
		private bool init;
		//private bool IsSeasonal;
		//private bool IsHardcore;
		private long LastSeenCount;
		private uint LastSeenHero;
		private int Gained;
		private int Spent;
		private IWatch Timer;
		
        public MenuMaterialGold()
        {
            Enabled = true;
			Priority = 33;
			DockId = "BottomRight";
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
			
			Timer = Hud.Time.CreateWatch();
        }
		
		public void OnRegister(MenuPlugin plugin)
		{
			//if (HideDefaultPlugin)
			//	Hud.TogglePlugin<TopExperienceStatistics>(false);
			
			TextFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 200, 200, 200, false, false, 100, 0, 0, 0, true);
			TimeFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 190, 255, 255, 255, false, false, true);
			MatFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 219, 187, 105, false, false, 100, 0, 0, 0, true);
			GreenFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 50, 255, 50, false, false, 100, 0, 0, 0, true);
			RedFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 255, 50, 50, false, false, 100, 0, 0, 0, true);
			
			Label = new LabelRowDecorator(Hud,
				new LabelStringDecorator(Hud, () => ValueToString(Hud.Game.Me.Materials.Gold, (Hud.Game.Me.Materials.Gold < 10000 ? ValueFormat.NormalNumberNoDecimal : ValueFormat.ShortNumber))) {FuncCacheDuration = 1, Font = MatFont, Alignment = HorizontalAlign.Left}, //Timer.ElapsedMilliseconds > 0 ? ValueToString((long)((Gained + Spent) / ((decimal)Timer.ElapsedMilliseconds / 3600000)), ValueFormat.LongNumber) + "/h" : "0/h") {FuncCacheDuration = 1, Font = MatFont, Alignment = HorizontalAlign.Left},
				new LabelTextureDecorator(Hud, Hud.Texture.GetTexture(3393203302)) {TextureHeight = plugin.MenuHeight-6}
			) {/*Hint = plugin.CreateHint("Level")*/};
			LabelHint = plugin.CreateHint("Золото");
			
			DeltaUI = new LabelRowDecorator(Hud,
				new LabelStringDecorator(Hud, "Δ") {Font = TimeFont, Alignment = HorizontalAlign.Right},
				new LabelStringDecorator(Hud, () => {
					var diff = Gained - Spent;
					if (diff < 1000000000)
						return ValueToString(diff, ValueFormat.NormalNumberNoDecimal);
					return ((double)diff).ToHumanReadable(5);
				}) {Font = TimeFont, Alignment = HorizontalAlign.Left, SpacingLeft = 20, SpacingRight = 20},
				new LabelStringDecorator(Hud, () => Timer.ElapsedMilliseconds > 0 ? ((double)((Gained + Spent) / ((decimal)Timer.ElapsedMilliseconds / 3600000))).ToHumanReadable(4) + "/ч" : "0/ч") {FuncCacheDuration = 1, Font = TimeFont, Alignment = HorizontalAlign.Left}
			);
			
			Panel = new LabelColumnDecorator(Hud, 
				new LabelDelayedDecorator(Hud,
					new LabelAlignedDecorator(Hud, 
						new LabelStringDecorator(Hud, "ЗОЛОТО") {Font = plugin.TitleFont, SpacingLeft = 10, SpacingRight = 10},
						plugin.CreateReset(this.Reset),
						plugin.CreatePin(this)
					)
				) {BackgroundBrush = plugin.BgBrush},
				new LabelAlignedDecorator(Hud,
					new LabelRowDecorator(Hud,
						new LabelTextureDecorator(Hud, Hud.Texture.GetTexture(3393203302)) {TextureHeight = plugin.MenuHeight-6},
						new LabelStringDecorator(Hud, () => ValueToString(Hud.Game.Me.Materials.Gold, ValueFormat.NormalNumberNoDecimal)) {Font = MatFont}
					) {Alignment = HorizontalAlign.Left, SpacingRight = 20},
					new LabelRowDecorator(Hud,
						new LabelStringDecorator(Hud, () => plugin.MillisecondsToString(Timer.ElapsedMilliseconds)) {Font = TimeFont, SpacingRight = 2},
						new LabelStringDecorator(Hud, "🕓") {Font = TimeFont}
					) {Alignment = HorizontalAlign.Right}
				) {BackgroundBrush = plugin.BgBrush, SpacingLeft = 10, SpacingRight = 10, SpacingBottom = 3},
				new LabelRowCollection(Hud,
					new LabelRowDecorator(Hud,
						new LabelStringDecorator(Hud, "Получено") {Font = GreenFont, Alignment = HorizontalAlign.Right},
						new LabelStringDecorator(Hud, () => Gained < 1000000000 ? ValueToString(Gained, ValueFormat.NormalNumberNoDecimal) : ((double)Gained).ToHumanReadable(5)) {Hint = plugin.CreateHint("Получил "), Font = GreenFont, Alignment = HorizontalAlign.Left, SpacingLeft = 20, SpacingRight = 20},
						new LabelStringDecorator(Hud, () => Timer.ElapsedMilliseconds > 0 ? ((double)(Gained / ((decimal)Timer.ElapsedMilliseconds / 3600000))).ToHumanReadable(4) + "/ч" : "0/ч") {FuncCacheDuration = 1, Font = GreenFont, Alignment = HorizontalAlign.Left}
					),
					new LabelRowDecorator(Hud,
						new LabelStringDecorator(Hud, "Потрачено") {Font = RedFont, Alignment = HorizontalAlign.Right},
						new LabelStringDecorator(Hud, () => Spent < 1000000000 ? ValueToString(Spent, ValueFormat.NormalNumberNoDecimal) : ((double)Spent).ToHumanReadable(5)) {Font = RedFont, Alignment = HorizontalAlign.Left, SpacingLeft = 20, SpacingRight = 20},
						new LabelStringDecorator(Hud, () => Timer.ElapsedMilliseconds > 0 ? ((double)(Spent / ((decimal)Timer.ElapsedMilliseconds / 3600000))).ToHumanReadable(4) + "/ч" : "0/ч") {FuncCacheDuration = 1, Font = RedFont, Alignment = HorizontalAlign.Left}
					),
					DeltaUI
				) {BackgroundBrush = plugin.BgBrush, SpacingLeft = 10, SpacingRight = 10, SpacingBottom = 5}
			);
			
			//Plugin = plugin;
		}
		
		public void AfterCollect()
		{
			if (!Hud.Game.IsInGame)
				return;

			//Hud.Game.Me.Hero.Seasonal is currently broken
			//if (!init || IsSeasonal != Hud.Game.Me.Hero.Seasonal || IsHardcore != Hud.Game.Me.Hero.Hardcore)
			
			if (!init) //|| IsSeasonal != Hud.Game.Me.Hero.Seasonal || IsHardcore != Hud.Game.Me.Hero.Hardcore)
			{
				init = true;
				//IsSeasonal = Hud.Game.Me.Hero.Seasonal;
				//IsHardcore = Hud.Game.Me.Hero.Hardcore;
				LastSeenCount = Hud.Game.Me.Materials.Gold;
				LastSeenHero = Hud.Game.Me.HeroId;
				Gained = 0;
				Spent = 0;
				DeltaUI.Enabled = false;
				
				GracePeriod = true;
				//Hud.Sound.Speak("Grace Period begin");
				if (Timer.IsRunning)
					Timer.Restart();
				else
					Timer.Start();
			}
			
			if (LastSeenHero != Hud.Game.Me.HeroId)
			{
				LastSeenHero = Hud.Game.Me.HeroId;
				
				GracePeriod = true;
				//Hud.Sound.Speak("Grace Period begin");
				if (Timer.IsRunning)
					Timer.Restart();
				else
					Timer.Start();
			}
			
			if (Timer.IsRunning)
			{
				if (LastSeenCount != Hud.Game.Me.Materials.Gold)
				{
					if (GracePeriod)
					{
						Timer.Restart();
						Gained = 0;
						Spent = 0;
					}
					else
					{
						if (LastSeenCount < Hud.Game.Me.Materials.Gold)
							Gained += (int)(Hud.Game.Me.Materials.Gold - LastSeenCount);
						else
							Spent += (int)(LastSeenCount - Hud.Game.Me.Materials.Gold);
						
						DeltaUI.Enabled = Gained != 0 && Spent != 0;
					}
					
					LastSeenCount = Hud.Game.Me.Materials.Gold;
				}
				else if (GracePeriod && Timer.ElapsedMilliseconds > 1500)
				{
					//Hud.Sound.Speak("Grace Period over");
					GracePeriod = false;
				}
			}
		}
		
		public void Reset(ILabelDecorator label)
		{
			init = false;
			Timer.Stop();
			Timer.Reset();
		}
	}
}