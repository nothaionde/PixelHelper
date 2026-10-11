using System;
using System.Linq;
using System.Collections.Generic;
using Turbo.Plugins.Default;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectInput;

namespace Turbo.Plugins.RNN
{
    public class ShadowCloneS22 : BasePlugin, INewAreaHandler, IMonsterKilledHandler, IAfterCollectHandler, IInGameTopPainter
    {
		public bool Debug { get; set; } = false;
		private Dictionary<uint,CloneInfo> CloneCreated { get;set; } = new Dictionary<uint,CloneInfo>();
		private Dictionary<ActorSnoEnum, Tuple<HeroClass,bool>> CloneSno { get; set; }
		private List<ActorSnoEnum> PlayerSnoEnumMale { get; set; }
		public Dictionary<HeroClass, IBrush> HeroBrush { get; set; }
		public Dictionary<HeroClass, string[]> HeroClonNames { get; set; }

		private SharpDX.DirectWrite.TextLayout layout { get; set; } = null;
		private int MyIndex { get; set; } = -1;
		private const int duration = 60;
		private bool InRift = false;
		private float sizeFontBar  { get; set; } = 7f;

		private IBrush BrushBorder { get; set; }
		private IBrush BrushBorderMe { get; set; }
		private IBrush BrushBorderOther { get; set; }
		private IBrush BrushBorderMapMe { get; set; }
		private IBrush BrushBorderMapOther { get; set; }
		private IBrush DefaultBrush { get; set; }
		private IBrush BrushCounter { get; set; }
        private IBrush TimeLeftClockBrush { get; set; }
		private IBrush TimerHigh { get; set; }
		private IBrush TimerLow { get; set; }
		private IBrush TimerBg { get; set; }
		private IBrush SkillBorderLight { get; set; }
		private IBrush SkillBorderDark { get; set; }

		private IFont FontText { get; set; }
		private IFont FontTextm { get; set; }
		private IFont DefaultFont { get; set; }
		private IFont FontName { get; set; }
		private IFont FontNameMe { get; set; }
		private IFont FontNear { get; set; }
		private IFont FontTimer { get; set; }
		private IFont FontTimerG { get; set; }
		private IFont FontTimerR { get; set; }

		private IFont HeroFont { get; set; }

        public float xPor { get; set; }
        public float yPor { get; set; }

		public bool ShowGroundOther { get; set; }
		public bool ShowMapOther { get; set; }
		public bool ShowBarOther { get; set; }
		public bool ShowBuildNumberBar { get; set; } = true;

		public float BarWidth { get; set; } = 95f;	// color bar
		public float BarHeight { get; set; } = 7f;	// color bar
		public string SymbolWarning { get; set; } = "⚠";	// ⚠☹️😕☹

		public int radius_countdown { get; set; } = 18;

		public string NameBuildDefault { get; set; } = "НетИмени";

		public class CloneInfo
		{
			public int CreatedAtInGameTick { get; set; } = 0;
			public bool Rift { get; set; } = false;
			public HeroClass HeroClass { get; set; } = HeroClass.None;
			public string PlayerName { get; set; } = string.Empty;
			public bool MyClone { get; set; } = false;
			public string BuildName { get; set; } = string.Empty;
			public int BuildConfig { get; set; } = 0;
			public bool Sighted { get; set; } = true;
			public bool Alive { get; set; } = true;
			public bool? SameClassGenre { get; set; } = null;

			public CloneInfo() {}
		}

        public ShadowCloneS22()
        {
            Enabled = true;
        }

