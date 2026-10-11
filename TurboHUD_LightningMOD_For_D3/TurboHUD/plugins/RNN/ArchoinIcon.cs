using Turbo.Plugins.Default;
using System;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;

namespace Turbo.Plugins.RNN
{
	public class ArchoinIcon : BasePlugin, IInGameTopPainter, INewAreaHandler
	{
		private SharpDX.DirectWrite.TextLayout layout { get; set; } = null;

		private IFont FontTimeLeft { get; set; }
		private IFont FontCooldown { get; set; }
		private IFont FontStacks { get; set; }

		private float _SizeMultiplier  { get; set; }
		private int MyIndex { get; set; } = -1;

		public float OffsetX { get; set; }
		public float OffsetY { get; set; }
		public float Opacity { get; set; }
		public float OpacityNoBuff { get; set; } = 0.25f;
		public bool EnableBuffsFar { get; set; }
		public bool TimeLeftClockBorder { get; set; }

		private int[,] DataBuffTick { get; set; } = new int[,] { {0,0,0},{0,0,0},{0,0,0},{0,0,0} };   // { index buff (0, 2 = archon or 6 = swami) , end tick buff archon (for index 2) , end tick buff Swami (if equipped) }

		public float SizeMultiplier
		{
			get { return _SizeMultiplier; }
			set
			{
				if (_SizeMultiplier != value)
				{
					_SizeMultiplier = value;
					FontTimeLeft  = Hud.Render.CreateFont("Tahoma", 9f * _SizeMultiplier, 255, 0, 255, 0, true, false, 255, 0, 0, 0, true);
					FontCooldown = Hud.Render.CreateFont("Tahoma", 9f * _SizeMultiplier, 255, 255, 255, 255, true, false, 255, 0, 0, 0, true);
					FontStacks = Hud.Render.CreateFont("Tahoma", 7.5f * _SizeMultiplier, 255, 255, 255, 0, false, false, 255, 0, 0, 0, true);
				}
			}
		}

        private IBrush TimeLeftClockBrush { get; set; }
		private IBrush BrushArchon { get; set; }
		private IBrush BrushSwami { get; set; }

		public ArchoinIcon()
		{
			Enabled = true;
		}

		public override void Load(IController hud)
		{
			base.Load(hud);
			Order = 91000;

			OffsetX = 3.0f;				// To set the x coordinate of the icon (OffsetYX * Portrait Width)
			OffsetY = 0.47f;			// To set the y coordinate of the icon (OffsetYX * Portrait Height)
			Opacity = 0.75f;			// 0f..1f  Opacity for icon texture
			SizeMultiplier = 0.85f;		// Size multiplier for icons
			EnableBuffsFar = true;		// Show Archon and Swami Buffs when the player is far
			TimeLeftClockBorder = true;

            TimeLeftClockBrush = Hud.Render.CreateBrush(220, 0, 0, 0, 0);
			BrushArchon = Hud.Render.CreateBrush(255, 115, 245, 255, 2);
			BrushSwami = Hud.Render.CreateBrush(255, 178, 102, 255, 2);
		}

		public void OnNewArea(bool newGame, ISnoArea area)
		{
			if (newGame || (MyIndex != Hud.Game.Me.Index) )   // Fix partialment the newGame limitation
			{
				MyIndex = Hud.Game.Me.Index;
				for (var i = 0; i < 4; i++)
				{
					DataBuffTick[i,0] = 0;
					DataBuffTick[i,1] = 0;
					DataBuffTick[i,2] = 0;
				}
			}
		}

