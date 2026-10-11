/*

This plugin checks a variety of statuses to show you whether or not your character is currently immune to damage. 
It has a movable countdown bar, and an optional visual hint when your character is under the effects of certain saving graces.

Changelog
September 5, 2021
	- the countdown bar is now resizable (the wrong flag set previously)
July 29, 2021
	- fixed the left bracket from shifting during movement (player.FloorCoordinate.ToScreenCoordinate() is not the same thing as player.ScreenCoordinate, apparently)
	- rewrote the draw code to tie all countdown number positions to the CountdownBar movable area and implemented ShowCountdownOthers (off by default)
July 21, 2021
	- Added follower Cheat Death immunities (thanks to Jembo for the buff data and testing)
July 19, 2021
	- Fixed ShowIndicator = false resulting in not showing Countdown as well
June 26, 2021
	- Added teammate immunity indicators and countdown bar
May 3, 2021
	- removed 3pc Crimson Set + Land of the Dead: Invigoration interaction
September 2, 2020
	- rewrite
*/

namespace Turbo.Plugins.Razor
{
	using SharpDX.DirectWrite;
	using System;
	using System.Drawing;
	using System.Linq;

	using Turbo.Plugins.Default;
	using Turbo.Plugins.Razor.Label;
	using Turbo.Plugins.Razor.Movable;

	public class ImmunityHelper : BasePlugin, IInGameTopPainter, IMovable
	{
		public bool ShowIndicator { get; set; } = false; //show parentheses around your character when immune
		public bool ShowIndicatorOthers { get; set; } = false; //show parentheses around teammates when they are immune (showing others immunity status can clutter the screen)
		public bool ShowCountdown { get; set; } = true; //showing countdown area
		public bool ShowCountdownOthers { get; set; } = true; //showing countdown area
		public bool ShowCountdownBar { get; set; } = true;
		public bool ShowCountdownBarOthers { get; set; } = true;
		//public bool ShowCountdownOnCursor { get; set; } = false;

		public string ImmunityText { get; set; } = "Иммуннитет"; //for immunities that only last a fraction of a second
		public IFont NotifyFont { get; set; }
		public IFont SFont { get; set; }
		public IFont StyleFont { get; set; } //for style 2
		
		public bool IsImmune { get; private set; }
		public double ImmuneDuration { get; private set; }
		public double ImmuneTotalTime { get; private set; }
		
		//public float BarWidth { get; set; } = 30f; //35f
		//public float BarHeight { get; set; } = 5f;
		public IBrush TimerHigh { get; set; }
		public IBrush TimerLow { get; set; }
		public IBrush BgBrush { get; set; }
		public IBrush SkillBorderLight { get; set; }
		public IBrush SkillBorderDark { get; set; }
		
		private LabelProgressBarDecorator CountdownBar;
		private float BarOffsetX;
		private float BarOffsetY;
		private float BarWidth;
		private float BarHeight;
		
		//private double duration;
		//private double totaltime;
		//private int LandStartTick;
		//private int RCRStartTick;
		
        public ImmunityHelper()
        {
            Enabled = true;
            Order = 100003;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);
			
			NotifyFont = Hud.Render.CreateFont("tahoma", 20f, 255, 98, 247, 252, true, false, 100, 0, 0, 0, true); //242, 192, 41 //235, 189, 52
			SFont = Hud.Render.CreateFont("tahoma", 10f, 255, 0, 0, 0, true, false, 100, 98, 247, 252, true); //98, 252, 234 //242, 192, 41 //255, 100, 50
			StyleFont = Hud.Render.CreateFont("arial", 75f, 255, 0, 0, 0, false, false, 175, 98, 247, 252, true); //255, 201, 38 //235, 189, 52
			
			BgBrush = Hud.Render.CreateBrush(150, 25, 25, 25, 0);
			TimerHigh = Hud.Render.CreateBrush(255, 98, 247, 252, 0); //Hud.Render.CreateBrush(255, 0, 255, 100, 0); //242, 192, 41 //255, 201, 38 //0, 255, 100
			TimerLow = Hud.Render.CreateBrush(255, 255, 0, 0, 0);
			