        public override void Load(IController hud)
        {
            base.Load(hud);

			xPor = 0.20f; // 0.74f;
			yPor = 0.02f; // 0.04f;

			ShowGroundOther = true;
			ShowMapOther = true;
			ShowBarOther = true;

			HeroFont = Hud.Render.CreateFont("tahoma", 6f, 255, 0, 168, 255, false, false, 128, 0, 0, 0, true);

			BrushBorder = hud.Render.CreateBrush(255, 0, 0, 0, 1);

			BrushBorderMe = hud.Render.CreateBrush(250, 255, 255, 255, 2);
			BrushBorderOther = hud.Render.CreateBrush(250, 180, 180, 180, 2);

			BrushBorderMapMe = hud.Render.CreateBrush(250, 255, 255, 255, 1);
			BrushBorderMapOther = hud.Render.CreateBrush(250, 180, 180, 180, 1);

            TimeLeftClockBrush = Hud.Render.CreateBrush(220, 0, 0, 0, 0);

			FontText = Hud.Render.CreateFont("tahoma", 9f, 255, 255, 255, 255, true, false, 128, 0, 0, 0, true);

			PlayerSnoEnumMale = new List<ActorSnoEnum>
			{
				ActorSnoEnum._demonhunter_male,	// 75207	//dh
				ActorSnoEnum._wizard_male,		// 6544		//wiz
				ActorSnoEnum._witchdoctor_male,	// 6485		//wd
				ActorSnoEnum._barbarian_male,	// 3301		//barb
				ActorSnoEnum._monk_male,		// 4721		//monk
				ActorSnoEnum._x1_crusader_male,	// 238284	//crus
				ActorSnoEnum._p6_necro_male,	// 454021	//necro
			};

			CloneSno = new Dictionary<ActorSnoEnum, Tuple<HeroClass,bool>>  // Class , IsMale
			{
				{ ActorSnoEnum._seasontheme_barbarian_male,		new Tuple<HeroClass,bool>(HeroClass.Barbarian,true)		},	{ ActorSnoEnum._seasontheme_barbarian_female,	new Tuple<HeroClass,bool>(HeroClass.Barbarian,false) },
				{ ActorSnoEnum._seasontheme_crusader_male,		new Tuple<HeroClass,bool>(HeroClass.Crusader,true)		},	{ ActorSnoEnum._seasontheme_crusader_female,	new Tuple<HeroClass,bool>(HeroClass.Crusader,false) },
				{ ActorSnoEnum._seasontheme_demonhunter_male,	new Tuple<HeroClass,bool>(HeroClass.DemonHunter,true)	},	{ ActorSnoEnum._seasontheme_demonhunter_female,	new Tuple<HeroClass,bool>(HeroClass.DemonHunter,false) },
				{ ActorSnoEnum._seasontheme_monk_male,			new Tuple<HeroClass,bool>(HeroClass.Monk,true)			}, 	{ ActorSnoEnum._seasontheme_monk_female,		new Tuple<HeroClass,bool>(HeroClass.Monk,false) },
				{ ActorSnoEnum._seasontheme_necromancer_male,	new Tuple<HeroClass,bool>(HeroClass.Necromancer,true)	},	{ ActorSnoEnum._seasontheme_necromancer_female,	new Tuple<HeroClass,bool>(HeroClass.Necromancer,false) },
				{ ActorSnoEnum._seasontheme_witchdoctor_male,	new Tuple<HeroClass,bool>(HeroClass.WitchDoctor,true)	},	{ ActorSnoEnum._seasontheme_witchdoctor_female,	new Tuple<HeroClass,bool>(HeroClass.WitchDoctor,false) },
				{ ActorSnoEnum._seasontheme_wizard_male,		new Tuple<HeroClass,bool>(HeroClass.Wizard,true)		},	{ ActorSnoEnum._seasontheme_wizard_female,		new Tuple<HeroClass,bool>(HeroClass.Wizard,false)	},
			};

			HeroBrush = new Dictionary<HeroClass, IBrush>()
			{
				{ HeroClass.Barbarian,		Hud.Render.CreateBrush(255,237, 20, 20,  0) },
				{ HeroClass.Crusader,		Hud.Render.CreateBrush(255,255,204,  0,  0) },
				{ HeroClass.DemonHunter,	Hud.Render.CreateBrush(255,  0,168,255,  0) },
				{ HeroClass.Monk,			Hud.Render.CreateBrush(255, 35,225,  6,  0) },
				{ HeroClass.Necromancer,	Hud.Render.CreateBrush(255, 64,192,176,  0) },
				{ HeroClass.WitchDoctor,	Hud.Render.CreateBrush(255,255,132,  0,  0) },
				{ HeroClass.Wizard,			Hud.Render.CreateBrush(255,220,150,255,  0) },
				{ HeroClass.None,			Hud.Render.CreateBrush(255,200,200,200,  0) },
			};

			HeroClonNames = new Dictionary<HeroClass, string[]>()
			{
				{ HeroClass.Barbarian,		new string[] { NameBuildDefault,	"1-СЕЙСМИЧЕСКИЙ",	"2-ВИХРЬ",	"3-ПРЫГУН"		}	},
				{ HeroClass.Crusader,		new string[] { NameBuildDefault,	"1-БОМБАРДИРОВКА",	"2-ОСВЯЩЕННЫЙ",	"3-ДЛАНЬ"		}	},
				{ HeroClass.DemonHunter,	new string[] { NameBuildDefault,	"1-КЛИНКИ",	"2-ЗАЛП",	"3-КИНЖАЛ"		}	},
				{ HeroClass.Necromancer,	new string[] { NameBuildDefault,	"1-КРОВЬ",	"2-ТЛЕН",	"3-КОСТИ"		}	},
				{ HeroClass.WitchDoctor,	new string[] { NameBuildDefault,	"1-САРАНЧА",	"2-БОМБА",	"3-ДУХ"		}	},
				{ HeroClass.Wizard,			new string[] { NameBuildDefault,	"1-ХОЛОД",	"2-ОГОНЬ",	"3-МАГИЯ"		}	},
				{ HeroClass.Monk,			new string[] { NameBuildDefault,	"1-ЯРОСТЬ",	"2-ИСПЕПЕЛЕНИЕ",	"3-ВНЕЗАПНОСТЬ"	}	},
				{ HeroClass.None,			new string[] { NameBuildDefault,	"Config1",	"Config2",	"Config3"		}	},	// Para evitar posibles excepciones
			};

			TimerHigh = Hud.Render.CreateBrush(255, 0, 255, 100, 0);
			TimerLow = Hud.Render.CreateBrush(255, 255, 0, 0, 0);
			TimerBg = Hud.Render.CreateBrush(75, 0, 0, 0, 0);

			SkillBorderLight = Hud.Render.CreateBrush(200, 95, 95, 95, 1);
			SkillBorderDark = Hud.Render.CreateBrush(150, 0, 0, 0, 1);

			FontNear = Hud.Render.CreateFont("tahoma", sizeFontBar - 0.5f, 255, 220, 185, 150, false, false, 175, 0, 0, 0, true);

			FontName = Hud.Render.CreateFont("tahoma", sizeFontBar, 255, 180, 147, 109, false, false, true);
			FontNameMe = Hud.Render.CreateFont("tahoma", sizeFontBar, 255, 225, 190, 160, false, false, 175, 0, 0, 0, true);

			FontTextm = Hud.Render.CreateFont("tahoma", sizeFontBar - 2.0f, 255, 255, 255, 255, true, false, 255, 0, 0, 0, true);

			FontTimer = Hud.Render.CreateFont("tahoma", sizeFontBar - 0.5f, 255, 200, 200, 200, false, false, 175, 0, 0, 0, true);
			FontTimerR = Hud.Render.CreateFont("tahoma", sizeFontBar - 0.5f, 255, 255, 150, 150, false, false, 175, 0, 0, 0, true);
			FontTimerG = Hud.Render.CreateFont("tahoma", sizeFontBar - 0.5f, 255, 150, 255, 150, false, false, 175, 0, 0, 0, true);
        }