		private void DrawTimeLeftClockBorder(float x, float y, float width, float height , double elapsed, double timeLeft, IBrush brushIcon) // From PartyCoe by RazorFish . Modified
		{
			if ((timeLeft > 0) && (elapsed >= 0) && (TimeLeftClockBrush != null) && (brushIcon != null))
			{
				RectangleF rect = new SharpDX.RectangleF(x + brushIcon.StrokeWidth , y + brushIcon.StrokeWidth, width  - 2 * brushIcon.StrokeWidth, height - 2 * brushIcon.StrokeWidth);
				var endAngle = Convert.ToInt32(360.0d / (timeLeft + elapsed) * elapsed);
				TimeLeftClockBrush.Opacity = 0.8f - (float)(0.5f / (timeLeft + elapsed) * elapsed);
				using (var pg = Hud.Render.CreateGeometry())
				{
					using (var gs = pg.Open())
					{
						gs.BeginFigure(rect.Center, FigureBegin.Filled);
						for (var angle = 0; angle <= endAngle; angle++)
						{
							Vector2 vec;
							if (angle < 45) //right top half
								vec = new Vector2(rect.Center.X + ((rect.Width*0.5f)/45f)*angle, rect.Y);
							else if (angle < 135) //right
								vec = new Vector2(rect.Right, rect.Y + (rect.Height/90f)*(angle - 45));
							else if (angle < 225) //bottom
								vec = new Vector2(rect.Right - (rect.Width/90f)*(angle - 135), rect.Bottom);
							else if (angle < 315) //left
								vec = new Vector2(rect.X, rect.Bottom - (rect.Height/90f)*(angle - 225));
							else //left top half
								vec = new Vector2(rect.X + ((rect.Width*0.5f)/45f)*(angle - 315), rect.Y);
							gs.AddLine(vec);
						}
						gs.EndFigure(FigureEnd.Closed);
						gs.Close();
					}
					TimeLeftClockBrush.DrawGeometry(pg);
					if (endAngle < 360) //left top half
						brushIcon.DrawLine(rect.X + ((rect.Width*0.5f)/45f)*(float)Math.Max(0, endAngle - 315), rect.Y, rect.Center.X, rect.Y);
					if (endAngle < 315) //left
						brushIcon.DrawLine(rect.X, rect.Y, rect.X, rect.Bottom - (rect.Height/90f)*(float)Math.Max(0, endAngle - 225));
					if (endAngle < 225) //bottom
						brushIcon.DrawLine(rect.X, rect.Bottom, rect.Right - (rect.Width/90f)*(float)Math.Max(0, endAngle - 135), rect.Bottom);
					if (endAngle < 135) //right
						brushIcon.DrawLine(rect.Right, rect.Y + (rect.Height/90f)*(float)Math.Max(0, endAngle - 45), rect.Right, rect.Bottom);
					if (endAngle < 45) //right top half
						brushIcon.DrawLine(rect.Center.X + ((rect.Width*0.5f)/45f)*endAngle, rect.Y, rect.Right, rect.Y);
				}
			}
		}

