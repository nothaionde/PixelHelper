// Versión adaptada a Movable plugins system: https://www.ownedcore.com/forums/showthread.php?t=908809&p=4231469#post4231469
using System;
using Turbo.Plugins.Default;
using System.Linq;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct2D1;
using System.Threading;
using Turbo.Plugins.Razor.Movable;
using System.Drawing;
using System.Windows.Forms;

namespace Turbo.Plugins.RNN
{
	public class MyCoeBarSelectable : BasePlugin, IMovable
	{
		private Dictionary<HeroClass,List<int>> AllBonusElements { get; set; }
		private int[] IndexToBonus { get; set; } = new int[8] { 7, 5, 3, 1, 6, 2, 0, 4 };	// Index: 1 = Arcane, 2 = Cold, 3 = Fire, 4 = Holy, 5 = Lightning, 6 = Physical, 7 = Poison // index to bonus	{ -, 5, 3, 1, 6, 2, 0, 4 };
		private int[] BonusToIndex { get; set; } = new int[7] { 6, 3, 5, 2, 7, 1, 4 };		// Bonus: 0 = Physical, 1 = Fire, 2 = Lightning, 3 = Cold, 4 = Poison, 5 = Arcane, 6 = Holy // bonus to index	{ 6, 3, 5, 2, 7, 1, 4};

		private float SizeIconWidth  { get; set; } = 0f;
		private float SizeIconHeight  { get; set; } = 0f;
		private SharpDX.DirectWrite.TextLayout layout { get; set; } = null;

		private IFont FontWhite { get; set; }
		private IFont FontGray { get; set; }
		private IFont FontGreen { get; set; }
		private IFont FontYellow { get; set; }
		private IFont FontOrange { get; set; }
		private IFont FontBlue { get; set; }
		private IFont FontBluem { get; set; }
		private IFont FontDefault { get; set; } = null;

		private IBrush TimeLeftClockBrush { get; set; }
		private IBrush BrushBlack { get; set; }
		private IBrush BrushGreen { get; set; }
		private IBrush BrushYellow { get; set; }
		private IBrush BrushOrange { get; set; }
		private IBrush BrushBlue { get; set; }
		private IBrush BrushDefault { get; set; }

		private long LastClick { get; set; } = 0;
		private Dictionary<uint,int> SelectElement { get; set; } = new Dictionary<uint,int>();
		private float _SizeMultiplier  { get; set; }

		public float SizeMultiplier
		{
			get { return _SizeMultiplier; }
			set
			{
				if (_SizeMultiplier != value)
				{
					_SizeMultiplier = value;

					FontWhite = Hud.Render.CreateFont("tahoma", 10f * _SizeMultiplier, 255, 255, 255, 255, true, false, 255, 0, 0, 0, true);
					FontGray = Hud.Render.CreateFont("tahoma", 10f * _SizeMultiplier, 235, 235, 235, 235, true, false, 255, 0, 0, 0, true);
					FontGreen = Hud.Render.CreateFont("tahoma", 13f * _SizeMultiplier, 255, 0, 255, 0, true, false, 255, 0, 0, 0, true);
					FontYellow = Hud.Render.CreateFont("tahoma", 13f * _SizeMultiplier, 255, 255, 255, 0, true, false, 255, 0, 0, 0, true);
					FontOrange = Hud.Render.CreateFont("tahoma", 13f * _SizeMultiplier, 255, 255, 150, 0, true, false, 255, 0, 0, 0, true);
					FontBlue = Hud.Render.CreateFont("tahoma", 7f * _SizeMultiplier, 255, 50, 150, 250, true, false, 255, 0, 0, 0, true);
					FontBluem = Hud.Render.CreateFont("tahoma", 5f * _SizeMultiplier, 255, 50, 150, 250, true, false, 255, 0, 0, 0, true);
				}
			}
		}

		public enum ElementNoRune { None = 0, Arcane = 1, Cold = 2, Fire = 3, Holy = 4, Lightning = 5, Physical = 6, Poison = 7 }

		public delegate IPlayerSkill SkillFunc(IPlayer player);
		public Dictionary<SkillFunc, ElementNoRune> SkillDefaultElement { get; set; } = new Dictionary<SkillFunc, ElementNoRune>();

		public float OffsetX_Me  { get; set; }
		public float OffsetY_Me  { get; set; }
		public float Opacity { get; set; }
		public bool ShowInactiveRed { get; set; }
		public int ProgressBarWidth { get; set; } = 2;
		public bool ShowBonusDamageInTown { get; set; }
		public bool SkillsWithJordanOnly { get; set; }