        private void DrawTimeLeftClock(RectangleF rect, double elapsed, double timeLeft)   // plugins\Default\BuffLists\Painter\BuffPainter.cs
        {
            if ((timeLeft > 0) && (elapsed >= 0) && (TimeLeftClockBrush != null))
            {
                var endAngle = Convert.ToInt32(360.0d / (timeLeft + elapsed) * elapsed);
                var startAngle = 0;
                TimeLeftClockBrush.Opacity = 1 - (float)(0.4f / (timeLeft + elapsed) * elapsed);
                var rad = rect.Width; // * 0.45f;
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

		public void OnMonsterKilled(IMonster monster)
		{
			if (CloneCreated.ContainsKey(monster.AnnId))
			{
				CloneCreated.Remove(monster.AnnId);
			}
		}

		public void OnNewArea(bool newGame, ISnoArea area)
		{
			if (newGame || (MyIndex != Hud.Game.Me.Index) )   // Fix partialment the newGame limitation
			{
				MyIndex = Hud.Game.Me.Index;
				CloneCreated.Clear();
				InRift = false;
			}
 		}

		public void AfterCollect()
        {
			if (!Hud.Game.IsInGame) return;
			if (InRift ^ (Hud.Game.Quests.FirstOrDefault(q => (q.SnoQuest.Sno == 337492) && (q.State == QuestState.started || q.State == QuestState.completed )) != null))
			{
				if (InRift)
				{
					foreach(var clone in CloneCreated)
						if (clone.Value.Rift) clone.Value.Alive = false;
				}
				InRift = !InRift;
			}
		}

		public int CloneGetConfig(IActor actor)
		{
			int number = 0; var heroClass = CloneSno[actor.SnoActor.Sno].Item1;
			switch (heroClass)
			{
				case HeroClass.Barbarian:
						if 		(actor.GetAttributeValue(Hud.Sno.Attributes.Rune_B, 86989) == 1) number = 1; //  Rune_B 86989 1 power: Seismic Slam // Rune_A  98878 1 power: EarthQuake
						else if (actor.GetAttributeValue(Hud.Sno.Attributes.Rune_A, 70472) == 1) number = 2; //  Rune_A 70472 1 power: Rend			// Rune_B 484654 1 power: Whirlwind
						else if (actor.GetAttributeValue(Hud.Sno.Attributes.Rune_C, 93409) == 1) number = 3; //  Rune_C 93409 1 power: Leap			// Rune_E	79242 1 power: Bash
						break;
				case HeroClass.Crusader:
						if 		(actor.GetAttributeValue(Hud.Sno.Attributes.Rune_B,284876) == 1) number = 1; // Rune_B	284876 1 power: Bombardment // Rune_B 239042 1 power: Sweep Attack
						else if (actor.GetAttributeValue(Hud.Sno.Attributes.Rune_D,266951) == 1) number = 2; // Rune_D 266951 1 power: B. Shield	// Rune_C	266766 1 power: B. Hammer
						else if (actor.GetAttributeValue(Hud.Sno.Attributes.Rune_E,239218) == 1) number = 3; // Rune_E 239218 1 power: Fist			// Rune_C 316014 1 power: Fury
						break;
				case HeroClass.DemonHunter:
						if 		(actor.GetAttributeValue(Hud.Sno.Attributes.Rune_B ,77546) == 1) number = 1; // Rune_B	77546	1	power: Fan of Knives
						else if (actor.GetAttributeValue(Hud.Sno.Attributes.Rune_C, 77649) == 1) number = 2; // Rune_C	77649	1	power: Multishot
						else if (actor.GetAttributeValue(Hud.Sno.Attributes.Rune_A,130831) == 1) number = 3; // Rune_A	130831	1	power: Impale Rune  Overpenetration
						break;
				case HeroClass.Necromancer:
						if 		(actor.GetAttributeValue(Hud.Sno.Attributes.Rune_C,462243) == 1) number = 1;  // Rune_C	462243	1 power:  Death Nova - Blood Nova
						else if (actor.GetAttributeValue(Hud.Sno.Attributes.Rune_D,462243) == 1) number = 2;  // Rune_D	462243	1 power:  Death Nova - Blight
						else if (actor.GetAttributeValue(Hud.Sno.Attributes.Rune_B,462243) == 1) number = 3;  // Rune_B	462243	1 power:  Death Nova - Bone Nova
						break;
				case HeroClass.WitchDoctor:
						if 		(actor.GetAttributeValue(Hud.Sno.Attributes.Rune_E,69867) == 1)	number = 1; // Rune_E 69866	1 power: spider 	// Rune_E 69867 1 power: locust
						else if (actor.GetAttributeValue(Hud.Sno.Attributes.Rune_E,67567) == 1) number = 2; // Rune_E 67567	1 power: fire bomb
						else if (actor.GetAttributeValue(Hud.Sno.Attributes.Rune_C,83602) == 1) number = 3; // Rune_C 83602	1 power: haunt
						break;
				case HeroClass.Wizard:
						if 		(actor.GetAttributeValue(Hud.Sno.Attributes.Rune_E,30668) == 1) number = 1;  // Rune_E	30668	1  power: Arcane Orb - Frozen Orb
						else if (actor.GetAttributeValue(Hud.Sno.Attributes.Rune_D,30668) == 1) number = 2;  // Rune_D	30668	1  power: Arcane Orb - Scorch
						else if (actor.GetAttributeValue(Hud.Sno.Attributes.Rune_A,30668) == 1) number = 3;  // Rune_A	30668	1  power: Arcane Orb - Obliteration
						break;
				case HeroClass.Monk:
						if 		(actor.GetAttributeValue(Hud.Sno.Attributes.Rune_E,96694) == 1) number = 1; // Rune_E	96694	1 power: Seven-Sided Strike - Fulminating Onslaught
						else if (actor.GetAttributeValue(Hud.Sno.Attributes.Rune_B,96694) == 1) number = 2; // Rune_B	96694	1 power: Seven-Sided Strike - Incinerate
						else if (actor.GetAttributeValue(Hud.Sno.Attributes.Rune_A,96694) == 1) number = 3; // Rune_A	96694	1 power: Seven-Sided Strike - Sudden Assault
						break;
				default:
						break;
			}
			return number;
		}

		public void PaintTopInGame(ClipState clipState)
		{
			if (clipState != ClipState.BeforeClip) return;
            if (!Hud.Game.IsInGame || Hud.Game.IsInTown) return;

			if ( Hud.Game.Me.Powers.BuffIsActive(484633) )
			{
				foreach (var clone in CloneCreated)
				{
					if (clone.Value.Sighted) clone.Value.Sighted = false;
				}
				var actors = Hud.Game.Actors.Where(a => CloneSno.ContainsKey(a.SnoActor.Sno));
				foreach (var actor in actors)
				{
					if (!CloneCreated.TryGetValue(actor.AnnId, out var info))
					{
						CloneCreated.Add(actor.AnnId,
							new CloneInfo()
							{
								CreatedAtInGameTick =  actor.CreatedAtInGameTick,
								Rift = (Hud.Game.Me.SnoArea.HostAreaSno == 288482) || (Hud.Game.Me.SnoArea.Sno == 288482),
								HeroClass = CloneSno[actor.SnoActor.Sno].Item1,
								MyClone = (actor.SummonerAcdDynamicId == Hud.Game.Me.SummonerId),
								PlayerName = string.Empty,
								SameClassGenre = null
							}
						);
						info = CloneCreated[actor.AnnId];
						info.BuildConfig = CloneGetConfig(actor);
						info.BuildName = HeroClonNames[info.HeroClass][info.BuildConfig];
					}
					if (info.PlayerName == string.Empty || info.SameClassGenre == null)
					{
						var player = Hud.Game.Players.FirstOrDefault(p => p.SummonerId == actor.SummonerAcdDynamicId);
						if (player != null)
						{
							if (info.PlayerName == string.Empty)
								info.PlayerName = player.BattleTagAbovePortrait;
							if (info.SameClassGenre == null)
							{
								if (info.HeroClass != player.HeroClassDefinition.HeroClass)	// Checking Class
									info.SameClassGenre = false;
								else if (player.HasValidActor)								// Checking Genre	// player.HeroIsMale is only for local player (player != Hud.Game.Me => value wrong)
										info.SameClassGenre = (PlayerSnoEnumMale.Contains(player.SnoActor.Sno) == CloneSno[actor.SnoActor.Sno].Item2);
								if (info.SameClassGenre == false && !string.IsNullOrEmpty(SymbolWarning))
									info.PlayerName = SymbolWarning + " " + info.PlayerName;
							}
						}
					}
					info.Sighted = true;
					var x = actor.FloorCoordinate.ToScreenCoordinate().X;
					var y = actor.FloorCoordinate.ToScreenCoordinate().Y;
					if (actor.IsOnScreen)
					{
						DefaultBrush = info.MyClone?BrushBorderMe:(ShowGroundOther?BrushBorderOther:null);
						if (DefaultBrush != null)
						{
							var elapsed = (Hud.Game.CurrentGameTick - info.CreatedAtInGameTick)/60d;
							if (elapsed > duration)	{ elapsed = duration; }
							var timeleft = duration - elapsed;
							var radiusc = radius_countdown / 1200.0f * Hud.Window.Size.Height;

							DefaultBrush.DrawEllipse(x, y, radiusc + 2 , radiusc + 2);

							HeroBrush[info.MyClone?HeroClass.None:info.HeroClass].DrawEllipse(x , y , radiusc , radiusc);
							DrawTimeLeftClock(new RectangleF(x - radiusc/2, y - radiusc/2 , radiusc, radiusc), elapsed , timeleft);

							layout = FontText.GetTextLayout( (timeleft < 0)?"0.0":timeleft.ToString((timeleft > 1)?"F0":"F1") );
							FontText.DrawText(layout, x - layout.Metrics.Width/2 , y - layout.Metrics.Height/2 - 1);

							layout = HeroFont.GetTextLayout(info.BuildName);
							HeroFont.DrawText(layout,x - layout.Metrics.Width / 2 , y + radiusc + 3);
						}
					}
					DefaultBrush = info.MyClone?BrushBorderMapMe:(ShowMapOther?BrushBorderMapOther:null);
					if (DefaultBrush != null)
					{
						Hud.Render.GetMinimapCoordinates(actor.FloorCoordinate.X, actor.FloorCoordinate.Y, out var mapX, out var mapY);
						DefaultBrush.DrawEllipse(mapX,mapY,6 ,6 );
						HeroBrush[info.MyClone?HeroClass.None:info.HeroClass].DrawEllipse(mapX,mapY,3 ,3 );
					}
				}
				var posX = Hud.Window.Size.Width * xPor;	var posY = Hud.Window.Size.Height * yPor;
				var i = 0; bool ShowTotal = true;

				foreach (var clone in CloneCreated.OrderByDescending(c => c.Value.CreatedAtInGameTick).ToArray())
				{
					double t = duration - (Hud.Game.CurrentGameTick - clone.Value.CreatedAtInGameTick)/60d;
					if (clone.Value.Alive)
					{
						if (t < 0)
						{
							CloneCreated[clone.Key].Alive = false;
						}
						else
						{
							if (clone.Value.MyClone || ShowBarOther)
							{
								if (ShowTotal)
								{
									ShowTotal = false;
									layout = FontNear.GetTextLayout("Всего клонов рядом: " + actors.Count() );
									FontNear.DrawText(layout, posX , posY);
									posY += 2 + layout.Metrics.Height;
								}

								TimerBg.DrawRectangle(posX + 3, posY + 3, BarWidth, BarHeight);
								float timeLeftPct = (float)(t / duration);
								if (timeLeftPct > 0)
								{
									TimerLow.DrawRectangle(posX + 3, posY + 3, BarWidth * timeLeftPct, BarHeight);

									TimerHigh.Opacity = timeLeftPct;
									TimerHigh.DrawRectangle(posX + 3, posY + 3, BarWidth * timeLeftPct, BarHeight);
								}
								SkillBorderDark.DrawRectangle(posX + 2, posY + 2, BarWidth + 2, BarHeight + 2);
								SkillBorderLight.DrawRectangle(posX + 1, posY + 1, BarWidth + 4, BarHeight + 4);
								SkillBorderDark.DrawRectangle(posX , posY, BarWidth + 6, BarHeight + 6);

								var y =  posY + (BarHeight + 6)/2.0f;
								if (clone.Value.PlayerName != string.Empty)
								{
									DefaultFont = clone.Value.MyClone?FontNameMe:FontName;
									layout = DefaultFont.GetTextLayout(clone.Value.PlayerName);
									DefaultFont.DrawText(layout, posX - layout.Metrics.Width - 3, y - layout.Metrics.Height/2.0f);
								}

								var x = posX + BarWidth + 9;
								DefaultFont = clone.Value.Sighted?FontTimerG:FontTimerR;
								layout = DefaultFont.GetTextLayout(t.ToString(t < 1 ? "F1" : "F0") + "s");
								DefaultFont.DrawText(layout, x, y - layout.Metrics.Height/2.0f);

								if (ShowBuildNumberBar)
								{
									x += DefaultFont.GetTextLayout("0.0s").Metrics.Width + sizeFontBar + 2;
									layout = FontTextm.GetTextLayout(clone.Value.BuildConfig.ToString());
									HeroBrush[clone.Value.MyClone?HeroClass.None:clone.Value.HeroClass].DrawEllipse(x, y, sizeFontBar, sizeFontBar);
									BrushBorder.DrawEllipse(x, y, sizeFontBar, sizeFontBar);
									FontTextm.DrawText(layout, x - layout.Metrics.Width/2.0f, y - layout.Metrics.Height/2.0f);
								}
								posY += (BarHeight + 6) * 1.2f;
							}
						}
					}
					else
					{
						if (t < -2) { CloneCreated.Remove(clone.Key); }
					}
				}
			}
        }
    }
}