        public void PaintTopInGame(ClipState clipState)
        {
			if (clipState != ClipState.BeforeClip) return;
			if (!Hud.Game.IsInGame) return;

			var players = Hud.Game.Players.Where(p => p.HeroClassDefinition.HeroClass == HeroClass.Wizard);
			foreach(var player in players)
			{
				var skill = player.Powers.UsedSkills.FirstOrDefault(s => s.SnoPower.Sno == 134872);
				if (skill != null)
				{
					var x = player.PortraitUiElement.Rectangle.X + player.PortraitUiElement.Rectangle.Width * OffsetX;				var y = player.PortraitUiElement.Rectangle.Y + player.PortraitUiElement.Rectangle.Height * OffsetY;
					var SizeIconWidth = Hud.Texture.BuffFrameTexture.Width * Hud.Window.Size.Height/1200.0f * _SizeMultiplier;		var SizeIconHeight = Hud.Texture.BuffFrameTexture.Height * Hud.Window.Size.Height/1200.0f * _SizeMultiplier;
					if (skill.Buff == null || player.IsDead)
					{
						if (DataBuffTick[player.Index,0] != 0)  DataBuffTick[player.Index,0] = 0;
						Hud.Texture.GetTexture(3123915539).Draw(x, y, SizeIconWidth, SizeIconHeight, OpacityNoBuff);
						Hud.Texture.DebuffFrameTexture.Draw(x, y, SizeIconWidth, SizeIconHeight, OpacityNoBuff);
					}
					else
					{
						double timeleft = 0f; string stacks = string.Empty; bool swami = false;
						if (player.HasValidActor)
						{
							if (skill.Buff.IconCounts[6] > 0)
							{
								timeleft = skill.Buff.TimeLeftSeconds[5];
								stacks = skill.Buff.IconCounts[5].ToString();
								Hud.Texture.InventoryLegendaryBackgroundLarge.Draw(x,y,SizeIconWidth,SizeIconHeight,Opacity);
								Hud.Texture.GetItemTexture(Hud.Sno.SnoItems.P3_Unique_WizardHat_003).Draw(x + SizeIconWidth/8, y - SizeIconHeight/4, SizeIconWidth * 0.75f, SizeIconHeight * 1.70f, Opacity);
								if (!player.IsMe)
								{
									DataBuffTick[player.Index,0] = 6;
									DataBuffTick[player.Index,2] = (int) (Hud.Game.CurrentGameTick + timeleft * 60);
								}
								swami = true;
							}
							else
							{
								if (skill.Buff.IconCounts[2] > 0)
								{
									timeleft = skill.Buff.TimeLeftSeconds[2];
									stacks = (skill.Buff.IconCounts[4] + skill.Buff.IconCounts[5]).ToString();
									if (!player.IsMe)
									{
										DataBuffTick[player.Index,0] = 2;
										DataBuffTick[player.Index,1] = (int) (Hud.Game.CurrentGameTick + timeleft * 60);

										//DataBuffTick[player.Index,2] = player.Powers.BuffIsActive(440336)?(DataBuffTick[player.Index,1] + 20 * 60):0;		// No Siempre es de valor 20 .. esto no sirve
										if (player.Powers.BuffIsActive(440336))		// check if swami is equipped
										{
											DataBuffTick[player.Index,2] = DataBuffTick[player.Index,1] + (int) ( player.GetAttributeValue(Hud.Sno.Attributes.Item_Power_Passive,440336) * 60 );
										}
										else DataBuffTick[player.Index,2] = 0;
									}
								}
								else
								{
									if (!player.IsMe) DataBuffTick[player.Index,0] = 0;
								}
								Hud.Texture.GetTexture(skill.SnoPower.NormalIconTextureId).Draw(x, y, SizeIconWidth, SizeIconHeight, Opacity);
							}
							Hud.Texture.BuffFrameTexture.Draw(x, y, SizeIconWidth, SizeIconHeight, Opacity);
						}
						else
						{
							if (DataBuffTick[player.Index,0] == 6) // Swuami
							{
								var LeftTicks = DataBuffTick[player.Index,2] - Hud.Game.CurrentGameTick;
								if (LeftTicks > 0 && LeftTicks < 60 * 21)
								{
									timeleft = LeftTicks/60d;
									stacks = "?";
									Hud.Texture.InventoryLegendaryBackgroundLarge.Draw(x,y,SizeIconWidth,SizeIconHeight,Opacity);
									Hud.Texture.GetItemTexture(Hud.Sno.SnoItems.P3_Unique_WizardHat_003).Draw(x + SizeIconWidth/8, y - SizeIconHeight/4, SizeIconWidth * 0.75f, SizeIconHeight * 1.70f, Opacity);
									swami = true;
								}
								else
								{
									DataBuffTick[player.Index,0] = 0;
									Hud.Texture.GetTexture(skill.SnoPower.NormalIconTextureId).Draw(x, y, SizeIconWidth, SizeIconHeight, Opacity);
								}
							}
							else
							{
								if (DataBuffTick[player.Index,0] == 2)	// Archon
								{
									var LeftTicks = DataBuffTick[player.Index,1] - Hud.Game.CurrentGameTick;
									if (LeftTicks > 0 && LeftTicks < 60 * 21)
									{
										timeleft = LeftTicks/60d;
										stacks = "?";
									}
									else // End Archon => swami ?
									{
										DataBuffTick[player.Index,0] = (DataBuffTick[player.Index,2] != 0)? 6 : 0;
									}
								}
								Hud.Texture.GetTexture(skill.SnoPower.NormalIconTextureId).Draw(x, y, SizeIconWidth, SizeIconHeight, Opacity);
							}
							Hud.Texture.DebuffFrameTexture.Draw(x, y, SizeIconWidth, SizeIconHeight, Opacity);

						}
						if (EnableBuffsFar || player.HasValidActor)
						{
							if (timeleft > 0)
							{
								if (TimeLeftClockBorder)	DrawTimeLeftClockBorder(x, y, SizeIconWidth, SizeIconHeight , 20 - timeleft, timeleft, swami?BrushSwami:BrushArchon);
								layout = FontTimeLeft.GetTextLayout((timeleft < 1.0f)?String.Format("{0:N1}", timeleft): String.Format("{0:0}",  (int) (timeleft + 0.80)));
								FontTimeLeft.DrawText(layout, x + ((SizeIconWidth - (float)Math.Ceiling(layout.Metrics.Width))/2.0f), y + ((SizeIconHeight - (float)Math.Ceiling(layout.Metrics.Height))/12.0f) );
							}
							if (stacks != string.Empty)
							{
								layout = FontStacks.GetTextLayout(stacks.ToString());
								FontStacks.DrawText(layout, x + ((SizeIconWidth - (float)Math.Ceiling(layout.Metrics.Width))/2.0f), y + ((SizeIconHeight - (float)Math.Ceiling(layout.Metrics.Height))/2.0f) );
							}
						}
					}
					if (skill.CooldownFinishTick > Hud.Game.CurrentGameTick)
					{
						double remaining = (skill.CooldownFinishTick - Hud.Game.CurrentGameTick)/60.0d;
						layout = FontCooldown.GetTextLayout( remaining.ToString( (remaining < 1)? "F1":"F0" ) );
						FontCooldown.DrawText(layout, x + ((SizeIconWidth - (float)Math.Ceiling(layout.Metrics.Width))/2.0f), y + ((SizeIconHeight - (float)Math.Ceiling(layout.Metrics.Height))/1.1f) );
					}
				}
			}
		}
	}
}