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

	public class MenuMaterialDeathsBreath : BasePlugin, IMenuAddon, IAfterCollectHandler//, ICustomizer /*, IInGameTopPainter, ILeftClickHandler, IRightClickHandler*/
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
		
        public MenuMaterialDeathsBreath()
        {
            Enabled = true;
			Priority = 31;
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
			MatFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 66, 245, 230, false, false, 100, 0, 0, 0, true);
			GreenFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 50, 255, 50, false, false, 100, 0, 0, 0, true);
			RedFont = Hud.Render.CreateFont("tahoma", plugin.FontSize, 255, 255, 50, 50, false, false, 100, 0, 0, 0, true);
			
			Label = new LabelRowDecorator(Hud,
				new LabelStringDecorator(Hud, () => ValueToString(Hud.Game.Me.Materials.DeathsBreath, (Hud.Game.Me.Materials.DeathsBreath < 10000 ? ValueFormat.NormalNumberNoDecimal : ValueFormat.ShortNumber))) {FuncCacheDuration = 1, Font = MatFont, Alignment = HorizontalAlign.Left}, //Timer.ElapsedMilliseconds > 0 ? ValueToString((long)((Gained + Spent) / ((decimal)Timer.ElapsedMilliseconds / 3600000)), ValueFormat.NormalNumberNoDecimal) + "/h" : "0/h") {FuncCacheDuration = 1, Font = MatFont, Alignment = HorizontalAlign.Left},
				new LabelTextureDecorator(Hud, Hud.Texture.GetItemTexture(Hud.Inventory.GetSnoItem(2087837753))) {TextureHeight = plugin.MenuHeight, ContentHeight = plugin.MenuHeight, ContentWidth = 18}
			) {/*Hint = plugin.CreateHint("Level")*/};
			LabelHint = plugin.CreateHint("Дыхание Смерти");
			
			DeltaUI = new LabelRowDecorator(Hud,
				new LabelStringDecorator(Hud, "Δ") {Font = TimeFont, Alignment = HorizontalAlign.Right},
				new LabelStringDecorator(Hud, () => ValueToString(Gained - Spent, ValueFormat.NormalNumberNoDecimal)) {Font = TimeFont, Alignment = HorizontalAlign.Left, SpacingLeft = 20, SpacingRight = 20},
				new LabelStringDecorator(Hud, () => Timer.ElapsedMilliseconds > 0 ? ((double)((Gained + Spent) / ((decimal)Timer.ElapsedMilliseconds / 3600000))).ToHumanReadable(4) + "/ч" : "0/ч") {FuncCacheDuration = 1, Font = TimeFont, Alignment = HorizontalAlign.Left}
			);
			
			Panel = new LabelColumnDecorator(Hud, 
				new LabelDelayedDecorator(Hud,
					new LabelAlignedDecorator(Hud, 
						new LabelStringDecorator(Hud, "ДЫХАНИЕ СМЕРТИ") {Font = plugin.TitleFont, SpacingLeft = 10, SpacingRight = 10},
						plugin.CreateReset(this.Reset),
						plugin.CreatePin(this)
					)
				) {BackgroundBrush = plugin.BgBrush},
				new LabelAlignedDecorator(Hud,
					new LabelRowDecorator(Hud,
						new LabelTextureDecorator(Hud, Hud.Texture.GetItemTexture(Hud.Inventory.GetSnoItem(2087837753))) {TextureHeight = plugin.MenuHeight},
						new LabelStringDecorator(Hud, () => ValueToString(Hud.Game.Me.Materials.DeathsBreath, ValueFormat.NormalNumberNoDecimal)) {Font = MatFont}
					) {Alignment = HorizontalAlign.Left, SpacingRight = 20},
					new LabelRowDecorator(Hud,
						new LabelStringDecorator(Hud, () => plugin.MillisecondsToString(Timer.ElapsedMilliseconds)) {Font = TimeFont, SpacingRight = 2},
						new LabelStringDecorator(Hud, "🕓") {Font = TimeFont}
					) {Alignment = HorizontalAlign.Right}
				) {BackgroundBrush = plugin.BgBrush, SpacingLeft = 10, SpacingRight = 10, SpacingBottom = 3},
				new LabelRowCollection(Hud,
					new LabelRowDecorator(Hud,
						new LabelStringDecorator(Hud, "Получено") {Font = GreenFont, Alignment = HorizontalAlign.Right},
						new LabelStringDecorator(Hud, () => ValueToString(Gained, ValueFormat.NormalNumberNoDecimal)) {Hint = plugin.CreateHint("Получил "), Font = GreenFont, Alignment = HorizontalAlign.Left, SpacingLeft = 20, SpacingRight = 20},
						new LabelStringDecorator(Hud, () => Timer.ElapsedMilliseconds > 0 ? ((double)(Gained / ((decimal)Timer.ElapsedMilliseconds / 3600000))).ToHumanReadable(4) + "/ч" : "0/ч") {FuncCacheDuration = 1, Font = GreenFont, Alignment = HorizontalAlign.Left}
					),
					new LabelRowDecorator(Hud,
						new LabelStringDecorator(Hud, "Потрачено") {Font = RedFont, Alignment = HorizontalAlign.Right},
						new LabelStringDecorator(Hud, () => ValueToString(Spent, ValueFormat.NormalNumberNoDecimal)) {Font = RedFont, Alignment = HorizontalAlign.Left, SpacingLeft = 20, SpacingRight = 20},
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

			if (!init) //|| IsSeasonal != Hud.Game.Me.Hero.Seasonal || IsHardcore != Hud.Game.Me.Hero.Hardcore)
			{
				init = true;
				//IsSeasonal = Hud.Game.Me.Hero.Seasonal;
				//IsHardcore = Hud.Game.Me.Hero.Hardcore;
				LastSeenCount = Hud.Game.Me.Materials.DeathsBreath;
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
				if (LastSeenCount != Hud.Game.Me.Materials.DeathsBreath)
				{
					if (GracePeriod)
					{
						Timer.Restart();
						Gained = 0;
						Spent = 0;
					}
					else
					{
						if (LastSeenCount < Hud.Game.Me.Materials.DeathsBreath)
							Gained += (int)(Hud.Game.Me.Materials.DeathsBreath - LastSeenCount);
						else
							Spent += (int)(LastSeenCount - Hud.Game.Me.Materials.DeathsBreath);
						
						DeltaUI.Enabled = Gained != 0 && Spent != 0;
					}
					
					LastSeenCount = Hud.Game.Me.Materials.DeathsBreath;
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