		private int Alerted { get; set; } = 0;
		public bool Alert { get; set; }
		public float AlertTime { get; set; }
		public string AlertFile { get; set; }
		public int AlertLevelGR { get; set; }

		public MyCoeBarSelectable()
		{
			Enabled = true;
		}

		public override void Load(IController hud)
        {
            base.Load(hud);
			Order = 30001;

			OffsetX_Me = 0.53f;		// 0f..1f  To set the x coordinate of the bar (Hud.Window.Size.Width * OffsetX_Me)
			OffsetY_Me = 0.46f;		// 0f..1f  To set the y coordinate of the bar (Hud.Window.Size.Height * OffsetY_Me)
			SizeMultiplier = 0.75f;	// 0f..1f  Size multiplier for icons
			Opacity = 0.75f;			// 0f..1f  Opacity for icon texture
			ShowInactiveRed = true; // inactive elements show as red (active with yellow frame)
			ProgressBarWidth = 2;	// Width progressbar Coe (pixels)
			ShowBonusDamageInTown = false;	// Enable Displaying Elemental Damage Bonuses in Town
			SkillsWithJordanOnly = false;	// The configured skills will be applied only if you have the Jordan equipped

			Alert = false;						// Alert With Sound (only playing GR)
			AlertTime = 1.0f;					// 1.0f .. 8.0f , Alert with these seconds in advance
			AlertFile = "notification_4.wav";	// File to be played. It must be in the Sounds\ folder
			AlertLevelGR = 119;					// Alert only when you are playing a GR greater than (not equal to) this value

			BrushBlack = Hud.Render.CreateBrush(255, 80, 80, 80, 0);
			BrushGreen = Hud.Render.CreateBrush(255, 0, 255 , 0, 0);
			BrushYellow = Hud.Render.CreateBrush(255, 255, 255 , 0, 0);
			BrushOrange = Hud.Render.CreateBrush(255, 255, 185 , 0, 0);
			BrushBlue = Hud.Render.CreateBrush(255, 50, 150 , 250, 0);
			BrushDefault = BrushBlack;
			TimeLeftClockBrush = Hud.Render.CreateBrush(220, 0, 0, 0, 0);

			AllBonusElements = new Dictionary<HeroClass, List<int>>	// 1 = Arcane, 2 = Cold, 3 = Fire, 4 = Holy, 5 = Lightning, 6 = Physical, 7 = Poison
			{
				{	HeroClass.Necromancer,	new List<int>{2,6,7}		},
				{	HeroClass.Barbarian,	new List<int>{2,3,5,6}		},
				{	HeroClass.DemonHunter,	new List<int>{2,3,5,6}		},
				{	HeroClass.Wizard,		new List<int>{1,2,3,5}		},
				{	HeroClass.WitchDoctor,	new List<int>{2,3,6,7}		},
				{	HeroClass.Crusader,		new List<int>{3,4,5,6}		},
				{	HeroClass.Monk,			new List<int>{2,3,4,5,6}	},
			};
		}

        private void DrawTimeLeftClock(SharpDX.RectangleF rect, double elapsed, double timeLeft)	// plugins\Default\BuffLists\Painter\BuffPainter.cs
        {
            if ((timeLeft > 0) && (elapsed >= 0) && (TimeLeftClockBrush != null))
            {
                var endAngle = Convert.ToInt32(360.0d / (timeLeft + elapsed) * elapsed);
                var startAngle = 0;
                TimeLeftClockBrush.Opacity = 1 - (float)(0.4f / (timeLeft + elapsed) * elapsed);
                var rad = rect.Width * 0.45f;
                using (var pg = Hud.Render.CreateGeometry())
                {
                    using (var gs = pg.Open())
                    {
                        gs.BeginFigure(rect.Center, FigureBegin.Filled);
                        for (var angle = startAngle; angle <= endAngle; angle++)
                        {
                            var mx = rad * (float)Math.Cos((angle - 90) * Math.PI / 180.0f);
                            var my = rad * (float)Math.Sin((angle - 90) * Math.PI / 180.0f);
                            var vec = new Vector2(rect.Center.X + mx, rect.Center.Y + my);
                            gs.AddLine(vec);
                        }
                        gs.EndFigure(FigureEnd.Closed);
                        gs.Close();
                    }
                    TimeLeftClockBrush.DrawGeometry(pg);
                }
            }
        }