			SkillBorderLight = Hud.Render.CreateBrush(125, 98, 247, 252, 1); //95, 95, 95 //235, 227, 164 //138, 135, 109
			SkillBorderDark = Hud.Render.CreateBrush(200, 0, 0, 0, 1);
			
			CountdownBar = new LabelProgressBarDecorator(Hud) {
				BarHeight = 1080 * 0.001667f * 5f, //BarHeight, 
				BarWidth = 1920 * 0.00155f * 30f, //BarWidth,
				Direction = HorizontalAlign.Center,
				BackgroundBrush = BgBrush,
				BarBrush = TimerHigh, 
				BarBrushUnderlay = TimerLow, 
				BorderBrush = SkillBorderDark//BorderBrush//, SpacingLeft = 5, SpacingRight = 5};
			};
        }
		
        public void PaintTopInGame(ClipState clipState)
        {
			if (clipState != ClipState.BeforeClip)
				return;
			
			//if ((Hud.Game.MapMode == MapMode.WaypointMap) || (Hud.Game.MapMode == MapMode.ActMap) || (Hud.Game.MapMode == MapMode.Map)) return;
			foreach (IPlayer player in Hud.Game.Players.Where(p => !p.IsMe && p.IsOnScreen))
			{
				if (GetImmunityStatus(player, out double duration, out double totaltime))
					Draw(player, duration, totaltime);
			}

			{ //to keep duration and totaltime scoped
				if (GetImmunityStatus(Hud.Game.Me, out double duration, out double totaltime))
				{
					Draw(Hud.Game.Me, duration, totaltime);
					IsImmune = true;
					ImmuneDuration = duration;
					ImmuneTotalTime = totaltime;
				}
				else
					IsImmune = false;
			}
		}
		
		public void OnRegister(MovableController mover)
		{
			//var layout = NotifyFont.GetText("000s");
			
			if (LabelDecorator.IsVisible(CountdownBar))
			{
				mover.CreateArea(
					this,
					"Countdown", //area name
					new RectangleF(Hud.Game.Me.ScreenCoordinate.X - CountdownBar.Width*0.5f, Hud.Game.Me.ScreenCoordinate.Y - 1080*0.1f, CountdownBar.Width, CountdownBar.Height), //Hud.Window.Size.Width*0.5f - CountdownBar.Width*0.5f //new RectangleF(pos.X - notify.Metrics.Width*0.5f, Hud.Game.Me.ScreenCoordinate.Y - Hud.Window.Size.Height*0.2f, w, h), //position + dimensions
					true, //ShowCountdown || ShowCountdownBar, //enabled at start?
					true, //save to config file?
					ResizeMode.On //resizable
				);
			}
		}

        //public void PaintTopInGame(ClipState clipState)
		//public void PaintWorld(WorldLayer layer)
		//public override void PaintFloater(float deltaX = 0, float deltaY = 0)
		public void PaintArea(MovableController mover, MovableArea area, float deltaX = 0, float deltaY = 0)
		{
			IScreenCoordinate pos = Hud.Game.Me.FloorCoordinate.ToScreenCoordinate();
			BarOffsetX = pos.X - (area.Rectangle.X + deltaX);
			BarOffsetY = pos.Y - (area.Rectangle.Y + deltaY);
			BarWidth = area.Rectangle.Width;
			BarHeight = area.Rectangle.Height;
		}
		
		public bool GetImmunityStatus(IPlayer player, out double duration, out double totaltime)
		{
			//IPlayer me = Hud.Game.Me;
			//IScreenCoordinate pos = player.FloorCoordinate.ToScreenCoordinate();
			//IScreenCoordinate pos2 = player.CollisionCoordinate.ToScreenCoordinate();
			
			duration = 0;
			totaltime = 0;
			
			if (player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_ActorInvulBuff.Sno)) //stasis
			{
				IBuff buff = player.Powers.GetBuff(Hud.Sno.SnoPowers.Generic_ActorInvulBuff.Sno);
				double remaining = buff.TimeLeftSeconds[0];
				if (duration < remaining)
				{
					duration = remaining;
					totaltime = buff.TimeElapsedSeconds[0] + remaining;
				}
			}
			
