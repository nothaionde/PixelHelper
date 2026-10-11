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
	public class OtherCoeBarSelectable : BasePlugin, ICustomizer, INewAreaHandler, IMovable	//, IInGameTopPainter
	{
		private Dictionary<HeroClass,List<int>> AllBonusElements { get; set; }
		private int[] IndexToBonus { get; set; } = new int[8] { 7, 5, 3, 1, 6, 2, 0, 4 };	// Index: 1 = Arcane, 2 = Cold, 3 = Fire, 4 = Holy, 5 = Lightning, 6 = Physical, 7 = Poison // index to bonus	{ -, 5, 3, 1, 6, 2, 0, 4 };
		private int[] BonusToIndex { get; set; } = new int[7] { 6, 3, 5, 2, 7, 1, 4 };		// Bonus: 0 = Physical, 1 = Fire, 2 = Lightning, 3 = Cold, 4 = Poison, 5 = Arcane, 6 = Holy // bonus to index	{ 6, 3, 5, 2, 7, 1, 4};

		private int[,] TicknBestElement { get; set; } = new int[,] { {0,0},{0,0},{0,0},{0,0} };   // tick rotation start , BestElement (last) element
		private int MyIndex { get; set; } = -1;
		private int TotalPlayers { get; set; }	= 0;

		private float SizeIconWidth  { get; set; } = 30f;
		private float SizeIconHeight  { get; set; } = 30f;
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
		private IBrush BrushRed { get; set; }
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
					FontBlue = Hud.Render.CreateFont("tahoma", 8f * _SizeMultiplier, 255, 50, 150, 250, true, false, 255, 0, 0, 0, true);
					FontBluem = Hud.Render.CreateFont("tahoma", 5f * _SizeMultiplier, 255, 50, 150, 250, true, false, 255, 0, 0, 0, true);
				}
			}
		}

		public enum ElementNoRune { None = 0, Arcane = 1, Cold = 2, Fire = 3, Holy = 4, Lightning = 5, Physical = 6, Poison = 7 }

		public delegate IPlayerSkill SkillFunc(IPlayer player);
		public Dictionary<SkillFunc, ElementNoRune> SkillDefaultElement { get; set; } = new Dictionary<SkillFunc, ElementNoRune>();

		public bool DisableDefaultCoe { get; set; } = true;
		public bool HideMyBar { get; set; }
		public bool MoveMyBar { get; set; }

		public float OffsetX  { get; set; }
		public float OffsetY  { get; set; }
		public float OffsetX_Me  { get; set; }
		public float OffsetY_Me  { get; set; }
		public float Opacity { get; set; }
		public bool ShowInactiveRed { get; set; }
		public bool GuessGROnly { get; set; }
		public int ProgressBarWidth { get; set; } = 2;
		public bool ShowBonusDamageInTown { get; set; }
		public bool SkillsWithJordanOnly { get; set; }

		private int Alerted { get; set; } = 0;
		private uint AlertHeroId100 { get; set; } = 0;
		private uint AlertHeroIdX { get; set; } = 0;
		public bool Alert { get; set; }
		public float AlertTime { get; set; }
		public string AlertFile { get; set; }
		public uint AlertLevelGR { get; set; }

		public OtherCoeBarSelectable()
		{
			Enabled = true;
		}

		public override void Load(IController hud)
        {
            base.Load(hud);
			Order = 30002;

			OffsetX = 0.047f;		// 0f..1f  To set the x coordinate of the bar (player.PortraitUiElement.Rectangle.X + Hud.Window.Size.Width * OffsetX)
			OffsetY = 0.071f;		// 0f..1f  To set the y coordinate of the bar (player.PortraitUiElement.Rectangle.Y + Hud.Window.Size.Height * OffsetY)
			HideMyBar = false;		// No CoeBar for Me
			MoveMyBar = false;		// Change position of My Bar (if HideMyBar = false)
			OffsetX_Me = 0.46f;		// 0f..1f  To set the x coordinate of the icon for Me (if MoveMyBar is true) (Hud.Window.Size.Width * OffsetX_Me)
			OffsetY_Me = 0.25f;		// 0f..1f  To set the y coordinate of the icon for Me (if MoveMyBar is true) (Hud.Window.Size.Height * OffsetY_Me)
			SizeMultiplier = 0.65f;	// 0f..1f  Size multiplier for icons
			Opacity = 1.0f;			// 0f..1f  Opacity for icon texture
			ShowInactiveRed = false; // inactive elements show as red (active with yellow frame)
			GuessGROnly = true;		// Guess only in GR the COE status of a remote player (Red Bar)
			ProgressBarWidth = 2;	// Width progressbar Coe (pixels)
			ShowBonusDamageInTown = false;	// Enable Displaying Elemental Damage Bonuses in Town
			SkillsWithJordanOnly = false;	// The configured skills will be applied only if you have the Jordan equipped

			Alert = false;						// Alert With Sound (only playing GR)
			AlertTime = 1.0f;					// 1.0f .. 8.0f , Alert with these seconds in advance
			AlertFile = "notification_4.wav";	// File to be played. It must be in the Sounds\ folder
			AlertLevelGR = 100;					// Alert only when you are playing a GR greater than (not equal to) this value

			BrushBlack = Hud.Render.CreateBrush(255, 80, 80, 80, 0);
			BrushGreen = Hud.Render.CreateBrush(255, 0, 255 , 0, 0);
			BrushYellow = Hud.Render.CreateBrush(255, 255, 255 , 0, 0);
			BrushOrange = Hud.Render.CreateBrush(255, 255, 185 , 0, 0);
			BrushBlue = Hud.Render.CreateBrush(255, 50, 150 , 250, 0);
			BrushRed = Hud.Render.CreateBrush(255, 255, 0 , 0, 0);
			BrushDefault = BrushBlack;
			TimeLeftClockBrush = Hud.Render.CreateBrush(220, 0, 0, 0, 0);

			AllBonusElements = new Dictionary<HeroClass, List<int>>	// 1 = Arcane, 2 = Cold, 3 = Fire, 4 = Holy, 5 = Lightning, 6 = Physical, 7 = Poison
			{
				{	HeroClass.Necromancer,	new List<int>{2,6,7}		},
				{	HeroClass.Barbarian,	new List<int>{2,3,5,6}		},
				{	HeroClass.DemonHunter,	new List<int>{2,3,5,6}		},
				{	HeroClass.WitchDoctor,	new List<int>{2,3,6,7}		},
				{	HeroClass.Wizard,		new List<int>{1,2,3,5}		},
				{	HeroClass.Crusader,		new List<int>{3,4,5,6}		},
				{	HeroClass.Monk,			new List<int>{2,3,4,5,6}	},
			};
		}

		public void Customize()
		{
			if (DisableDefaultCoe) { Hud.TogglePlugin<ConventionOfElementsBuffListPlugin>(false); }
		}

		public void OnNewArea(bool newGame, ISnoArea area)
		{
			if (newGame || (MyIndex != Hud.Game.Me.Index) )   // Fix partialment the newGame limitation
			{
				MyIndex = Hud.Game.Me.Index;
				TotalPlayers = Hud.Game.NumberOfPlayersInGame;
				for(var i = 0; i < 4; i++)	{ TicknBestElement[i,0] = 0;	TicknBestElement[i,1] = 0;	}
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

		public void PlayersIndex_Part()
		{
			for(int i = 0; i < 4; i++)
			{
				var part = true;
				foreach (IPlayer p in Hud.Game.Players)
				{
					if (p.Index == i)	{ part = false;	break; }
				}
				if (part)
				{
					TicknBestElement[i,0] = 0;	TicknBestElement[i,1] = 0;
				}
			}
		}

		public void OnRegister(MovableController mover)
		{
			if (this.Enabled == false) return;
			SizeIconWidth = Hud.Texture.BuffFrameTexture.Width   * Hud.Window.Size.Height/1200.0f * SizeMultiplier;
			SizeIconHeight = Hud.Texture.BuffFrameTexture.Height * Hud.Window.Size.Height/1200.0f * SizeMultiplier;
			var r = new System.Drawing.RectangleF(
				(MoveMyBar)? Hud.Window.Size.Width * OffsetX_Me : Hud.Game.Me.PortraitUiElement.Rectangle.X + Hud.Window.Size.Width * OffsetX ,
				(MoveMyBar)? Hud.Window.Size.Height * OffsetY_Me : Hud.Game.Me.PortraitUiElement.Rectangle.Y + Hud.Window.Size.Height * OffsetY,
				SizeIconWidth * 4,
				SizeIconHeight
			);
			mover.CreateArea(
				this,
				"BarOther", //area name
				r, //position + dimensions
				true, //enabled at start?
				true, //save changes to config file?
				ResizeMode.Off, //resizable?
				ClipState.BeforeClip
			);
		}

		public void PaintArea(MovableController mover, MovableArea area, float deltaX = 0, float deltaY = 0)
		{
			if (!Hud.Game.IsInGame) return;

			if (TotalPlayers != Hud.Game.NumberOfPlayersInGame)
			{
				PlayersIndex_Part();
				TotalPlayers = Hud.Game.NumberOfPlayersInGame;
			}

			var players = Hud.Game.Players.Where(p => (!p.IsMe || !HideMyBar) && p.Powers.GetBuff(Hud.Sno.SnoPowers.ConventionOfElements.Sno) != null );
			if (players.Any())
			{
				var AlertPlayer = (Alert && Hud.Game.Me.InGreaterRiftRank > AlertLevelGR)? (players.FirstOrDefault(pl => pl.InGreaterRift && pl.HeroId == (Hud.Game.RiftPercentage == 100 ? AlertHeroId100 :  AlertHeroIdX)) ?? Hud.Game.Me) : null;

				foreach(var player in players)
				{
					if (player.IsDeadSafeCheck)
					{
						if (TicknBestElement[player.Index,0] != 0) { TicknBestElement[player.Index,0] = 0;  TicknBestElement[player.Index,1] = 0; }
						continue;
					}

					float t; float x; float y;
					if (player.IsMe)
					{
						x = area.Rectangle.X + deltaX;
						y = area.Rectangle.Y + deltaY;
					}
					else
					{
						x = player.PortraitUiElement.Rectangle.X + Hud.Window.Size.Width * OffsetX;
						y = player.PortraitUiElement.Rectangle.Y + Hud.Window.Size.Height * OffsetY;
					}

					var BuffCoE = player.Powers.GetBuff(Hud.Sno.SnoPowers.ConventionOfElements.Sno); // 430674
					var HeroBonusElements = AllBonusElements[player.HeroClassDefinition.HeroClass];
					var BonusCount = HeroBonusElements.Count;

					if (player.HasValidActor)
					{
						if (BuffCoE.IconCounts[0] <= 0)
						{
							if (TicknBestElement[player.Index,0] != 0) { TicknBestElement[player.Index,0] = 0;  TicknBestElement[player.Index,1] = 0; }
							continue;
						}
						var ActiveElement = -1; var BestElement = -1;  double MaxBonus = -1; 	string ActiveLetter = string.Empty;
						for(var k = BonusCount - 1; k > -1 ; k--)
						{
							var index = HeroBonusElements[k];
							if (BuffCoE.IconCounts[index] > 0)	{	ActiveElement = index;  }

							var bonus = player.Offense.ElementalDamageBonus[IndexToBonus[index]];
							if (bonus > MaxBonus)
							{
								MaxBonus = bonus;
								BestElement = index;
								ActiveLetter = "B";
							}
							else if (bonus == MaxBonus) ActiveLetter = "";
						}
						if ((ActiveElement == -1)  || (BestElement == -1) )  continue;
						if (SelectElement.TryGetValue(player.HeroId, out var sindex) && sindex != int.MaxValue)
						{
							ActiveLetter = "M";
							BestElement = sindex;
						}
						else if (!SkillsWithJordanOnly || player.Powers.BuffIsActive(Hud.Sno.SnoPowers.StoneOfJordan.Sno)) // 484282 - P69_ItemPassive_Unique_Ring_001
						{
							foreach(var data in SkillDefaultElement)
							{
								var skill = data.Key(player);
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

						for (var l = 0; l < BonusCount; l++)
						{
							if (HeroBonusElements[BonusCount - 1] != BestElement)
							{
								HeroBonusElements.Insert(0,HeroBonusElements[BonusCount - 1]);
								HeroBonusElements.RemoveAt(BonusCount);
							}
							else  {   break;  }
						}
						foreach(var index in HeroBonusElements)
						{
							Hud.Texture.GetTexture(BuffCoE.SnoPower.Icons[index].TextureId).Draw(x, y, SizeIconWidth, SizeIconHeight, Opacity);
							double s = 0; bool best = false;
							if (index == ActiveElement)
							{
								s = BuffCoE.TimeLeftSeconds[index];
								if (!player.IsMe)
								{
									TicknBestElement[player.Index,0] = (int) (Hud.Game.CurrentGameTick - (HeroBonusElements.IndexOf(ActiveElement) * 4 + 4 - s) * 60);
									TicknBestElement[player.Index,1] = BestElement;
								}
								if (index == BestElement)
								{
									FontDefault = FontGreen; BrushDefault = BrushGreen;
									best = true;
								}
								else { FontDefault = FontGray; }
								DrawTimeLeftClock(new SharpDX.RectangleF(x, y , SizeIconWidth, SizeIconHeight), BuffCoE.TimeElapsedSeconds[index], BuffCoE.TimeLeftSeconds[index]);
								Hud.Texture.BuffFrameTexture.Draw(x, y, SizeIconWidth, SizeIconHeight, Opacity);
							}
							else
							{
								if (index == BestElement)
								{
									best = true;
									s = (BonusCount - HeroBonusElements.IndexOf(ActiveElement) - 1) * 4 - BuffCoE.TimeElapsedSeconds[ActiveElement] ;
									if (  s >  ((BonusCount - 1) * 2) ) { FontDefault = FontOrange;	BrushDefault = BrushOrange; }
									else { FontDefault = FontYellow; BrushDefault = BrushYellow; }
								}
								(ShowInactiveRed?Hud.Texture.DebuffFrameTexture:Hud.Texture.BuffFrameTexture).Draw(x, y, SizeIconWidth, SizeIconHeight, Opacity);
							}
							if (Hud.Window.CursorInsideRect(x, y, SizeIconWidth, SizeIconHeight) && Hud.Input.IsKeyDown(Keys.Menu) && (Hud.Game.CurrentRealTimeMilliseconds - LastClick > 1000))
							{
								if (Hud.Input.IsKeyDown(Keys.LButton))
								{
									if (Hud.Game.IsInTown)
									{
										SelectElement[player.HeroId] = (SelectElement.TryGetValue(player.HeroId,out var findex) && findex == index)? int.MaxValue : index;
										LastClick = Hud.Game.CurrentRealTimeMilliseconds;
										return;
									}
								}
								else if (Hud.Input.IsKeyDown(Keys.RButton))
								{
									if (Alert && Hud.Game.Me.InGreaterRiftRank > AlertLevelGR)
									{
										if (Hud.Game.RiftPercentage == 100) AlertHeroId100 = (AlertHeroId100 != player.HeroId && !player.IsMe) ? player.HeroId : 0;
										else AlertHeroIdX = (AlertHeroIdX != player.HeroId && !player.IsMe) ? player.HeroId : 0;
										LastClick = Hud.Game.CurrentRealTimeMilliseconds;
									}
								}
							}
							if (Hud.Game.IsInTown)
							{
								if (ShowBonusDamageInTown)
								{
									layout = FontBlue.GetTextLayout(Math.Round(player.Offense.ElementalDamageBonus[IndexToBonus[index]] * 100).ToString());
									FontBlue.DrawText(layout, x + (SizeIconWidth - (float)Math.Ceiling(layout.Metrics.Width))/6f , y + (SizeIconHeight - (float)Math.Ceiling(layout.Metrics.Height))/1.05f );
								}
								if (best)
								{
									layout = FontBlue.GetTextLayout(ActiveLetter);
									FontBlue.DrawText(layout, x + (SizeIconWidth - (float)Math.Ceiling(layout.Metrics.Width))/1.16f , y + (SizeIconHeight - (float)Math.Ceiling(layout.Metrics.Height))/1.05f );
								}
							}
							else
							{
								if (best)
								{
									if (player == AlertPlayer)
									{
										layout = FontBluem.GetTextLayout("🔈");
										FontBluem.DrawText(layout, x + (SizeIconWidth - (float)Math.Ceiling(layout.Metrics.Width))/1.10f , y + (SizeIconHeight - (float)Math.Ceiling(layout.Metrics.Height))/10f );
									}
								}
							}
							if (s > 0)
							{
								layout = FontDefault.GetTextLayout( (s < 1)? s.ToString("F1") : String.Format("{0:0}",(int) (s + 0.80)) ); // Redondeará a X si es menor  a X.20
								FontDefault.DrawText(layout, x + (SizeIconWidth - (float)Math.Ceiling(layout.Metrics.Width))/2.0f, y + (SizeIconHeight - (float)Math.Ceiling(layout.Metrics.Height))/2.50f);
							}
							x += SizeIconWidth;
						}
						t = (float) ((BonusCount - HeroBonusElements.IndexOf(ActiveElement)) * 4 - BuffCoE.TimeElapsedSeconds[ActiveElement]);
					}
					else
					{
						if (GuessGROnly && !Hud.Game.Me.InGreaterRift) continue;
						if (TicknBestElement[player.Index,0] == 0) continue;
						var BestElement = TicknBestElement[player.Index,1];
						for (var l = 0; l < BonusCount; l++)
						{
							if (HeroBonusElements[BonusCount - 1] != BestElement)
							{
								HeroBonusElements.Insert(0,HeroBonusElements[BonusCount - 1]);
								HeroBonusElements.RemoveAt(BonusCount);
							}
							else  {   break;  }
						}
						foreach(var index in HeroBonusElements)
						{
							Hud.Texture.GetTexture(BuffCoE.SnoPower.Icons[index].TextureId).Draw(x, y, SizeIconWidth, SizeIconHeight, Opacity);
							Hud.Texture.BuffFrameTexture.Draw(x, y, SizeIconWidth, SizeIconHeight, Opacity);
							x += SizeIconWidth;
						}
						BrushDefault = BrushRed;
						t = (float) (4 * BonusCount - ( (Hud.Game.CurrentGameTick - TicknBestElement[player.Index,0]) % (60 * 4 * BonusCount) ) / 60f);
					}
					BrushBlack.DrawRectangle(x , y, - SizeIconWidth * BonusCount , - ProgressBarWidth);
					if (t > 0)
					{
						BrushDefault.DrawRectangle(x , y, - (t * SizeIconWidth/4) , - ProgressBarWidth);
						if (player == AlertPlayer)
						{
							if ( Math.Abs(AlertTime + 4 - t) < 0.25 && Math.Abs(Hud.Game.CurrentGameTick - Alerted) > 120 )
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