        public void Play_Sound(string Sonido)
        {
			var playSonido = Hud.Sound.LoadSoundPlayer(Sonido);
			ThreadPool.QueueUserWorkItem(state =>
			{
				try  { playSonido.PlaySync(); }
				catch (Exception)  {   }
			}	);
		}
		public void OnRegister(MovableController mover)
		{
			if (this.Enabled == false) return;
			SizeIconWidth = Hud.Texture.BuffFrameTexture.Width   * Hud.Window.Size.Height/1200.0f * SizeMultiplier;
			SizeIconHeight = Hud.Texture.BuffFrameTexture.Height * Hud.Window.Size.Height/1200.0f * SizeMultiplier;
			mover.CreateArea(
				this,
				"Bar", //area name
				new System.Drawing.RectangleF(Hud.Window.Size.Width * OffsetX_Me, Hud.Window.Size.Height * OffsetY_Me, SizeIconWidth * 4, SizeIconHeight), //position + dimensions
				true, //enabled at start?
				true, //save changes to config file?
				ResizeMode.Off, //resizable?
				ClipState.BeforeClip
			);
		}

		public void PaintArea(MovableController mover, MovableArea area, float deltaX = 0, float deltaY = 0)
		{
			if (!Hud.Game.IsInGame) return;

			var BuffCoE = Hud.Game.Me.Powers.GetBuff(Hud.Sno.SnoPowers.ConventionOfElements.Sno); // 430674
			if ((BuffCoE != null) && (BuffCoE.IconCounts[0] > 0))
			{
				var HeroBonusElements = AllBonusElements[Hud.Game.Me.HeroClassDefinition.HeroClass];

				var j =	HeroBonusElements.Count; var ActiveElement = -1; var BestElement = -1; double MaxBonus = -1; string ActiveLetter = string.Empty;
				for(var k = j - 1; k > -1 ; k--)
				{
					var index = HeroBonusElements[k];
					if (BuffCoE.IconCounts[index] > 0)	{	ActiveElement = index;  }

					var bonus = Hud.Game.Me.Offense.ElementalDamageBonus[IndexToBonus[index]];
					if (bonus > MaxBonus)
					{
						MaxBonus = bonus;
						BestElement = index;
						ActiveLetter = "B";
					}
					else if (bonus == MaxBonus) ActiveLetter = "X";
				}

				if ( (ActiveElement != -1) && (BestElement != -1) )
				{
					if (SelectElement.TryGetValue(Hud.Game.Me.HeroId,out var sindex) && sindex != int.MaxValue)
					{
						ActiveLetter = "M";
						BestElement = sindex;
					}
					else if (!SkillsWithJordanOnly || Hud.Game.Me.Powers.BuffIsActive(Hud.Sno.SnoPowers.StoneOfJordan.Sno)) // 484282 - P69_ItemPassive_Unique_Ring_001
					{
						foreach(var data in SkillDefaultElement)
						{
							var skill = data.Key(Hud.Game.Me);
							if (skill != null)
							{
								if (skill.Rune != 255 || HeroBonusElements.Contains((int) data.Value))
								{
									BestElement = (skill.Rune == 255)? (int) data.Value : BonusToIndex[skill.ElementalType];
									ActiveLetter = "S";
								}
								else if (data.Value == ElementNoRune.None) continue; // None (continue) o bad index (break)
								break;
							}
						}
					}
					for (var l = 0; l < j; l++)
					{
						if (HeroBonusElements[j - 1] != BestElement)
						{
							HeroBonusElements.Insert(0,HeroBonusElements[j - 1]);
							HeroBonusElements.RemoveAt(j);
						}
						else  {   break;  }
					}

					float x = area.Rectangle.X + deltaX;	float y = area.Rectangle.Y + deltaY;
					foreach(var index in HeroBonusElements)
					{
						Hud.Texture.GetTexture(BuffCoE.SnoPower.Icons[index].TextureId).Draw(x, y, SizeIconWidth, SizeIconHeight, Opacity);
						double s = 0; bool best = false;
						if (index == ActiveElement)
						{
							s = BuffCoE.TimeLeftSeconds[index];
							if (index == BestElement) { FontDefault = FontGreen; BrushDefault = BrushGreen;  best = true; }
							else { FontDefault = FontGray; }
							DrawTimeLeftClock(new SharpDX.RectangleF(x, y , SizeIconWidth, SizeIconHeight), BuffCoE.TimeElapsedSeconds[index], BuffCoE.TimeLeftSeconds[index]);
							Hud.Texture.BuffFrameTexture.Draw(x, y, SizeIconWidth, SizeIconHeight, Opacity);
						}
						else
						{
							if (index == BestElement)
							{
								best = true;
								s = (j - HeroBonusElements.IndexOf(ActiveElement) - 1) * 4 - BuffCoE.TimeElapsedSeconds[ActiveElement] ;
								if (  s >  ((j - 1) * 2) ) { FontDefault = FontOrange;	BrushDefault = BrushOrange; }
								else { FontDefault = FontYellow; BrushDefault = BrushYellow; }
							}
							(ShowInactiveRed?Hud.Texture.DebuffFrameTexture:Hud.Texture.BuffFrameTexture).Draw(x, y, SizeIconWidth, SizeIconHeight, Opacity);
						}
						if (Hud.Game.IsInTown)
						{
							if (Hud.Window.CursorInsideRect(x, y, SizeIconWidth, SizeIconHeight))
							{
								if (Hud.Input.IsKeyDown(Keys.Menu) && Hud.Input.IsKeyDown(Keys.LButton) && (Hud.Game.CurrentRealTimeMilliseconds - LastClick > 1000))
								{
									SelectElement[Hud.Game.Me.HeroId] = (SelectElement.TryGetValue(Hud.Game.Me.HeroId,out var findex) && findex == index)? int.MaxValue : index;
									LastClick = Hud.Game.CurrentRealTimeMilliseconds;
									return;
								}
							}
							if (ShowBonusDamageInTown)
							{
								layout = FontBlue.GetTextLayout(Math.Round(Hud.Game.Me.Offense.ElementalDamageBonus[IndexToBonus[index]] * 100).ToString());
								FontBlue.DrawText(layout, x + (SizeIconWidth - (float)Math.Ceiling(layout.Metrics.Width))/6f , y + (SizeIconHeight - (float)Math.Ceiling(layout.Metrics.Height))/1.08f );
							}
							if (best)
							{
								layout = FontBlue.GetTextLayout(ActiveLetter);
								FontBlue.DrawText(layout, x + (SizeIconWidth - (float)Math.Ceiling(layout.Metrics.Width))/1.16f , y + (SizeIconHeight - (float)Math.Ceiling(layout.Metrics.Height))/1.08f );
							}
						}
						else
						{
							if (best)
							{
								if (Alert && Hud.Game.Me.InGreaterRiftRank > AlertLevelGR)
								{
									layout = FontBluem.GetTextLayout("🔈");
									FontBluem.DrawText(layout, x + (SizeIconWidth - (float)Math.Ceiling(layout.Metrics.Width))/8f , y + (SizeIconHeight - (float)Math.Ceiling(layout.Metrics.Height))/10f );
								}
							}
						}
						if (s > 0)
						{
							layout = FontDefault.GetTextLayout( (s < 1)? s.ToString("F1") : String.Format("{0:0}",(int) (s + 0.80)) ); // Redondeará a X si es menor  a X.20
							FontDefault.DrawText(layout, x + (SizeIconWidth - (float)Math.Ceiling(layout.Metrics.Width))/2.0f, y + (SizeIconHeight - (float)Math.Ceiling(layout.Metrics.Height))/2.25f);
						}
						x += SizeIconWidth;
					}
					BrushBlack.DrawRectangle(x , y, - SizeIconWidth * j , - ProgressBarWidth);
					var t = (float) ((j - HeroBonusElements.IndexOf(ActiveElement)) * 4 - BuffCoE.TimeElapsedSeconds[ActiveElement]);
					if (t > 0)
					{
						BrushDefault.DrawRectangle(x, y, - (t * SizeIconWidth/4) , - ProgressBarWidth);
						if (Alert && Hud.Game.Me.InGreaterRiftRank > AlertLevelGR)
						{
							if ( Math.Abs(Hud.Game.CurrentGameTick - Alerted) > 120 && Math.Abs(AlertTime + 4 - t) < 0.25 )
							{
								var quest = Hud.Game.Quests.FirstOrDefault(q => q.SnoQuest.Sno == 337492);
								if (quest != null && (quest.QuestStepId == 13 || quest.QuestStepId == 16))
								{
									Alerted = Hud.Game.CurrentGameTick;
									Play_Sound(AlertFile);
								}
							}
						}
					}
				}
			}
		}
	}
}