			if (player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_PagesBuffInvulnerable.Sno)) //Shield Pylon
			{
				IBuff buff = player.Powers.GetBuff(Hud.Sno.SnoPowers.Generic_PagesBuffInvulnerable.Sno);
				double remaining = buff.TimeLeftSeconds[0];
				if (duration < remaining)
				{
					duration = remaining;
					totaltime = buff.TimeElapsedSeconds[0] + remaining;
				}
			}
			
			//Templar's Guardian
			if (duration < 5 && player.Powers.BuffIsActive(485532, 2))
			{
				IBuff buff = player.Powers.GetBuff(485532);
				double remaining = buff.TimeLeftSeconds[2];
				if (duration < remaining)
				{
					duration = remaining;
					totaltime = buff.TimeElapsedSeconds[2] + remaining;
				}
			}
			
			//Scoundrel's Vanish
            if (duration < 7 && player.Powers.BuffIsActive(485336, 0))
            {
                IBuff buff = player.Powers.GetBuff(485336);
                double remaining = buff.TimeLeftSeconds[0];
                if (duration < remaining)
                {
                    duration = remaining;
                    totaltime = buff.TimeElapsedSeconds[0] + remaining;
                }
            }

			//Enchantress' Fate's Lapse
			if (duration < 1 && player.Powers.BuffIsActive(485530, 2))
            {
                IBuff buff = player.Powers.GetBuff(485530);
                double remaining = buff.TimeLeftSeconds[2];
                if (duration < remaining)
                {
                    duration = remaining;
                    totaltime = buff.TimeElapsedSeconds[2] + remaining;
                }
            }

			if (player.HeroClassDefinition.HeroClass == HeroClass.Monk)
			{
				if (duration < 4 && player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Monk_Serenity.Sno, 0))
				{
					IBuff buff = player.Powers.GetBuff(Hud.Sno.SnoPowers.Monk_Serenity.Sno);
					double remaining = buff.TimeLeftSeconds[0];
					if (duration < remaining)
					{
						duration = remaining;
						totaltime = buff.TimeElapsedSeconds[0] + remaining;
					}
				}
			
				if (duration < 2 && player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Monk_Passive_NearDeathExperience.Sno, 3))
				{
					IBuff buff = player.Powers.GetBuff(Hud.Sno.SnoPowers.Monk_Passive_NearDeathExperience.Sno);
					double remaining = buff.TimeLeftSeconds[3];
					if (duration < remaining)
					{
						duration = remaining;
						totaltime = buff.TimeElapsedSeconds[3] + remaining;
					}
				}
			}
			else if (player.HeroClassDefinition.HeroClass == HeroClass.DemonHunter)
			{
				//smoke screen
				if (duration < 1.5 && player.Powers.BuffIsActive(Hud.Sno.SnoPowers.DemonHunter_SmokeScreen.Sno))
				{
					IBuff buff = player.Powers.GetBuff(Hud.Sno.SnoPowers.DemonHunter_SmokeScreen.Sno);
					int index = buff.TimeLeftSeconds[2] > 0 ? 2 : 0;
					double remaining = buff.TimeLeftSeconds[index];
					if (duration < remaining)
					{
						duration = remaining;
						totaltime = buff.TimeElapsedSeconds[index] + remaining;
					}
				}
			}
			else if (player.HeroClassDefinition.HeroClass == HeroClass.Necromancer)
			{
				//interaction now nerfed out of existence
				//land of the dead: invigoration + crimson set 3-pc bonus
				/*if (duration < 10 && player.Defense.drCombined == 1f && player.Powers.BuffIsActive(483574))
				{
					if (player.Powers.UsedSkills.Any(s => s.SnoPower.Sno == Hud.Sno.SnoPowers.Necromancer_LandOfTheDead.Sno && s.Rune == 0))
					{
						if (RCRStartTick == 0)
							RCRStartTick = Hud.Game.CurrentGameTick;

						//evidence suggests that the effect is caused by LotD: Invigoration
						if (player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Necromancer_LandOfTheDead.Sno, 3) && LandStartTick == 0)
						{
							IBuff buff = player.Powers.GetBuff(Hud.Sno.SnoPowers.Necromancer_LandOfTheDead.Sno);
							LandStartTick = Hud.Game.CurrentGameTick - (int)(buff.TimeElapsedSeconds[0]*60);
						}
						
						if (RCRStartTick != 0 && LandStartTick != 0)
						{
							duration = (double)(RCRStartTick + 600 - Hud.Game.CurrentGameTick)/60d;
							totaltime = 10;
							
							//TextLayout notify = NotifyFont.GetTextLayout((RCRStartTick - LandStartTick).ToString());
							//NotifyFont.DrawText(notify, Hud.Window.CursorX, Hud.Window.CursorY - notify.Metrics.Height);
						}
					}
				}
				else
				{
					LandStartTick = 0;
					RCRStartTick = 0;
				}*/

				//bone armor: limited immunity
				if (duration < 5 && player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Necromancer_BoneArmor.Sno, 1))
				{
					IBuff buff = player.Powers.GetBuff(Hud.Sno.SnoPowers.Necromancer_BoneArmor.Sno);
					double remaining = buff.TimeLeftSeconds[1];
					if (duration < remaining)
					{
						duration = remaining;
						totaltime = buff.TimeElapsedSeconds[1] + remaining;
					}
				}

				//rathma's shield
				if (duration < 4 && player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Necromancer_Passive_RathmasShield.Sno, 1))
				{
					IBuff buff = player.Powers.GetBuff(Hud.Sno.SnoPowers.Necromancer_Passive_RathmasShield.Sno);
					double remaining = buff.TimeLeftSeconds[1];
					if (duration < remaining)
					{
						duration = remaining;
						totaltime = buff.TimeElapsedSeconds[1] + remaining;
					}
				}
					
				//final service secondary proc effect
				if (duration < 4 && player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Necromancer_Passive_FinalService.Sno, 2))
				{
					IBuff buff = player.Powers.GetBuff(Hud.Sno.SnoPowers.Necromancer_Passive_FinalService.Sno);
					double remaining = player.Powers.GetBuff(Hud.Sno.SnoPowers.Necromancer_Passive_FinalService.Sno).TimeLeftSeconds[2];
					if (duration < remaining)
					{
						duration = remaining;
						totaltime = buff.TimeElapsedSeconds[2] + remaining;
					}
				}
					

			}
			else if (player.HeroClassDefinition.HeroClass == HeroClass.WitchDoctor)
			{
				if (duration < 3 && player.Powers.BuffIsActive(Hud.Sno.SnoPowers.WitchDoctor_SpiritWalk.Sno))
				{
					//check if spirit walk mojo is equipped
					
					//otherwise
					IBuff buff = player.Powers.GetBuff(Hud.Sno.SnoPowers.WitchDoctor_SpiritWalk.Sno);
					double remaining = buff.TimeLeftSeconds[0];
					if (duration < remaining)
					{
						duration = remaining;
						totaltime = buff.TimeElapsedSeconds[0] + remaining;
					}
				}
				
				if (duration < 2 && player.Powers.BuffIsActive(Hud.Sno.SnoPowers.WitchDoctor_Passive_SpiritVessel.Sno, 2))
				{
					IBuff buff = player.Powers.GetBuff(Hud.Sno.SnoPowers.WitchDoctor_Passive_SpiritVessel.Sno);
					double remaining = buff.TimeLeftSeconds[2];
					if (duration < remaining)
					{
						duration = remaining;
						totaltime = buff.TimeElapsedSeconds[2] + remaining;
					}
				}
			}

			return duration > 0 || player.Powers.BuffIsActive(Hud.Sno.SnoPowers.Generic_InvulnerableDuringBuff.Sno) || player.Defense.drCombined == 1f; //InvulnerableDuringBuff (blood rush, teleport, vault immunity)
		}
		
		public void Draw(IPlayer player, double duration, double totaltime)
		{
			IScreenCoordinate pos = player.FloorCoordinate.ToScreenCoordinate(); //player.ScreenCoordinate;

			if ((player.IsMe && ShowIndicator) || (!player.IsMe && ShowIndicatorOthers))
			{
				float radius = player.RadiusBottom;
				float rotation = -45f;
				IWorldCoordinate center = player.FloorCoordinate;
				int i = 2;
				float sx = radius * (float)Math.Cos((i * 90 + rotation) * Math.PI / 180f);
				float sy = radius * (float)Math.Sin((i * 90 + rotation) * Math.PI / 180f);
				IScreenCoordinate sPos = Hud.Window.WorldToScreenCoordinate(center.X + sx, center.Y + sy, center.Z);
			
				//draw right parenthesis
				TextLayout layout = StyleFont.GetTextLayout(")");
				StyleFont.DrawText(layout, sPos.X, sPos.Y - layout.Metrics.Height*0.97f);
				
				//draw left parenthesis
				layout = StyleFont.GetTextLayout("(");
				StyleFont.DrawText(layout, pos.X - (sPos.X - pos.X) - layout.Metrics.Width, sPos.Y - layout.Metrics.Height*0.97f);
				
				//LabelDecorator.DebugWrite("()", pos.X, pos.Y);
			}
			
			if ((player.IsMe && ShowCountdownBar) || (!player.IsMe && ShowCountdownBarOthers))
			{
				var x = pos.X - BarOffsetX;
				var y = pos.Y - BarOffsetY;
				//CountdownBar.BarWidth = BarWidth;
				//CountdownBar.BarHeight = BarHeight;
				CountdownBar.Progress = (float)(duration == 0 || totaltime == 0 ? 1 : duration / totaltime);
				CountdownBar.Paint(x, y);
				
				//SkillBorderDark.DrawRectangle(xBar - 1, y - 1, w + 2, h + 2);
				SkillBorderLight.DrawRectangle(x - 2, y - 2, CountdownBar.Width + 4, CountdownBar.Height + 4);
				SkillBorderDark.DrawRectangle(x - 3, y - 3, CountdownBar.Width + 6, CountdownBar.Height + 6);
			}
			
			if (duration > 0)//(player.IsMe && ShowCountdown) || (!player.IsMe && ShowCountdownOthers))
			{
				if (player.IsMe && ShowCountdown)
				{
					var x = pos.X - BarOffsetX + CountdownBar.Width*0.5f;
					var y = pos.Y - BarOffsetY;
					TextLayout notify = NotifyFont.GetTextLayout(duration.ToString(duration < 1 ? "F1" : "F0")); //ImmunityText
					//TextLayout sLayout = SFont.GetTextLayout("s"); //ImmunityText
					
					//var xPos = x + CountdownBar.Width*0.5f - notify.Metrics.Width*0.5f;
					NotifyFont.DrawText(notify, x - notify.Metrics.Width*0.5f, y - notify.Metrics.Height - 3);
					//SFont.DrawText(sLayout, xPos + notify.Metrics.Width + 3, y - sLayout.Metrics.Height*1.15f - 4);
				}
				else if (!player.IsMe && ShowCountdownOthers)
				{
					var x = pos.X - BarOffsetX + CountdownBar.Width*0.5f;
					var y = pos.Y - BarOffsetY;
					TextLayout notify = SFont.GetTextLayout(duration.ToString(duration < 1 ? "F1" : "F0")); //ImmunityText
					//var xPos = x + CountdownBar.Width*0.5f - notify.Metrics.Width*0.5f;
					SFont.DrawText(notify, x - notify.Metrics.Width*0.5f, y - notify.Metrics.Height - 3);
				}
			}
		}
	}
}